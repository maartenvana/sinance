using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sinance.Storage
{
    public class SinanceContextFactory : IDesignTimeDbContextFactory<SinanceContext>
    {
        public SinanceContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SinanceContext>();
            optionsBuilder.UseMySql("server=localhost;port=3307;database=SinanceDev;user=root;password=my-secret-pw;");

            return new SinanceContext(optionsBuilder.Options, new DesignTimeDbUserIdProvider());
        }

        private class DesignTimeDbUserIdProvider : IUserIdProvider
        {
            public int GetCurrentUserId()
            {
                return 0;
            }
        }
    }
}