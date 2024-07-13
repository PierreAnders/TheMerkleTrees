using TheMerkleTrees.Domain.Models;

namespace TheMerkleTrees.Infrastructure.Mappers;

public static class UserMapper
{
    public static User ToDomain(this UserEntity entity)
    {
        return new User
        {
            Id = entity.Id,
            Email = entity.Email,
            PasswordHash = entity.PasswordHash
        };
    }

    public static UserEntity ToEntity(this User user)
    {
        return new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash
        };
    }
}