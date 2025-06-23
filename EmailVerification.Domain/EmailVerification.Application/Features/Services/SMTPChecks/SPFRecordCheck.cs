using DnsClient;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.SMTPChecks
{
    public class SPFRecordCheck : IEmailValidationChecker
    {
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;
        private readonly ILookupClient _dnsClient;

        public SPFRecordCheck(IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory, ILookupClient? dnsClient = null)
        {
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
            _dnsClient = dnsClient ?? new LookupClient();
        }

        public string Name => CheckNames.SpfRecord;

        public async Task<EmailValidationChecksInfo> HasSPFRecord(string domain, EmailValidationCheck check)
        {
            int score = check.AllotedScore;
            bool passed = true;
            bool valid = true;

            passed = await CheckSPFAsync(domain);

            if (!passed)
            {
                score = 0;
            }

            var response = _emailValidationChecksInfoFactory.Create(check, score, passed, valid);
            return response;
        }

        public virtual async Task<bool> CheckSPFAsync(string domain)
        {
            int score = 0;

            var txtRecords = (await _dnsClient.QueryAsync(domain, QueryType.TXT)).Answers.TxtRecords();
            var spfRecord = txtRecords.FirstOrDefault(r => r.Text.Any(t => t.StartsWith("v=spf1")));
            var deprecatedSPF = (await _dnsClient.QueryAsync(domain, (QueryType)99)).Answers;

            if (deprecatedSPF.Count == 0)
            {
                score += 1;
            }

            if (spfRecord != null)
            {
                string record = string.Join("", spfRecord.Text);
                score += 1;

                if (record.Split("v=spf1").Length - 1 <= 1)
                {
                    score += 1;
                }

                string[] mechanisms = record.Split(' ');
                string allMechanism = mechanisms.LastOrDefault(m => m.EndsWith("all"));

                if (allMechanism != null && record.Trim().EndsWith(allMechanism))
                {
                    if (allMechanism.StartsWith("+"))
                        score += 2;
                    else if (allMechanism.StartsWith("~") || allMechanism.StartsWith("?"))
                        score += 1;
                }

                int lookupCount = record.Split(new[] { "include:", "a", "mx", "ptr" }, StringSplitOptions.None).Length - 1;
                if (lookupCount < 10)
                {
                    score += 1;
                }

                if (!record.Contains("ptr"))
                {
                    score += 1;
                }

                score += 1; // SPF record found
            }

            return score >= 5;
        }

        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck check)
        {
            int score = check.AllotedScore;
            int negativeScore = score / 2;
            bool passed = true;
            bool valid = true;

            string domain = records.Domain;
            string parentDomain = records.ParentDomain;

            var domainCheckTask = CheckSPFAsync(domain);
            var parentDomainCheckTask = CheckSPFAsync(parentDomain);
            Console.WriteLine("SPF Check progress");
            await Task.WhenAll(domainCheckTask, parentDomainCheckTask);
            Console.WriteLine("SPF Check done");

            if (!domainCheckTask.Result)
            {
                score -= negativeScore;
            }

            if (!parentDomainCheckTask.Result)
            {
                if (score >= negativeScore)
                {
                    score -= negativeScore;
                }
                else
                {
                    score = 0;
                }
            }

            var response = _emailValidationChecksInfoFactory.Create(check, score, passed, valid);
            return response;
        }
    }
}
