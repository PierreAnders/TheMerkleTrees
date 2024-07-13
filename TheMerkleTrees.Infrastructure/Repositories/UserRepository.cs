using MongoDB.Driver;
using Microsoft.Extensions.Options;using TheMerkleTrees.Domain.Interfaces.Repositories;
using TheMerkleTrees.Domain.Models;
using TheMerkleTrees.Infrastructure.Configurations;
using TheMerkleTrees.Infrastructure.Mappers;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<UserEntity> _usersCollection;

    public UserRepository(IOptions<MongoDBSettings> mongoDBSettings)
    {
        var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _usersCollection = database.GetCollection<UserEntity>("Users");
    }

    public async Task<User> GetUserByEmailAsync(string email) =>
        (await _usersCollection.Find(user => user.Email == email).FirstOrDefaultAsync())?.ToDomain();

    public async Task CreateUserAsync(User user)
    {
        await _usersCollection.InsertOneAsync(user.ToEntity());
    }
}