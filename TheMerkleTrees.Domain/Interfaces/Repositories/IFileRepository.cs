using TheMerkleTrees.Domain.Models;
using File = TheMerkleTrees.Domain.Models.File;

namespace TheMerkleTrees.Domain.Interfaces.Repositories;

public interface IFileRepository
{
    Task<List<File>> GetAsync();
    Task<File?> GetAsync(string name, string userId);
    Task CreateAsync(File newFile);
    Task UpdateAsync(string id, File updateFile);
    Task RemoveAsync(string name, string userId);
    Task<List<File>> GetFilesByUserAsync(string userId);
    Task<List<File>> GetFilesByUserAndCategoryAsync(string category, string userId);
}