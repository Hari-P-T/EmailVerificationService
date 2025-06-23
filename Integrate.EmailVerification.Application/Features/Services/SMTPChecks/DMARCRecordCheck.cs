using DnsClient;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;
using System.Diagnostics.CodeAnalysis;


namespace Integrate.EmailVerification.Application.Features.Services.SMTPChecks  
{
    [ExcludeFromCodeCoverage]
    public class DMARCRecordCheck : IEmailValidationChecker
    {
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;
    private readonly ILookupClient _dnsClient;

        public string Name => CheckNames.DmarcRecord;
        public DMARCRecordCheck(IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
        {
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }
        private static readonly LookupClient dnsClient = new LookupClient(new LookupClientOptions
        {
            Timeout = TimeSpan.FromSeconds(2),
            Retries = 1
        });
        public virtual async Task<int> EvaluateDmarcPolicyScore(string domain)
        {
            string dmarcDomain = $"_dmarc.{domain}";
            try
            {
                var lookup = new LookupClient();
                var result = await lookup.QueryAsync(dmarcDomain, QueryType.TXT);

                var txtRecords = result.Answers.TxtRecords();
                if (!txtRecords.Any())
                    return 0;
                var txt = txtRecords.FirstOrDefault();
                if(txt == null)
                    return 0;
                string record = string.Join("", txt.Text);

                if (record.StartsWith("v=DMARC1", StringComparison.OrdinalIgnoreCase))
                {
                    var pIndex = record.IndexOf("p=", StringComparison.OrdinalIgnoreCase);
                    if (pIndex == -1)
                    {
                        return 5;
                    }

                    string policy = record.Substring(pIndex + 2).Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].ToLower();

                    return policy switch
                    {
                        "none" => 5,
                        "quarantine" => 7,
                        "reject" => 10,
                        _ => 5
                    };
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            string Domain = records.Domain;
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = true;
            int score1 = await EvaluateDmarcPolicyScore(Domain);
            if (score1 < 5)
            {
            score = 0;
                passed = false;
                score = 0;
            }
            EmailValidationChecksInfo response = _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);
            return response;
        }
    }
}
