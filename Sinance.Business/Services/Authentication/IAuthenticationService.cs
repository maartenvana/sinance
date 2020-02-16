using Sinance.Communication.Model.User;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task<SinanceUserModel> CreateUser(string userName, string password);

        Task<SinanceUserModel> SignIn(string userName, string password);
    }
}