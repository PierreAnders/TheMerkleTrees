﻿using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TheMerkleTrees.Domain.Interfaces.Repositories;
using TheMerkleTrees.Domain.Models;
using TheMerkleTrees.Infrastructure.Configurations;
using TheMerkleTrees.Infrastructure.Entities;
using TheMerkleTrees.Infrastructure.Mappers;

namespace TheMerkleTrees.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<CategoryEntity> _categoriesCollection;

        public CategoryRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(
                mongoDBSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                mongoDBSettings.Value.DatabaseName);

            _categoriesCollection = mongoDatabase.GetCollection<CategoryEntity>("Categories");
        }

        public async Task<List<Category>> GetAsync() =>
            (await _categoriesCollection.Find(_ => true).ToListAsync())
            .Select(entity => entity.ToDomain())
            .ToList();

        public async Task<Category?> GetAsync(string id) =>
            (await _categoriesCollection.Find(x => x.Id == id).FirstOrDefaultAsync())?.ToDomain();

        public async Task CreateAsync(Category newCategory) =>
            await _categoriesCollection.InsertOneAsync(newCategory.ToEntity());

        public async Task UpdateAsync(string id, Category updateCategory) =>
            await _categoriesCollection.ReplaceOneAsync(x => x.Id == id, updateCategory.ToEntity());

        public async Task RemoveAsync(string id) =>
            await _categoriesCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<List<Category>> GetCategoriesByUserAsync(string userId) =>
            (await _categoriesCollection.Find(category => category.Owner == userId).ToListAsync())
            .Select(entity => entity.ToDomain())
            .ToList();
    }
}