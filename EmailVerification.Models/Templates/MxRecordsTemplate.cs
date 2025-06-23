using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Models.Templates;

public class MxRecordsTemplate
{
    public string ParentDomain { get; set; }
    public List<string> mxRecords { get; set; }

    public MxRecordsTemplate(string _parentDomain, List<string> _mxRecords)
    {
        ParentDomain = _parentDomain;
        mxRecords = _mxRecords;
    }
    public MxRecordsTemplate()
    {
        ParentDomain = string.Empty;
        mxRecords = new List<string>();
    }
}
