using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebThree.api.Services;

namespace WebThree.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly MongoDBCategoryService _categoryService;

        public CategoriesController(MongoDBCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<List<Category>> Get() =>
            await _categoryService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Category>> Get(string id)
        {
            var category = await _categoryService.GetAsync(id);

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
            await _categoryService.CreateAsync(newCategory);
            return CreatedAtAction(nameof(Get), new { id = newCategory.Id }, newCategory);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Category updatedCategory)
        {
            var category = await _categoryService.GetAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            updatedCategory.Id = category.Id;
            await _categoryService.UpdateAsync(id, updatedCategory);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var category = await _categoryService.GetAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            await _categoryService.RemoveAsync(id);

            return NoContent();
        }
        
        [HttpGet("user/{userId}")]
        public async Task<List<Category>> GetCategoriesByUser(string userId) =>
            await _categoryService.GetCategoriesByUserAsync(userId);
    }
}