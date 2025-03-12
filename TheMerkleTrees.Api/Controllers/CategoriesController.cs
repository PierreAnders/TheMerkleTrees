using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheMerkleTrees.Domain.Interfaces.Repositories;
using TheMerkleTrees.Domain.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace TheMerkleTrees.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet("{categoryName}")]
        public async Task<ActionResult<Category>> Get(string categoryName)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            
            var category = await _categoryRepository.GetAsync(categoryName, userId);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] Category newCategory)
        {
            if (newCategory == null || string.IsNullOrEmpty(newCategory.Name))
            {
                return BadRequest("Invalid category data.");
            }
            
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            newCategory.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            newCategory.Owner = userId;

            await _categoryRepository.CreateAsync(newCategory);
            return CreatedAtAction(nameof(Get), new { id = newCategory.Id }, newCategory);
        }

        [HttpDelete("{categoryName}")]
        [Authorize]
        public async Task<IActionResult> Delete(string categoryName)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            await _categoryRepository.RemoveAsync(categoryName, userId);

            return NoContent();
        }
        
        [Authorize]
        [HttpGet("user")]
        public async Task<ActionResult<List<Category>>> GetCategoriesByUser()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var categories = await _categoryRepository.GetCategoriesByUserAsync(userId);
            return Ok(categories);
        }
    }
}