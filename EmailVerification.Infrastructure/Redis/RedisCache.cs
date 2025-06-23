using Integrate.EmailVerification.Models.Response;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Integrate.EmailVerification.Infrastructure.Redis
{
    [ExcludeFromCodeCoverage]
    public class RedisCache : IRedisCache
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisCache(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<EmailVerificationResponse> GetResponseFromCache(string key)
        {
            var db = _redis.GetDatabase();
            var cachedValue = await db.StringGetAsync(key);
            if (cachedValue.IsNullOrEmpty)
            {
                return new EmailVerificationResponse();
            }
            else
            {
                var response = JsonConvert.DeserializeObject<EmailVerificationResponse>(cachedValue.ToString() ?? string.Empty);
                return response ?? new EmailVerificationResponse();
            }
        }

        public async Task<bool> IsResponseInCache(string key)
        {
            var db = _redis.GetDatabase();
            key = key.ToLower();
            return await db.KeyExistsAsync(key);
        }

        public async Task<bool> PutResponseIntoCache(EmailVerificationResponse response)
        {
            var db = _redis.GetDatabase();
            if(await db.KeyExistsAsync(response.Email.ToLower()))
            {
                return true;
            }
            return await db.StringSetAsync(response.Email.ToLower(), JsonConvert.SerializeObject(response), TimeSpan.FromDays(7));
        }
    }
}
