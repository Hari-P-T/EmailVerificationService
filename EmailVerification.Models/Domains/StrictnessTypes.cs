using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Integrate.EmailVerification.Models.Domains;
[ExcludeFromCodeCoverage]
public class StrictnessTypes
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<ValidationChecks> ValidationChecks { get; set; }
    public ICollection<EmailValidationResults> EmailValidationResults { get; set; }
}