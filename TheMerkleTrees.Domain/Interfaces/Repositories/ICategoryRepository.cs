using TheMerkleTrees.Domain.Models;

namespace TheMerkleTrees.Domain.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAsync();
        Task CreateAsync(Category newCategory);
        Task UpdateAsync(string id, Category updatedCategory);
        Task RemoveAsync(string categoryName, string userId);
        Task<List<Category>> GetCategoriesByUserAsync(string userId);
        Task<Category?> GetAsync(string categoryName, string userId);
    }
}