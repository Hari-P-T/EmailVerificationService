
namespace Integrate.EmailVerification.Models.Templates;
/*
 * <Summary>
 * This will be added to the result of each checks -> maps the email and the score.
 */

public class EmailValidationChecksInfo : EmailValidationCheck
{
    public string Email { get; set; }
    public int ObtainedScore { get; set; }
    public bool Performed { get; set; }
    public bool Passed { get; set; }
    public string CheckName { get; set; }

    public EmailValidationChecksInfo(EmailValidationCheck check)
    {
        CheckName = check.Name;
        ObtainedScore = check.AllotedScore;
        Performed = check.Performed;
        Passed = check.Passed;
    }
}



