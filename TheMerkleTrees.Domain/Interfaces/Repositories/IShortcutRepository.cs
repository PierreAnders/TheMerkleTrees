using TheMerkleTrees.Domain.Models;

namespace TheMerkleTrees.Domain.Interfaces.Repositories;

public interface IShortcutRepository
{
     Task CreateAsync(Shortcut newShortcut);
     Task<List<Shortcut>> GetByUserIdAsync(string userId);
     Task RemoveAsync(string shortcutName, string userId);
}