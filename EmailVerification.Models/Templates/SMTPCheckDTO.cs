
namespace Integrate.EmailVerification.Models.Templates
{
    public class SMTPCheckDTO
    {
        public bool SmtpCheckValid { get; set; }
        public string Code { get; set; } = "";
    }
}
