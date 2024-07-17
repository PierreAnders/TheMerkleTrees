using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TheMerkleTrees.Domain.Interfaces.Repositories;
using TheMerkleTrees.Infrastructure.Configurations;
using TheMerkleTrees.Infrastructure.Entities;
using TheMerkleTrees.Infrastructure.Mappers;
using File = TheMerkleTrees.Domain.Models.File;

namespace TheMerkleTrees.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly IMongoCollection<FileEntity> _filesCollection;

    public FileRepository(IOptions<MongoDBSettings> mongoDBSettings)
    {
        var mongoClient = new MongoClient(
            mongoDBSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            mongoDBSettings.Value.DatabaseName);

        _filesCollection = mongoDatabase.GetCollection<FileEntity>("Files");
    }

    public async Task<List<File>> GetAsync() =>
        (await _filesCollection.Find(_ => true).ToListAsync())
        .Select(entity => entity.ToDomain())
        .ToList();

    public async Task<File?> GetAsync(string name, string userId) => 
        (await _filesCollection.Find(file => file.Name == name && file.Owner == userId).FirstOrDefaultAsync()).ToDomain();

    public async Task CreateAsync(File newFile) =>
        await _filesCollection.InsertOneAsync(newFile.ToEntity());

    public async Task UpdateAsync(string id, File updateFile) =>
        await _filesCollection.ReplaceOneAsync(x => x.Id == id, updateFile.ToEntity());

    public async Task RemoveAsync(string name, string userId) =>
        await _filesCollection.DeleteOneAsync(x => x.Name == name && x.Owner == userId);
        
    public async Task<List<File>> GetFilesByUserAsync(string userId) =>
        (await _filesCollection.Find(file => file.Owner == userId).ToListAsync())
        .Select(entity => entity.ToDomain())
        .ToList();
}