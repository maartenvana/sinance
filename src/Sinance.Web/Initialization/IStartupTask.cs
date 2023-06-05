using System.Threading;
using System.Threading.Tasks;

namespace Sinance.Web.Initialization;

public interface IStartupTask
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
