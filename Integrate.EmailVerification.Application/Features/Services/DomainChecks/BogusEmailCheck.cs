using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using StackExchange.Redis;

namespace Integrate.EmailVerification.Application.Features.Services.DomainChecks;

public class BogusEmailCheck : IEmailValidationChecker
{
    private readonly IDatabase _redisdb;
    private readonly IRedisSeeder _redisSeeder;
    private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

    public BogusEmailCheck(IConnectionMultiplexer redis, IRedisSeeder redisSeeder,
        IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
    {
        _redisdb = redis.GetDatabase();
        _redisSeeder = redisSeeder;
        _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
    }

    public string Name => CheckNames.BogusEmailAddress;
    private string Key = ConstantKeys.Bogus;

    public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
    {
        int score = Check.AllotedScore;
        bool passed = true;
        bool valid = true;

        if (!await _redisdb.KeyExistsAsync(Key))
            await _redisSeeder.SeedAsync(Key);

        valid = await IsBogusEmailAddress(records, records.Code, records.DnsStatus);

        if (!valid)
        {
            passed = false;
            score = 0;
        }

        valid = true;
        EmailValidationChecksInfo response = _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);

        return response;
    }

    public async Task<bool> CheckRedis(string value)
    {
        return await _redisdb.SetContainsAsync(Key, value);
    }

    public async Task<bool> IsBogusEmailAddress(RecordsTemplate records, string code, bool dnsStatus)
    {
        if (records.UserName == records.Domain)
        {
            return false;
        }

        var parts = new List<string>();
        parts.AddRange(records.UserName.Split('.'));
        var domainSegments = records.Domain.Split('.');
        for (int i = 0; i < domainSegments.Length - 1; i++) 
        {
            parts.Add(domainSegments[i]);
        }

        foreach (var part in parts)
        {
            if (await CheckRedis(part))
            {
                return false;
            }
        }

        if (records.mxRecords.Count == 0) return false;

        if (code == null || code.StartsWith("5")) return false;

        if (dnsStatus == false) return false;

        return true;
    }
}
