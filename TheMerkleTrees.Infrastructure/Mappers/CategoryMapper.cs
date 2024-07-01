using TheMerkleTrees.Domain.Models;
using TheMerkleTrees.Infrastructure.Entities;

namespace TheMerkleTrees.Infrastructure.Mappers;

public static class CategoryMapper
{
    public static Category ToDomain (this CategoryEntity entity)
    {
        return new Category
        {
            Id = entity.Id,
            Name = entity.Name,
            Owner = entity.Owner
        };
    }
    
    public static CategoryEntity ToEntity (this Category category)
    {
        return new CategoryEntity
        {
            Id = category.Id,
            Name = category.Name,
            Owner = category.Owner
        };
    }
}