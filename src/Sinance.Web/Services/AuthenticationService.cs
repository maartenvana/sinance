using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sinance.Business.DataSeeding;
using Sinance.Business.DataSeeding.Seeds;
using Sinance.Business.Exceptions.Authentication;
using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.Model.User;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Threading.Tasks;

namespace Sinance.Web.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IPasswordHasher<SinanceUserEntity> _passwordHasher;
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;

    public AuthenticationService(
        IPasswordHasher<SinanceUserEntity> passwordHasher,
        IDbContextFactory<SinanceContext> dbContextFactory)
    {
        _passwordHasher = passwordHasher;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<SinanceUserModel> CreateUser(string userName, string password)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var user = await context.Users.SingleOrDefaultAsync(x => x.Username == userName);

        if (user == null)
        {
            var newUser = new SinanceUserEntity()
            {
                Username = userName,
                Password = _passwordHasher.HashPassword(user, password)
            };

            await context.Users.AddAsync(newUser);
            await context.SaveChangesAsync();

            context.OverwriteUserIdProvider(new SeedUserIdProvider(newUser.Id));
            await CategorySeed.SeedStandardCategoriesForUser(context, newUser.Id);

            return newUser.ToDto();
        }
        else
        {
            throw new UserAlreadyExistsException("User already exists");
        }
    }

    public async Task<SinanceUserModel> SignIn(string userName, string password)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var user = await context.Users.SingleOrDefaultAsync(x => x.Username == userName);

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
                await context.SaveChangesAsync();
                return user.ToDto();
            }
            else
            {
                Log.Warning("Incorrect password for user {userName}", userName);
                return null;
            }
        }
        else
        {
            Log.Warning("No user found for username {userName}", userName);
            return null;
        }
    }
}