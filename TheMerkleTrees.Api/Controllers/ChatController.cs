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

        [HttpPost("message")]
        public async Task<ActionResult> SendMessage([FromBody] string message)
        {
            // URL locale pour le serveur Ollama
            var baseUrl = "http://localhost:11434/api/chat";

            // Messages à envoyer au modèle DeepSeek
            var messages = new[]
            {
                new { role = "system", content = "You are an artificial intelligence assistant and you need to engage in a helpful, detailed, polite conversation with a user." },
                new { role = "user", content = message }
            };

            // Création de l'objet de la requête
            var requestData = new
            {
                model = "deepseek-r1:7b",
                messages = messages,
                stream = false // Option pour désactiver le streaming des réponses
            };

            // Sérialisation de l'objet de la requête en JSON
            var json = JsonConvert.SerializeObject(requestData);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            // Configuration du client HTTP
            using var client = new HttpClient();

            try
            {
                // Envoi de la requête POST au serveur Ollama
                var response = await client.PostAsync(baseUrl, data);

                // Vérification que la requête a réussi
                response.EnsureSuccessStatusCode();

                // Lecture de la réponse
                var result = await response.Content.ReadAsStringAsync();

                // Affichage de la réponse dans les logs
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
