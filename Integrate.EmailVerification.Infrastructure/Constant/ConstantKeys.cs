using System.Diagnostics.CodeAnalysis;

namespace Integrate.EmailVerification.Infrastructure.Constant
{
    [ExcludeFromCodeCoverage]
    public static class ConstantKeys
    {
        public const string Tlds = "tlds";
        public const string WhitelistedDomains = "whitelist";
        public const string GreylistedDomains = "greylist";
        public const string DisposableDomains = "disposable";
        public const string Bogus = "bogus";
        public const string VulgarWords = "vulgar";
        public const string AliasNames = "alias";
        public const string BlacklistedDomains = "blacklist";
        public const string Established = "established";
        public const string Spam = "spam";
    }
}
