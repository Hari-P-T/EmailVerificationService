using Integrate.EmailVerification.Models.Response;

namespace Integrate.EmailVerification.Infrastructure.Redis
{
    public interface IRedisCache
    {
        public Task<bool> IsResponseInCache(string key);
        public Task<EmailVerificationResponse> GetResponseFromCache(string key);
        public Task<bool> PutResponseIntoCache(EmailVerificationResponse response);
    }
}
