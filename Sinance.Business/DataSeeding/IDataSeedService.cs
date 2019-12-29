using Sinance.Storage;
using System.Threading.Tasks;

namespace Sinance.Business.DataSeeding
{
    public interface IDataSeedService
    {
        Task NewUserSeed(IUnitOfWork unitOfWork, int userId);

        Task StartupSeed();
    }
}