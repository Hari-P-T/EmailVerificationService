using System.Diagnostics.CodeAnalysis;
using DnsClient;
using Integrate.EmailVerification.Application.Features.EmailVerification;
using Integrate.EmailVerification.Application.Features.Interfaces;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Application.Features.Services;
using Integrate.EmailVerification.Application.Features.Services.DomainChecks;
using Integrate.EmailVerification.Application.Features.Services.EmailAddress;
using Integrate.EmailVerification.Application.Features.Services.Factory;
using Integrate.EmailVerification.Application.Features.Services.Regex;
using Integrate.EmailVerification.Application.Features.Services.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Services.UserNameChecks;
using Integrate.EmailVerification.Application.Features.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace Integrate.EmailVerification.Application.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmailVerificationHandler, EmailVerificationHandler>();
        services.AddScoped<IBulkEmailVerifier, BulkEmailVerifier>();

        //Checker Class
        services.AddTransient<IEmailValidationFactory, EmailValidationFactory>();
        services.AddScoped<IEmailValidationChecksInfoFactory, EmailValidationChecksInfoFactory>();

        services.AddScoped<IEmailValidationChecker, RegisteredTLDCheck>();
        services.AddScoped<IEmailValidationChecker, ValidEmailRegex>();
        services.AddScoped<IEmailValidationChecker, ValidDomainRegex>();
        services.AddScoped<IEmailValidationChecker, UserNameValidator>();
        services.AddScoped<IEmailValidationChecker, DNSValidationCheck>();
        services.AddScoped<IEmailValidationChecker, MXRecordCheck>();
        services.AddScoped<IEmailValidationChecker, EstablishedCheck>();
        services.AddScoped<IEmailValidationChecker, AliasCheck>();
        services.AddScoped<IEmailValidationChecker, BogusSMSCheck>();
        services.AddScoped<IEmailValidationChecker, BogusEmailCheck>();
        services.AddScoped<IEmailValidationChecker, VulgarCheck>();
        services.AddScoped<IEmailValidationChecker, MailBoxFullCheck>();
        services.AddScoped<IEmailValidationChecker, MailBoxAvailablity>();
        services.AddScoped<IEmailValidationChecker, DisposableDomainCheck>();
        services.AddScoped<IEmailValidationChecker, SpamCheck>();
        services.AddScoped<IEmailValidationChecker, BlackListedDomainCheck>();
        services.AddScoped<IEmailValidationChecker, KnownGreyListerCheck>();
        services.AddScoped<IEmailValidationChecker, WhiteListedDomainCheck>();
        services.AddScoped<IEmailValidationChecker, BadEmailCheck>();
        services.AddScoped<IEmailValidationChecker, OPTInRequiredCheck>();
        services.AddScoped<IEmailValidationChecker, ConnectionRefusedCheck>();
        

        services.AddScoped<IEmailValidationChecker, ValidSMTPCheck>();
        services.AddScoped<IEmailValidationChecker, CatchAllCheck>(); 
        services.AddScoped<IEmailValidationChecker, SPFRecordCheck>();
        services.AddScoped<IEmailValidationChecker, DKIMRecordCheck>();
        services.AddScoped<IEmailValidationChecker, DMARCRecordCheck>();


        services.AddScoped<IEmailHelper, EmailHelper>();

        // SMTP checks
        services.AddScoped<IMXRecordChecker, MXRecordChecker>();

        //Repositories Linking
        services.AddScoped<IAddRequestUserToRepository, AddRequestUserToRepository>();
        services.AddSingleton<ILookupClient, LookupClient>();


        return services;
    }
}