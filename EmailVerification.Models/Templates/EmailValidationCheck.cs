
namespace Integrate.EmailVerification.Models.Templates
{
    public class EmailValidationCheck
    {
        public Guid CheckId { get; set; }
        public string Name { get; set; }
        public int AllotedScore { get; set; }
        public bool Passed { get; set; }
        public bool Performed { get; set; }

    }
}
