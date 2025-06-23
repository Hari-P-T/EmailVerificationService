using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integrate.EmailVerification.Models.Request;

namespace Integrate.EmailVerification.Application.Models.Request
{
    public class BulkEmailVerificationRequest
    {
       
        public  List<EmailVerificationRequest> BulkEmailVerificationList { get; set; }
    }
}
