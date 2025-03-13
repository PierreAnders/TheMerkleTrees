using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TheMerkleTrees.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ChatController(ILogger<ChatController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Envoie un message à l'API de chat et retourne la réponse
        /// </summary>
        /// param name="request">Le message à envoyer</param>
        /// <returns>La réponse de l'API de chat</returns>
        [Authorize]
        [HttpPost("message")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            
            var baseUrl = _configuration["ChatApi:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                _logger.LogError("Chat API base URL is not configured.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Configuration error.");
            }

            var messages = new[]
            {
                new { role = "system", content = "You are an artificial intelligence assistant and you need to engage in a helpful, detailed, polite conversation with a user." },
                new { role = "user", content = request.Message }
            };

            var requestData = new
            {
                model = request.Model,
                messages = messages,
                stream = false
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(60);
                
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(baseUrl, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Chat API returned an error : {StatusCode}", response.StatusCode);
                    return StatusCode((int)response.StatusCode, new ProblemDetails
                    {
                        Title = "Chat API Error",
                        Detail = $"The Chat API returned a {response.StatusCode}"
                    });
                }

                var result = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Chat API response: {Response}", result);

                return Ok(result);
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("The request to the Chat API timed out.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new ProblemDetails
                    {
                        Title = "Service Unavailable",
                        Detail = "The Chat API did not respond in time."
                    });
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Error while sending request to Chat API.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new ProblemDetails
                    {
                        Title = "Service Unavailable",
                        Detail = "Unable to reach the Chat API."
                    });
            }
            catch (JsonException e)
            {
                _logger.LogError(e, "Error deserializing response from Char API.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = "Internal Server Error",
                        Detail = "Error deserializing response from Chat API."
                    });
            }
        }
    }

    public class ChatRequest
    {
        [Required(ErrorMessage="Le message est obligatoire")]
        [StringLength(10000, ErrorMessage = "Le message ne doit pas dépasser {1} caractères")]
        public string Message { get; set; }
        
        [Required(ErrorMessage = "Le model est obligatoire")]
        public string Model { get; set; }
    }
}

// Télécharger les modèles 
// ollama pull mistral
// ollama pull llama3
// ollama pull deepseek-r1
// ollama pull codellama
// ollama pull qwen

// Vérifier les modèles téléchargés
// ollama list

// Lancer le serveur
// ollama serve

// Afficher l'utilisation CPU/mémoire, utilisateur, etc.
// ps aux | grep ollama 
// btop aux | grep ollama 

// Afficher PID, PPID, utilisateur, commande complète.
// ps -ef | grep ollama
// btop -ef | grep ollama

// Couper le serveur
// kill -INT <PID>

// Nom pour les modèles
// Mistral 7B : model = "mistral"
// LLaMA 3 : model = "llama3"
// DeepSeek-R1 7B : model = "deepseek-r1:7b"
// Code Llama 7B : model = "codellama:7b"
// Qwen-1.5 : model = "qwen:1.5"
    

// Installer btop
// brew install btop
// btop    