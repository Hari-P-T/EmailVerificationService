using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Models.Templates;

public class RecordsTemplate
{
    public string UserName { get; set; }
    public string ParentDomain { get; set; }
    public string Email { get; set; }
    public string Domain { get; set; }
    public string TLD { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool DnsStatus { get; set; } = false;
    public List<string> mxRecords { get; set; }
    

    public RecordsTemplate(string _userName,string _tLD, string _email, string _domain, string _parentDomain, List<string> _mxRecords)
    {
        UserName = _userName;
        ParentDomain = _parentDomain;
        mxRecords = _mxRecords;
        Email = _email;
        Domain = _domain;
        TLD = _tLD;
    }
}
