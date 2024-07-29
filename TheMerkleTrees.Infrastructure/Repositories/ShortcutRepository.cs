using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TheMerkleTrees.Domain.Interfaces.Repositories;
using TheMerkleTrees.Domain.Models;
using TheMerkleTrees.Infrastructure.Configurations;
using TheMerkleTrees.Infrastructure.Entities;
using TheMerkleTrees.Infrastructure.Mappers;

namespace TheMerkleTrees.Infrastructure.Repositories;

public class ShortcutRepository : IShortcutRepository
{
    private readonly IMongoCollection<ShortcutEntity> _shortcutsCollection;

    public ShortcutRepository(IOptions<MongoDBSettings> mongoDBSettings)
    {
        var mongoClient = new MongoClient(
            mongoDBSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            mongoDBSettings.Value.DatabaseName);

        _shortcutsCollection = mongoDatabase.GetCollection<ShortcutEntity>("Shortcuts");
    }

    public async Task CreateAsync(Shortcut newShortcut) =>
        await _shortcutsCollection.InsertOneAsync(newShortcut.ToEntity());

     public async Task<List<Shortcut>> GetByUserIdAsync(string userId) =>
        (await _shortcutsCollection.Find(s => s.Owner == userId).ToListAsync())
        .Select(entity => entity.ToDomain())
        .ToList();

    public async Task RemoveAsync(string shortcutName, string userId) =>
        await _shortcutsCollection.DeleteOneAsync(s => s.Owner == userId && s.Name == shortcutName);
}