using Sinance.Storage;

namespace Sinance.Business.Tests
{
    public class TestUserIdProvider : IUserIdProvider
    {
        public int CurrentUserId { get; set; }

        public int GetCurrentUserId()
        {
            return CurrentUserId;
        }
    }
}