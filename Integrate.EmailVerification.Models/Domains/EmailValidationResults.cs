using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Integrate.EmailVerification.Models.Domains;

[Index(nameof(Email), IsUnique = false)]
public class EmailValidationResults
{
    [Key]
    public Guid Id { get; set; }

    public Guid RequestId { get; set; }

    [ForeignKey(nameof(RequestId))]
    public Requests Request { get; set; }

    public string Email { get; set; }
    public int StrictnessTypeId { get; set; }

    [ForeignKey(nameof(StrictnessTypeId))]
    public StrictnessTypes StrictnessType { get; set; }

    public int TotalScore { get; set; }
    public string Status { get; set; }
    public Guid? ClientReferenceId { get; set; }
    public ICollection<EmailValidationCheckMappings> EmailValidationCheckMappingsTable { get; set; }
}