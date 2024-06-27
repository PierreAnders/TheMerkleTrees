using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace WebThree.api.Services;

public class MongoDBCategoryService
{
    private readonly IMongoCollection<Category> _categoriesCollection;

    public MongoDBCategoryService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
        
        var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
        
        _categoriesCollection = mongoDatabase.GetCollection<Category>("Categories");
    }

    public async Task<List<Category>> GetAsync() =>
        await _categoriesCollection.Find(_ => true).ToListAsync();

    public async Task<Category?> GetAsync(string id) =>
        await _categoriesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Category newCategory) =>
        await _categoriesCollection.InsertOneAsync(newCategory);

    public async Task UpdateAsync(string id, Category updatedCategory) =>
        await _categoriesCollection.ReplaceOneAsync(x => x.Id == id, updatedCategory);

    public async Task RemoveAsync(string id) =>
        await _categoriesCollection.DeleteOneAsync(x => x.Id == id);
    
    public async Task<List<Category>> GetCategoriesByUserAsync(string userId) =>
        await _categoriesCollection.Find(category => category.Owner == userId).ToListAsync();
}