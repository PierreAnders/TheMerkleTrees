using TheMerkleTrees.Infrastructure.Entities;
using File = TheMerkleTrees.Domain.Models.File;

namespace TheMerkleTrees.Infrastructure.Mappers;

public static class FileMapper
{
    public static File ToDomain (this FileEntity entity)
    {
        return new File
        {
            Id = entity.Id,
            Hash = entity.Hash,
            Name = entity.Name,
            Category = entity.Category,
            IsPublic = entity.IsPublic,
            Owner = entity.Owner,
            EncryptionKey = entity.EncryptionKey,
            Key = entity.Key,
            IV = entity.IV,
            Extension = entity.Extension
        };
    }

    public static FileEntity ToEntity(this File file)
    {
        return new FileEntity
        {
            Id = file.Id,
            Hash = file.Hash,
            Name = file.Name,
            Category = file.Category,
            IsPublic = file.IsPublic,
            Owner = file.Owner,
            EncryptionKey = file.EncryptionKey,
            Key = file.Key,
            IV = file.IV,
            Extension = file.Extension
        };
    }
}