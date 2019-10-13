using Sinance.Domain.Entities;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task<SinanceUser> CreateUser(string userName, string password);

        Task<int> GetCurrentUserId();

        Task<SinanceUser> SignIn(string userName, string password);
    }
}