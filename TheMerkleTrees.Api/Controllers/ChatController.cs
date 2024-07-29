using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace TheMerkleTrees.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;

        public ChatController(ILogger<ChatController> logger)
        {
            _logger = logger;
        }

        [HttpPost("sendMessage")]
        public async Task<ActionResult> SendMessage([FromBody] string message)
        {
            var baseUrl = "https://api.perplexity.ai";

            // Clé API
            var apiKey = Environment.GetEnvironmentVariable("API_KEY_PERPLEXITY");

            // Messages à envoyer
            var messages = new[]
            {
                new { role = "system", content = "You are an artificial intelligence assistant and you need to engage in a helpful, detailed, polite conversation with a user." },
                new { role = "user", content = message }
            };

            // Création de l'objet de la requête
            var requestData = new
            {
                model = "mistral-7b-instruct",
                messages = messages
            };

            // Sérialisation de l'objet de la requête en JSON
            var json = JsonConvert.SerializeObject(requestData);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            // Configuration du client HTTP
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Construction de l'URL pour la requête POST
            var url = $"{baseUrl}/chat/completions";

            try
            {
                // Envoi de la requête POST
                var response = await client.PostAsync(url, data);

                // Vérification que la requête a réussi
                response.EnsureSuccessStatusCode();

                // Lecture de la réponse
                var result = await response.Content.ReadAsStringAsync();

                // Affichage de la réponse
                _logger.LogInformation(result);
                return Ok(result);
            }
            catch (HttpRequestException e)
            {
                // Gestion des erreurs
                _logger.LogError($"Request exception: {e.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Erreur lors de l'envoi de la requête.");
            }
        }
    }
}
