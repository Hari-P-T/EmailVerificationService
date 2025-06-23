using System.Diagnostics.CodeAnalysis;
using Autofac.Core;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Infrastructure.Repositories;
using Integration.Util.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Integrate.EmailVerification.Infrastructure.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        //Redis 
        services.AddSingleton<RedisSeeder>();
        services.AddSingleton<IRedisSeeder, RedisSeeder>();
        services.AddSingleton<IRedisCache, RedisCache>();
        services.AddScoped<IEmailValidationChecksMappingRepository, EmailValidationChecksMappingRepository>();
        services.AddScoped<IEmailValidationResultsRepository, EmailValidationResultsRepository>();
        services.AddScoped<IValidationChecksRepository, ValidationChecksRepository>();
        services.AddScoped<IRequestsRepository, RequestsRepository>();
        services.RegisterLogging(string.Empty, null);
        return services;
    }
}
