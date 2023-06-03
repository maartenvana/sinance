using Microsoft.AspNetCore.Http;
using Sinance.Storage;
using System.Linq;

namespace Sinance.Web.Services
{
    public class UserIdProvider : IUserIdProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserIdProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Returns the current logged in user id
        /// </summary>
        public int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.Claims.Single(x => x.Type == "UserId");

            return int.Parse(userIdClaim.Value);
        }
    }
}