using TheMerkleTrees.Domain.Models;

namespace TheMerkleTrees.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> GetUserByEmailAsync(string email);
    Task CreateUserAsync(User user);
}