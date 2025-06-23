using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Interfaces.Utility;

public interface IEmailHelper
{
    public string GetUserName(string email);
    public string GetTLD(string email);
    public Task<MxRecordsTemplate> GetParentDomain(string email);
    public List<string> GetDomainParts(string email);
    string GetDomain(string email);
    Task<bool> GetDnsStatus(RecordsTemplate record);
}
