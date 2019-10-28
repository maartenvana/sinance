using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Sinance.Business.DataSeeding;
using Sinance.Business.Exceptions.Authentication;
using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.Model.User;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Web.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly Lazy<IDataSeedService> _dataSeedService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPasswordHasher<SinanceUserEntity> _passwordHasher;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public bool IsLoggedIn
        {
            get { return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated; }
        }

        public AuthenticationService(
            Lazy<IDataSeedService> dataSeedService,
            IPasswordHasher<SinanceUserEntity> passwordHasher,
            IHttpContextAccessor httpContextAccessor,
            Func<IUnitOfWork> unitOfWork)
        {
            _dataSeedService = dataSeedService;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<SinanceUserModel> CreateUser(string userName, string password)
        {
            using var unitOfWork = _unitOfWork();
            var user = await unitOfWork.UserRepository.FindSingle(x => x.Username == userName);

            if (user == null)
            {
                var newUser = new SinanceUserEntity
                {
                    Username = userName
                };
                newUser.Password = _passwordHasher.HashPassword(user, password);

                unitOfWork.UserRepository.Insert(newUser);
                await unitOfWork.SaveAsync();

                await _dataSeedService.Value.NewUserSeed(newUser.Id);

                return newUser.ToDto();
            }
            else
            {
                throw new UserAlreadyExistsException("User already exists");
            }
        }

        /// <summary>
        /// Returns the current logged in user id
        /// </summary>
        public Task<int> GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.Claims.Single(x => x.Type == "UserId");

            return Task.FromResult(int.Parse(userIdClaim.Value));
        }

        public async Task<SinanceUserModel> SignIn(string userName, string password)
        {
            using var unitOfWork = _unitOfWork();
            var user = await unitOfWork.UserRepository.FindSingleTracked(x => x.Username == userName);

            if (user != null)
            {
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                if (passwordVerificationResult == PasswordVerificationResult.Success)
                {
                    return user.ToDto();
                }
                else if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.Password = _passwordHasher.HashPassword(user, password);
                    await unitOfWork.SaveAsync();
                    return user.ToDto();
                }
                else
                {
                    throw new IncorrectPasswordException($"Incorrect password for user {userName}");
                }
            }
            else
            {
                throw new UserNotFoundException($"No user found for username {userName}");
            }
        }
    }
}