using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sinance.Storage;

public class SinanceContextFactory : IDesignTimeDbContextFactory<SinanceContext>
{
    public SinanceContext CreateDbContext(string[] args)
    {
        var connectionString = "server=localhost;port=3307;database=SinanceDev;user=root;password=my-secret-pw;";

        var optionsBuilder = new DbContextOptionsBuilder<SinanceContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new SinanceContext(optionsBuilder.Options, new DesignTimeDbUserIdProvider());
    }

    private sealed class DesignTimeDbUserIdProvider : IUserIdProvider
    {
        public int GetCurrentUserId()
        {
            return 0;
        }
    }
}