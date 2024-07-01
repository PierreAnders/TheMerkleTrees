using TheMerkleTrees.Domain.Models;
using File = TheMerkleTrees.Domain.Models.File;

namespace TheMerkleTrees.Domain.Interfaces.Repositories;

public interface IFileRepository
{
    Task<List<File>> GetAsync();
    Task<File?> GetAsync(string id);
    Task CreateAsync(File newFile);
    Task UpdateAsync(string id, File updateFile);
    Task RemoveAsync(string id);

    Task<List<File>> GetFilesByUserAsync(string userId);
}