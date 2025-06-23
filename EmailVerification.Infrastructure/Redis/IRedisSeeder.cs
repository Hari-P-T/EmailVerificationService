
namespace Integrate.EmailVerification.Infrastructure.Redis
{ 
    public interface IRedisSeeder
    {
        public Task SeedAsync();
        public Task SeedAsync(string key);

    }

}
