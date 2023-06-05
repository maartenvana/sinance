using Microsoft.AspNetCore.Identity;
using Sinance.Business.DataSeeding;
using Sinance.Business.DataSeeding.Seeds;
using Sinance.Business.Exceptions.Authentication;
using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.Model.User;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Threading.Tasks;

namespace Sinance.Web.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly CategorySeed _categorySeed;
    private readonly IPasswordHasher<SinanceUserEntity> _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public AuthenticationService(
        CategorySeed categorySeed,
        IPasswordHasher<SinanceUserEntity> passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _categorySeed = categorySeed;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<SinanceUserModel> CreateUser(string userName, string password)
    {
        
        var user = await _unitOfWork.UserRepository.FindSingle(x => x.Username == userName);

        if (user == null)
        {
            var newUser = new SinanceUserEntity
            {
                Username = userName
            };
            newUser.Password = _passwordHasher.HashPassword(user, password);

            _unitOfWork.UserRepository.Insert(newUser);
            await _unitOfWork.SaveAsync();

            await _categorySeed.SeedStandardCategoriesForUser(_unitOfWork, newUser.Id);

            return newUser.ToDto();
        }
        else
        {
            throw new UserAlreadyExistsException("User already exists");
        }
    }

    public async Task<SinanceUserModel> SignIn(string userName, string password)
    {
        
        var user = await _unitOfWork.UserRepository.FindSingleTracked(x => x.Username == userName);

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
                await _unitOfWork.SaveAsync();
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