using System.ComponentModel.DataAnnotations;

namespace Integrate.EmailVerification.Models.Domains;
public class Requests
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public ICollection<EmailValidationResults> EmailValidationResults { get; set; }
}