using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using StackExchange.Redis;

namespace Integrate.EmailVerification.Application.Features.Services.EmailAddress
{
    public class EstablishedCheck : IEmailValidationChecker
    {
        public string Name => CheckNames.Established;

        private readonly IDatabase _redisdb;
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;
        private readonly IRedisSeeder _redisSeeder;

        public EstablishedCheck(IConnectionMultiplexer redis, IRedisSeeder redisSeeder, IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
        {
            _redisdb = redis.GetDatabase();
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
            _redisSeeder = redisSeeder;
        }

        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate record, EmailValidationCheck Check)
        {
            bool valid = true;
            bool passed = true;
            int score = Check.AllotedScore;
            string Key = ConstantKeys.Established;

            if (!string.IsNullOrWhiteSpace(record.Email))
            {
                if (!await _redisdb.KeyExistsAsync(Key))
                {
                    await _redisSeeder.SeedAsync(Key);
                }
                valid = await _redisdb.SetContainsAsync(Key, record.Email);
            }

            if (valid)
            {
                passed = false;
                score = 0;
            }

            valid = true;
            EmailValidationChecksInfo response = _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);
            return response;
        }
    }
}
