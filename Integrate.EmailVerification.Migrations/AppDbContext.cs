using Microsoft.EntityFrameworkCore;
using Integrate.EmailVerification.Models.Domains;

namespace Integrate.EmailVerification.Migrations;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<EmailValidationResults> EmailValidationResults { get; set; }
    public DbSet<ValidationChecks> ValidationChecks { get; set; }
    public DbSet<EmailValidationCheckMappings> EmailValidationCheckMappingsTable { get; set; }
    public DbSet<Requests> Requests { get; set; }
    public DbSet<StrictnessTypes> StrictnessTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var systemUser = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var timestamp = new DateTime(2025, 06, 04, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<StrictnessTypes>()
        .Property(st => st.Id)
        .ValueGeneratedNever();

        modelBuilder.Entity<StrictnessTypes>().HasData(
            new StrictnessTypes { Id = 0, Name = "Basic" },
            new StrictnessTypes { Id = 1, Name = "Intermediate" },
            new StrictnessTypes { Id = 2, Name = "Advanced" }
        );

        modelBuilder.Entity<ValidationChecks>().HasData(
            new ValidationChecks {CheckId = Guid.Parse("00000000-0000-0000-0000-000000000001"),CheckName = "UnRecognizedTLD",Description = "Top-level domain is not recognized by ICANN.",Weightage = 10,StrictnessTypeId = 1,IsActive = true,IsDeleted = false,CreatedAt = timestamp,UpdatedAt = timestamp,CreatedBy = systemUser,UpdatedBy = systemUser},
            new ValidationChecks {CheckId = Guid.Parse("00000000-0000-0000-0000-000000000002"),CheckName = "InvalidSyntax",Description = "Email address is syntactically invalid.",Weightage = 10,StrictnessTypeId = 1,IsActive = true,IsDeleted = false,CreatedAt = timestamp,UpdatedAt = timestamp,CreatedBy = systemUser,UpdatedBy = systemUser},
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000003"), CheckName = "InvalidDomainSpecificSyntax", Description = "Email is invalid for the given domain.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000004"), CheckName = "InvalidDNS", Description = "Domain is unregistered or lacks A records.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000005"), CheckName = "NoMXRecords", Description = "Registered DNS does not have an MX record.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000006"), CheckName = "Established", Description = "Email is in known bulk marketing lists.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000007"), CheckName = "Alias", Description = "Email is believed to be an alias.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000008"), CheckName = "Bogus", Description = "Email is likely a bogus.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000009"), CheckName = "BogusSMSAddress", Description = "Email is a bogus SMS domain address.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000010"), CheckName = "Garbage", Description = "Email contains garbage-like strokes or characters.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000011"), CheckName = "Vulgar", Description = "Email contains vulgar words or content.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000012"), CheckName = "MailBoxIsFull", Description = "Mailbox is full and cannot receive messages.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000013"), CheckName = "MailboxIsBusy", Description = "Mailbox is busy and cannot currently accept messages.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000014"), CheckName = "DisposableEmail", Description = "Email is believed to be a disposable address.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000015"), CheckName = "KnownSpammer", Description = "Email is known for spam-like activities.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000016"), CheckName = "BlacklistedDomain", Description = "Domain appears in one or more blacklists.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000017"), CheckName = "KnownGreylister", Description = "Domain server commonly uses greylisting techniques.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000018"), CheckName = "OptInRequired", Description = "Mail server opted in to send/receive emails.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000019"), CheckName = "IsWhiteListOnly", Description = "Given domain is whitelisted.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000020"), CheckName = "ConnectionRefused", Description = "Mail server refuses SMTP connection.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000021"), CheckName = "EmailIsBad", Description = "Critical failure in email verification like SPF/DMARC/DKIM.", Weightage = 10, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser },
            new ValidationChecks { CheckId = Guid.Parse("00000000-0000-0000-0000-000000000022"), CheckName = "IsNotACatchAll", Description = "Checks if the domain has a catch-all mailbox accepting all emails.", Weightage = 0, StrictnessTypeId = 1, IsActive = true, IsDeleted = false, CreatedAt = timestamp, UpdatedAt = timestamp, CreatedBy = systemUser, UpdatedBy = systemUser }
        );
    }
}