using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using StackExchange.Redis;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;

namespace Integrate.EmailVerification.Application.Features.Services.UserNameChecks
{
    public class AliasCheck : IEmailValidationChecker
    {
        private readonly IDatabase _redisdb;
        private readonly IEmailHelper _emailHelper;
        private readonly IRedisSeeder _redisSeeder;
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;
        public AliasCheck(
            IConnectionMultiplexer redis,
            IEmailHelper emailHelper,
            IRedisSeeder redisSeeder,
            IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory
)
        {
            _redisdb = redis.GetDatabase();
            _emailHelper = emailHelper;
            _redisSeeder = redisSeeder;
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }
        public string Name => CheckNames.Alias;

        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {

            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = true;
            string userName = _emailHelper.GetUserName(records.Email);
            string Key = ConstantKeys.AliasNames;

            if (!string.IsNullOrWhiteSpace(userName))
            {
                if (!await _redisdb.KeyExistsAsync(Key))
                {
                    await _redisSeeder.SeedAsync(Key);
                }
                valid = await _redisdb.SetContainsAsync(Key, userName);
            }

            if (!valid)
            {
                passed = false;
                score = 0; 
            }

            
            EmailValidationChecksInfo response = _emailValidationChecksInfoFactory.Create(Check, score, passed, true);
            return response;

        }
    }
}
