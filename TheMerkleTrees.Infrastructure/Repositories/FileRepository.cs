using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TheMerkleTrees.Domain.Interfaces.Repositories;
using TheMerkleTrees.Infrastructure.Configurations;
using TheMerkleTrees.Domain.Models;
using File = TheMerkleTrees.Domain.Models.File;

namespace TheMerkleTrees.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly IMongoCollection<File> _filesCollection;

    public FileRepository(IOptions<MongoDBSettings> mongoDBSettings)
    {
        var mongoClient = new MongoClient(
            mongoDBSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            mongoDBSettings.Value.DatabaseName);

        _filesCollection = mongoDatabase.GetCollection<File>("Files");
    }

    public async Task<List<File>> GetAsync() =>
        await _filesCollection.Find(_ => true).ToListAsync();

    public async Task<File?> GetAsync(string id) =>
        await _filesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(File newFile) =>
        await _filesCollection.InsertOneAsync(newFile);

    public async Task UpdateAsync(string id, File updateFile) =>
        await _filesCollection.ReplaceOneAsync(x => x.Id == id, updateFile);

    public async Task RemoveAsync(string id) =>
        await _filesCollection.DeleteOneAsync(x => x.Id == id);

    public async Task<List<File>> GetFilesByUserAsync(string userId) =>
        await _filesCollection.Find(file => file.Owner == userId).ToListAsync();
}