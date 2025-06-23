using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks
{
    public interface IMXRecordChecker
    {
        public Task<List<string>> GetMXRecordsAsync(string domain);

        public Task<MxRecordsTemplate> GetParentDomain(string domain);
        
        public Task<bool> HasMXRecords(string domain);

        public Task<SMTPCheckDTO> CheckSingleMXAsync(string email, string domain, string mxRecord);

        public Task SendCommandAsync(Stream stream, string command);

        public Task<string> ReceiveResponseAsync(Stream stream);

        public Task<bool> IsSMTPValid(string email, string domain, List<string> mxRecords);

    }
}
