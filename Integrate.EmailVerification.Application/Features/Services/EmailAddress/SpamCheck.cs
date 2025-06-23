using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Models.Templates;
using Integrate.EmailVerification.Infrastructure.Redis;
using StackExchange.Redis;

namespace Integrate.EmailVerification.Application.Features.Services.EmailAddress
{
    public class SpamCheck : IEmailValidationChecker
    {
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;
        private readonly IRedisSeeder _redisSeeder;
        private readonly IDatabase _redisdb;
        public SpamCheck(IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory, IRedisSeeder redisSeeder, IConnectionMultiplexer redis)
        {
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
            _redisSeeder = redisSeeder;
            _redisdb = redis.GetDatabase();
        }

        public string Name => CheckNames.SpamDomain;

        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = true;
            string Email = records.Email;
            string Key = ConstantKeys.Spam;

            if (!string.IsNullOrWhiteSpace(Email))
            {
                if (!await _redisdb.KeyExistsAsync(Key))
                {
                    await _redisSeeder.SeedAsync(Key);
                }
                valid = await _redisdb.SetContainsAsync(Key, Email);
            }

            if (valid)
            {
                passed = false;
                score = 0;
            }


            EmailValidationChecksInfo response = _emailValidationChecksInfoFactory.Create(Check, score, passed, true);
            return response;
        }
    }
}
