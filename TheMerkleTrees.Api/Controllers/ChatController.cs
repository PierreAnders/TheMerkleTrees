using System.ComponentModel.DataAnnotations;
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

        [Authorize]
        [HttpPost("message")]
        public async Task<ActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
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
                model = "deepseek-r1:7b",
                messages = messages,
                stream = false
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(baseUrl, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Chat API returned an error: {StatusCode}", response.StatusCode);
                    return StatusCode((int)response.StatusCode, "Error from Chat API.");
                }

                var result = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Chat API response: {Response}", result);

                return Ok(JsonSerializer.Deserialize<object>(result));
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Error while sending request to Chat API.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Unable to reach the Chat API.");
            }
        }
    }

    public class ChatRequest
    {
        [Required]
        public string Message { get; set; }
    }
}
