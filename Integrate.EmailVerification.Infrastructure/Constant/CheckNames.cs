using System.Diagnostics.CodeAnalysis;

namespace Integrate.EmailVerification.Infrastructure.Constant;

[ExcludeFromCodeCoverage]
public static class CheckNames
{
    public const string ValidTopLevelDomain = "UnrecognizedTLD";
    public const string ValidEmailRegex = "InvalidSyntax";
    public const string ValidDomainSyntax = "InvalidDomainSpecificSyntax";
    public const string DnsValidation = "InvalidDNS";
    public const string MxRecord = "NoMXRecords";
    public const string Established = "Established";
    public const string Alias = "Alias";
    public const string BogusEmailAddress = "Bogus";
    public const string BogusSMSAddress = "BogusSMSAddress";
    public const string GargbageEmailAddress = "Garbage";
    public const string Vulgar = "Vulgar";
    public const string MailBoxFull = "MailBoxIsFull";
    public const string MailBoxAvailablity = "MailboxIsBusy";
    public const string DisposableDomain = "DisposableEmail";
    public const string SpamDomain = "KnownSpammer";
    public const string BlackListedDomain = "BlacklistedDomain";
    public const string GreyListedDomain = "KnownGreylister";
    public const string OptInRequired = "OptInRequired";
    public const string WhiteListedDomain = "IsWhiteListOnly";
    public const string ConnectionRefused = "ConnectionRefused";  
    public const string BadEmail = "EmailIsBad";

    // Extra Checks
    public const string ValidSMTPCheck = "ValidSMTPCheck";
    public const string CatchAll = "IsNotACatchAll";
    public const string SpfRecord = "HasASpfRecord";
    public const string DkimRecord = "HasADkimRecord";
    public const string DmarcRecord = "HasADmarcRecord";
    public const string DomainReachable = "DomainRechable";
}
