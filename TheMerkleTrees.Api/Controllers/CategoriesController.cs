using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheMerkleTrees.Infrastructure;
using TheMerkleTrees.Domain;
using TheMerkleTrees.Domain.Interfaces.Repositories;
using TheMerkleTrees.Domain.Models;
using TheMerkleTrees.Infrastructure.Repositories;

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

        [HttpGet]
        public async Task<List<Category>> Get() =>
            await _categoryRepository.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Category>> Get(string id)
        {
            var category = await _categoryRepository.GetAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Category newCategory)
        {
            if (newCategory == null || string.IsNullOrEmpty(newCategory.Name) || string.IsNullOrEmpty(newCategory.Owner))
            {
                return BadRequest("Invalid category data.");
            }

            newCategory.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            await _categoryRepository.CreateAsync(newCategory);
            return CreatedAtAction(nameof(Get), new { id = newCategory.Id }, newCategory);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Category updatedCategory)
        {
            var category = await _categoryRepository.GetAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            updatedCategory.Id = category.Id;
            await _categoryRepository.UpdateAsync(id, updatedCategory);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var category = await _categoryRepository.GetAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            await _categoryRepository.RemoveAsync(id);

            return NoContent();
        }
        
        [HttpGet("user/{userId}")]
        public async Task<List<Category>> GetCategoriesByUser(string userId) =>
            await _categoryRepository.GetCategoriesByUserAsync(userId);
    }
}