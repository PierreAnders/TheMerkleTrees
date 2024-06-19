using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace WebThree.api.Services;

public class MongoDBService
{
    private readonly IMongoCollection<File> _filesCollection;

    public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        var mongoClient = new MongoClient(
            mongoDBSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            mongoDBSettings.Value.DatabaseName);

        _filesCollection = mongoDatabase.GetCollection<File>("Files");
    }

    public async Task<List<File>> GetAsync() =>
        await _filesCollection.Find(_ => true).ToListAsync();

    public async Task<File?> GetAsync(string hash) =>
        await _filesCollection.Find(x => x.Hash == hash).FirstOrDefaultAsync();

    public async Task CreateAsync(File newFile) =>
        await _filesCollection.InsertOneAsync(newFile);

    public async Task UpdateAsync(string hash, File updateFile) =>
        await _filesCollection.ReplaceOneAsync(x => x.Hash == hash, updateFile);

    public async Task RemoveAsync(string hash) =>
        await _filesCollection.DeleteOneAsync(x => x.Hash == hash);
}