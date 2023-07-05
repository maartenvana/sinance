using Sinance.Storage;

namespace Sinance.Business.DataSeeding;

public class SeedUserIdProvider : IUserIdProvider
{
    public int CurrentUserId { get; set; }

    public SeedUserIdProvider(int currentUserId)
    {
        CurrentUserId = currentUserId;
    }

    public int GetCurrentUserId()
    {
        return CurrentUserId;
    }
}