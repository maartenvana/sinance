using System.Threading.Tasks;

namespace Sinance.Business.DataSeeding;

public interface IDataSeedService
{
    Task NewUserSeed(int userId);

    Task StartupSeed();
}