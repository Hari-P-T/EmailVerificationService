using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

[assembly: ExcludeFromCodeCoverage]

namespace Integrate.EmailVerification.Migrations;

[ExcludeFromCodeCoverage]
public class DbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var config = BuildConfiguration();
        var connectionString = config["EmailVerificationMigrationDatabase"];
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
    private static IConfiguration BuildConfiguration()
    {
        var config = new ConfigurationBuilder();
        config.AddEnvironmentVariables();

        return config.Build();
    }
}