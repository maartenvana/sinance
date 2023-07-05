using Sinance.Communication.Model.User;
using Sinance.Storage.Entities;

namespace Sinance.Business.Extensions;

public static class SinanceUserExtensions
{
    public static SinanceUserModel ToDto(this SinanceUserEntity entity)
    {
        return new SinanceUserModel
        {
            Id = entity.Id,
            Username = entity.Username
        };
    }
}