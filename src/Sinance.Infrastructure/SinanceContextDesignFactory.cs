using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sinance.Infrastructure;

public class SinanceContextDesignFactory : IDesignTimeDbContextFactory<SinanceContext>
{
    public SinanceContext CreateDbContext(string[] args)
    {
        var connectionString = "server=localhost;port=3307;database=SinanceDev;user=root;password=my-secret-pw;";

        var optionsBuilder = new DbContextOptionsBuilder<SinanceContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new SinanceContext(optionsBuilder.Options, new NoMediator(), new DesignTimeDbUserIdProvider());
    }

    private sealed class DesignTimeDbUserIdProvider : IUserIdProvider
    {
        public int GetCurrentUserId()
        {
            return 0;
        }
    }


    class NoMediator : IMediator
    {
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<object> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default(CancellationToken)) where TNotification : INotification
        {
            return Task.CompletedTask;
        }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult<TResponse>(default(TResponse));
        }

        public Task<object> Send(object request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(object));
        }
    }
}