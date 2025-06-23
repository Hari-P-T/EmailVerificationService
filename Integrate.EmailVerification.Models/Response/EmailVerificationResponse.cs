using Integrate.EmailVerification.Models.Enum;

namespace Integrate.EmailVerification.Models.Response
{
    public class EmailVerificationResponse
    {
        public string Email { get; set; }
        public int Score { get; set; }
        public string Status { get; set; } = EmailValidationStatus.Unknown.ToString();
        public Guid ResultId { get; set; }
        public List<CheckResult> CheckResult { get; set; } = new();
    }

    public class CheckResult
    {
        public string CheckName { get; set; }
        public bool Performed { get; set; }
        public bool Passed { get; set; }
        public int Score { get; set; }
    }
}
