using Integrate.EmailVerification.Api.Middlewares;
using Integrate.EmailVerification.Application.Features.Interfaces;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Infrastructure.Repositories;
using Integrate.EmailVerification.Models.Domains;
using Integrate.EmailVerification.Models.Enum;
using Integrate.EmailVerification.Models.Response;
using Integrate.EmailVerification.Models.Templates;
using System.Net; 

namespace Integrate.EmailVerification.Application.Features.EmailVerification;

public class EmailVerificationHandler : IEmailVerificationHandler
{
    private readonly IEmailHelper _emailHelper;
    private readonly IEmailValidationFactory _emailValidationFactory;
    private readonly IMXRecordChecker _mXRecordChecker;
    private readonly IEmailValidationResultsRepository _emailValidationResultsRepository;
    private readonly IEmailValidationChecksMappingRepository _emailValidationChecksMappingRepository;
    private readonly IValidationChecksRepository _validationChecksRepository;
    private readonly IRedisCache _redisCache;

    private readonly List<string> BasicChecks = new()
    {
        CheckNames.BogusEmailAddress,
        CheckNames.MxRecord,
        CheckNames.DnsValidation,
        CheckNames.ValidTopLevelDomain
    };

    private readonly List<string> UnknownChecks = new()
    {
        CheckNames.CatchAll
    };

    public EmailVerificationHandler(
        IEmailValidationFactory emailValidationFactory,
        IEmailHelper emailHelper,
        IMXRecordChecker mXRecordChecker,
        IEmailValidationResultsRepository emailValidationResultsRepository,
        IValidationChecksRepository validationChecksRepository,
        IEmailValidationChecksMappingRepository emailValidationChecksMappingRepository,
        IRedisCache redisCache)
    {
        _emailValidationFactory = emailValidationFactory;
        _emailHelper = emailHelper;
        _mXRecordChecker = mXRecordChecker;
        _emailValidationResultsRepository = emailValidationResultsRepository;
        _emailValidationChecksMappingRepository = emailValidationChecksMappingRepository;
        _validationChecksRepository = validationChecksRepository;
        _redisCache = redisCache;
    }

    public async Task<EmailVerificationResponse> ValidateEmail(EmailValidationInfo emailValidationInfo)
    {
        try
        {
            var email = emailValidationInfo.Email.Trim().ToLower();
            var requestId = emailValidationInfo.RequestId;
            var resultId = Guid.NewGuid();

            var checks = await _validationChecksRepository.RetrieveAllValidationChecks();
            if (checks == null || !checks.Any())
                throw new NullFoundException("No validation checks found in the database");

            EmailVerificationResponse response;
            List<CheckResult> checkResults;

        if (await _redisCache.IsResponseInCache(email))
        {
            
            var cachedResponse = await _redisCache.GetResponseFromCache(email);
            checkResults = cachedResponse.CheckResult;

            response = new EmailVerificationResponse
            {
                Email = email,
                CheckResult = checkResults,
                ResultId = resultId,
                Score = cachedResponse.Score,
                Status = cachedResponse.Status
            };
        }
        else
        {
                checkResults = await RunAllCheckStrategies(email, checks);
                var (finalStatus, finalScore) = EvaluateScore(checkResults, checks.Count);

            response = new EmailVerificationResponse
            {
                Email = email,
                CheckResult = checkResults,
                ResultId = resultId,
                Score = finalScore,
                Status = finalStatus
            };

            await _redisCache.PutResponseIntoCache(response);
        }


        await PersistValidationResult(emailValidationInfo, resultId, response.Score, response.Status);
        var mappings = MapValidationChecks(checkResults, checks, resultId, emailValidationInfo);
        await SaveValidationMappings(mappings);

            return response;
        }
        catch (NullFoundException)
        {
            throw;
        }
        catch (MethodFailException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }

        async Task<EmailVerificationResponse> CreateEmptyResult(string email, Guid requestId, Guid resultId)
        {
            await _emailValidationResultsRepository.CreateEmailValidationResults(new List<EmailValidationResults>
            {
                    new EmailValidationResults
                    {
                        Id = resultId,
                        Email = email,
                        StrictnessTypeId = (int)emailValidationInfo.Strictness,
                        TotalScore = 0,
                        Status = EmailValidationStatus.Unknown.ToString(),
                        RequestId = requestId
                    }
            });

            return new EmailVerificationResponse
            {
                Email = email,
                ResultId = resultId,
                Score = 0,
                Status = EmailValidationStatus.Unknown.ToString()
            };
        }

        async Task<List<CheckResult>> RunAllCheckStrategies(string email, List<ValidationChecks> checks)
        {
            var domain = _emailHelper.GetDomain(email).ToLower();
            var tld = _emailHelper.GetTLD(domain).ToLower();
            var mxTemplate = await _mXRecordChecker.GetParentDomain(domain) ?? throw new NullFoundException($"No MX record template found for domain '{domain}'.");
            mxTemplate.mxRecords ??= [];
            var userName = _emailHelper.GetUserName(email).ToLower();
            var records = new RecordsTemplate(userName, tld, email, domain, mxTemplate.ParentDomain, mxTemplate.mxRecords);
            records.Code = _mXRecordChecker.CheckSingleMXAsync(email, domain, mxTemplate.mxRecords.FirstOrDefault("")).Result.Code;
            records.DnsStatus = await _emailHelper.GetDnsStatus(records);
            Console.WriteLine(records.mxRecords.Count + "<---------- mx records count");
            var tasks = checks.Select(async check =>
            {
                try
                {
                    var validator = _emailValidationFactory.GetValidator(check.CheckName);
                    if (validator == null)
                    {
                        return new CheckResult
                        {
                            CheckName = check.CheckName,
                            Passed = false,
                            Performed = false,
                            Score = 0
                        };
                    }
                    var result = await validator.EmailCheckValidator(records, new()
                    {
                        AllotedScore = check.Weightage,
                        Name = check.CheckName,
                        CheckId = check.CheckId
                    });
                    return new CheckResult
                    {
                        CheckName = check.CheckName,
                        Passed = result.Passed,
                        Performed = result.Performed,
                        Score = result.ObtainedScore
                    };
                }
                catch (Exception ex)
                {
                    throw new NullFoundException($"Validator not found for check '{check.CheckName}': {ex.Message}");
                }
            });

            var results = await Task.WhenAll(tasks);

            var response = results.Where(r => r != null).ToList();
            return response;
        }

        (string status, int score) EvaluateScore(List<CheckResult> results, int totalChecks)
        {
            int score = 0;
            string status = EmailValidationStatus.Valid.ToString();
            foreach (var result in results)
            {
                if (BasicChecks.Contains(result.CheckName) && !result.Passed)
                {
                    return (EmailValidationStatus.Invalid.ToString(), 0);
                }
                if (UnknownChecks.Contains(result.CheckName) && result.Passed)
                {
                    return new(EmailValidationStatus.Unknown.ToString(), 0);
                }
                score += result.Score;
            }
            
            if (score > 0 && totalChecks > 1)
                score = (score / (totalChecks - 1)) * 10;

            if (score < 70)
                status = EmailValidationStatus.Invalid.ToString();

            return (status, score);
        }

        async Task PersistValidationResult(EmailValidationInfo info, Guid resultId, int score, string status)
        {
            var inserted = await _emailValidationResultsRepository.CreateEmailValidationResults(
            [
                new EmailValidationResults
                {
                    Id = resultId,
                    Email = info.Email,
                    StrictnessTypeId = (int)info.Strictness,
                    TotalScore = score,
                    Status = status,
                    RequestId = info.RequestId
                }
            ]);

            if (!inserted)
                throw new MethodFailException("Failed to insert EmailValidationResults. Check for FK violations or DB issues.");
        }

        List<EmailValidationCheckMappings> MapValidationChecks(List<CheckResult> results, List<ValidationChecks> checks, Guid resultId, EmailValidationInfo info)
        {
            
            return results.Select(result =>
            {
                var check = checks.FirstOrDefault(c => c.CheckName == result.CheckName);
                if (check == null)
                    throw new NullFoundException($"No matching check found for {result.CheckName}");

                return new EmailValidationCheckMappings
                {
                    Id = Guid.NewGuid(),
                    EmailValidationResultId = resultId,
                    CheckId = check.CheckId,
                    IsValid = result.Passed,
                    ObtainedScore = result.Score,
                    CreatedBy = info.CreatedBy,
                    CreatedAt = info.CreatedAt
                };
            }).ToList();
        }

        async Task SaveValidationMappings(List<EmailValidationCheckMappings> mappings)
        {
            if (!mappings.Any())
                throw new NullFoundException("No valid mappings could be created. FK violation likely due to missing CheckId references.");

            var inserted = await _emailValidationChecksMappingRepository.AddEmailValidationCheckMapping(mappings);
            if (!inserted)
                throw new MethodFailException("Failed to insert EmailValidationCheckMappings. Check for FK violations or DB issues.");
        }
    }
}