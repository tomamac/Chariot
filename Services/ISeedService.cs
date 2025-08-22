using Chariot.Entities;

namespace Chariot.Services
{
    public interface ISeedService
    {
        Task<User?> AdminSeedAsync();
    }
}
