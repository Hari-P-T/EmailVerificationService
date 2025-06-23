using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using StackExchange.Redis;


namespace Integrate.EmailVerification.Application.Features.Services.EmailAddress
{
    public class VulgarCheck : IEmailValidationChecker
    {

        private readonly IEmailHelper _emailHelper;
        private readonly IDatabase _redisdb;
        private readonly IRedisSeeder _redisSeeder;
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

        public VulgarCheck(IEmailHelper emailHelper, IRedisSeeder redisSeeder,
            IConnectionMultiplexer redis,
            IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory
            )
        {
            _emailHelper = emailHelper;
            _redisdb = redis.GetDatabase();
            _redisSeeder = redisSeeder;
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }

        public string Name => CheckNames.Vulgar;
        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            string Email = records.Email;
            string userName = records.UserName;
            string Domain = records.Domain;
            string Key = ConstantKeys.VulgarWords;

            if (Check == null)
            {
                throw new ArgumentNullException(nameof(Check));
            }
            if (!await _redisdb.KeyExistsAsync(Key))
            {
                await _redisSeeder.SeedAsync(Key);
            }
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = false;

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(Domain))
            {
                if (!await _redisdb.KeyExistsAsync(Key))
                {
                    await _redisSeeder.SeedAsync(Key);
                }
                valid = await _redisdb.SetContainsAsync(Key, userName) && await _redisdb.SetContainsAsync(Key, Domain);
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
