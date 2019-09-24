using Sinance.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using Sinance.Storage;
using Sinance.Business.Exceptions.Authentication;
using System;

namespace Sinance.Business.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPasswordHasher<SinanceUser> _passwordHasher;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public bool IsLoggedIn
        {
            get { return this._httpContextAccessor.HttpContext.User.Identity.IsAuthenticated; }
        }

        public AuthenticationService(
            IPasswordHasher<SinanceUser> passwordHasher,
            IHttpContextAccessor httpContextAccessor,
            Func<IUnitOfWork> unitOfWork)
        {
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<SinanceUser> CreateUser(string userName, string password)
        {
            using (var unitOfWork = _unitOfWork())
            {
                var user = unitOfWork.UserRepository.FindSingle(x => x.Username == userName);

                if (user == null)
                {
                    var newUser = new SinanceUser
                    {
                        Username = userName,
                        UserId = Guid.NewGuid().ToString()
                    };
                    newUser.Password = _passwordHasher.HashPassword(user, password);

                    unitOfWork.UserRepository.Insert(newUser);
                    await unitOfWork.SaveAsync();

                    return newUser;
                }
                else
                {
                    throw new UserAlreadyExistsException("User already exists");
                }
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

        public SinanceUser SignIn(string userName, string password)
        {
            using (var unitOfWork = _unitOfWork())
            {
                var user = unitOfWork.UserRepository.FindSingle(x => x.Username == userName);

                if (user != null)
                {
                    var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                    if (passwordVerificationResult == PasswordVerificationResult.Success)
                    {
                        return user;
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
}