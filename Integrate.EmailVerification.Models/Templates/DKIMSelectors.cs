using System.Diagnostics.CodeAnalysis;


namespace Integrate.EmailVerification.Models.Templates
{
    [ExcludeFromCodeCoverage]
    public class DKIMSelectors
    {
        public List<string> Selectors { get; set; } = new List<string>();
    }
}
