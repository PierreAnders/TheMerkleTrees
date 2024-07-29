using TheMerkleTrees.Domain.Models;
using TheMerkleTrees.Infrastructure.Entities;

namespace TheMerkleTrees.Infrastructure.Mappers;

public static class ShortcutMapper
{
    public static Shortcut ToDomain(this ShortcutEntity entity)
    {
        return new Shortcut
        {
            Id = entity.Id,
            Name = entity.Name,
            Url = entity.Url,
            Owner = entity.Owner
        };
    }

    public static ShortcutEntity ToEntity(this Shortcut shortcut)
    {
        return new ShortcutEntity
        {
            Id = shortcut.Id,
            Name = shortcut.Name,
            Url = shortcut.Url,
            Owner = shortcut.Owner
        };
    }
}