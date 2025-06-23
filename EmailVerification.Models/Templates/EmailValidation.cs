using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integrate.EmailVerification.Models.Enum;


namespace Integrate.EmailVerification.Models.Templates;

public class EmailValidation
{
    public string Email { get; set; }
    public EStrictness Strictness { get; set; } = EStrictness.Basic;
    //public int TimeOut { get; set; }
}

public class EmailValidationInfo : EmailValidation
{
    public Guid RequestId { get; set; }

    public Guid CreatedBy { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
