using FluentValidation;
using Integrate.EmailVerification.Api;
using Integrate.EmailVerification.Api.Constants;
using Integrate.EmailVerification.Api.Extensions;
using Integrate.EmailVerification.Api.Middlewares;
using Integrate.EmailVerification.Api.Validators;
using Integrate.EmailVerification.Application.Extensions;
using Integrate.EmailVerification.Application.Models.Request;
using Integrate.EmailVerification.Infrastructure.Extensions;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Migrations;
using Integrate.EmailVerification.Models.Request;
using Integration.Util.Logging;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;

namespace Integrate.EmailVerification.Api;
internal class Program
{
    [ExcludeFromCodeCoverage]
    private static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        var envName = builder.Environment.EnvironmentName.ToLower();

        // Service registrations
        services.AddApplication();
        services.AddInfrastructure();
        services.AddControllers();
        services.AddHealthChecks();
        var redisConnectionString = builder.Configuration["RedisSettingsConnectionString"];
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration["EmailVerificationDatabase"]));

        builder.Services.RegisterLogging(string.Empty, null);
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration[ConfigKeys.EmailVerificationDatabase]));

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(builder.Configuration[ConfigKeys.RedisConnectionString]));

        services.AddTransient<IValidator<EmailVerificationRequest>, EmailVerificationRequestValidator>();
        services.AddTransient<IValidator<BulkEmailVerificationRequest>, BulkEmailVerificationRequestValidator>();

        services.AddEndpointsApiExplorer();
        services.AddCustomSwagger();

        var app = builder.Build();

        app.UseCustomSwaggerUI();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.EnableCorrelationTracing();
        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<RedisSeeder>();
            await seeder.SeedAsync();
        }
        app.Run();
    }
}
