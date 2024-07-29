using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheMerkleTrees.Domain.Models;
using System.Security.Claims;
using TheMerkleTrees.Domain.Interfaces.Repositories;

namespace TheMerkleTrees.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShortcutController : ControllerBase
    {
        private readonly IShortcutRepository _shortcutRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ShortcutController> _logger;

        public ShortcutController(IShortcutRepository shortcutRepository, IConfiguration configuration, ILogger<ShortcutController> logger)
        {
            _shortcutRepository = shortcutRepository;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] ShortcutRequest shortcutRequest)
        {
            if (shortcutRequest == null || string.IsNullOrEmpty(shortcutRequest.Name) || string.IsNullOrEmpty(shortcutRequest.Url))
            {
                return BadRequest("Invalid shortcut data.");
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var newShortcut = new Shortcut
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Name = shortcutRequest.Name,
                Url = shortcutRequest.Url,
                Owner = userId
            };

            await _shortcutRepository.CreateAsync(newShortcut);

            return CreatedAtAction(nameof(Post), new { id = newShortcut.Id }, newShortcut);
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var shortcuts = await _shortcutRepository.GetByUserIdAsync(userId);
            return Ok(shortcuts);
        }

        [HttpDelete("user/{shortcutName}")]
        [Authorize]
        public async Task<IActionResult> Delete(string shortcutName)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            await _shortcutRepository.RemoveAsync(shortcutName, userId);

            return NoContent();
        }
    }
}