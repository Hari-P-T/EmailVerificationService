using DnsClient;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.SMTPChecks  
{
    public class DKIMRecordCheck : IEmailValidationChecker
    {

        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

        private static readonly LookupClient dnsClient = new LookupClient(new LookupClientOptions
        {
            Timeout = TimeSpan.FromSeconds(2),
            Retries = 1
        });
        public string Name => CheckNames.DkimRecord;


        private static readonly Dictionary<string, List<string>> ProviderSelectorMap = new()
        {
            ["google.com"] = new() { "google", "20230601" },
            ["gmail.com"] = new() { "google", "20230601" },
            ["outlook.com"] = new() { "selector1", "selector2" },
            ["office365.com"] = new() { "selector1", "selector2" },
            ["sendgrid.net"] = new() { "s1", "s2", "sendgrid" },
            ["mailgun.org"] = new() { "mg", "smtp" },
            ["zoho.com"] = new() { "zoho", "zmail", "1522905413783" },
            ["amazonaws.com"] = new() { "mail", "amazonses", "eaxkvsyelrnxjh4cicqyjjmtjpetuwjx" },
            ["sparkpostmail.com"] = new() { "s1", "s2" },
            ["mandrillapp.com"] = new() { "mandrill" },
            ["postmarkapp.com"] = new() { "pm", "smtp" },
            ["fastmail.com"] = new() { "k1" },
            ["brevo.com"] = new() { "br" },
            
        };
        public DKIMRecordCheck( IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory
)
        {
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }


        public async Task<bool> HasAnyDkimRecord(string parentDomain)
        {
            var selectors = ProviderSelectorMap.TryGetValue(parentDomain, out var mappedSelectors) ? mappedSelectors : new()   { "default" };

            var tasks = selectors.Select(async selector =>
            {
                string dkimDomain = $"{selector}._domainkey.{parentDomain}";
                try
                {
                    var result = await dnsClient.QueryAsync(dkimDomain, QueryType.TXT);
                    var txtRecord = result.Answers.TxtRecords().FirstOrDefault();
                    if (txtRecord != null)
                    {
                        string recordValue = string.Join("", txtRecord.Text);
                        if (recordValue.Contains("v=DKIM1", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    Console.WriteLine($"[Error] Failed to query DKIM record for {dkimDomain}");
                }
                return false;
            });

            var results = await Task.WhenAll(tasks);
            return results.Any(r => r);
        }

        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = true;
            string ParentDomain = records.ParentDomain;
            passed = await HasAnyDkimRecord(ParentDomain);
            if (!passed)
            {
                score = 0;
            }
            else
            {
                score = Check.AllotedScore;
            }
            var response = _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);
            return response;
        }
    }
}
