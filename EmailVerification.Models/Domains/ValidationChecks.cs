using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Integrate.EmailVerification.Models.Domains;
[Index(nameof(CheckName), IsUnique = true)]

[ExcludeFromCodeCoverage]
public class ValidationChecks
{
    [Key]
    public Guid CheckId { get; set; }

    public string CheckName { get; set; }
    public string Description { get; set; }
    public int Weightage { get; set; }

    // FK to StrictnessTypes
    public int StrictnessTypeId { get; set; }

    [ForeignKey(nameof(StrictnessTypeId))]
    public StrictnessTypes StrictnessType { get; set; }

    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }

    public ICollection<EmailValidationCheckMappings> EmailValidationCheckMappingsTable { get; set; }
}