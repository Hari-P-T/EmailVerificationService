using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using StackExchange.Redis;

namespace Integrate.EmailVerification.Application.Features.Services.DomainChecks
{
    public class DisposableDomainCheck : IEmailValidationChecker
    {

        private readonly IDatabase _redisdb;
        private readonly IRedisSeeder _redisSeeder;
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

        public DisposableDomainCheck(IConnectionMultiplexer redis, IRedisSeeder redisSeeder,
            IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory
            )
        {
            _redisdb = redis.GetDatabase();
            _redisSeeder = redisSeeder;
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;

        }

        public string Name => CheckNames.DisposableDomain;
        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = true;
            string Domain = records.Domain;
            string ParentDomain = records.ParentDomain;
            string Key = ConstantKeys.DisposableDomains;

            if (!await _redisdb.KeyExistsAsync(Key))
            {
                await _redisSeeder.SeedAsync(Key);
            }

            if (!string.IsNullOrWhiteSpace(ParentDomain))
            {
                valid = await _redisdb.SetContainsAsync(Key, ParentDomain);
            }
            if (valid)
            {
                passed = false;
                score = 0;
            }

            if (!string.IsNullOrWhiteSpace(Domain))
            {
                valid = await _redisdb.SetContainsAsync(Key, Domain);
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
