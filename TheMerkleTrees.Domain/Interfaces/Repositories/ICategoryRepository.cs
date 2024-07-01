using TheMerkleTrees.Domain.Models;

namespace TheMerkleTrees.Domain.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAsync();
        Task<Category?> GetAsync(string id);
        Task CreateAsync(Category newCategory);
        Task UpdateAsync(string id, Category updatedCategory);
        Task RemoveAsync(string id);
        Task<List<Category>> GetCategoriesByUserAsync(string userId);
    }
}