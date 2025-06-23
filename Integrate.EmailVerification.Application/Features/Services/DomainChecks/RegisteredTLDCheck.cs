using StackExchange.Redis;
using Integrate.EmailVerification.Models.Templates;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Infrastructure.Constant;

namespace Integrate.EmailVerification.Application.Features.Services.DomainChecks
{
    public class RegisteredTLDCheck : IEmailValidationChecker
    {
        private readonly IDatabase _redisdb;
        private readonly IEmailHelper _emailHelper;
        private readonly IRedisSeeder _redisSeeder;
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

        public RegisteredTLDCheck(IConnectionMultiplexer redis, IRedisSeeder redisSeeder,
            IEmailHelper emailHelper,
            IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
        {
            _redisdb = redis.GetDatabase();
            _redisSeeder = redisSeeder;
            _emailHelper = emailHelper;
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }
        public string Name => CheckNames.ValidTopLevelDomain;
        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = false;
            string tld = records.TLD;
            string Key = ConstantKeys.Tlds;

            if (!string.IsNullOrWhiteSpace(tld))
            {
                if (!await _redisdb.KeyExistsAsync(Key))
                {
                    await _redisSeeder.SeedAsync(Key);
                }
                valid = await _redisdb.SetContainsAsync(Key, tld);
            }

            if (!valid)
            {
                passed = false;
                score = 0;
            }
            valid = true;
            return _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);
        }
    }
}
