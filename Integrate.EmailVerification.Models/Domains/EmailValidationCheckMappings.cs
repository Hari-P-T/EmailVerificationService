using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Integrate.EmailVerification.Models.Domains
{
    public class EmailValidationCheckMappings
    {
        [Key]
        public Guid Id { get; set; }

        public Guid EmailValidationResultId { get; set; }
        public Guid CheckId { get; set; }

        public int ObtainedScore { get; set; }
        public bool IsValid { get; set; }

        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        [ForeignKey(nameof(EmailValidationResultId))]
        public EmailValidationResults EmailValidationResult { get; set; }

        [ForeignKey(nameof(CheckId))]
        public ValidationChecks ValidationCheck { get; set; }
    }
}
