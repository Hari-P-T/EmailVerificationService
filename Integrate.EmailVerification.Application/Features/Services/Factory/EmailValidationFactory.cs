
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;

namespace Integrate.EmailVerification.Application.Features.Services.Factory
{
    public class EmailValidationFactory : IEmailValidationFactory
    {
        private readonly Dictionary<string, IEmailValidationChecker> _validators;

        public EmailValidationFactory(IEnumerable<IEmailValidationChecker> validators)
        {
            _validators = validators.ToDictionary(v => v.Name, StringComparer.OrdinalIgnoreCase);
        }

        public IEmailValidationChecker GetValidator(string type)
        {
            if (_validators.TryGetValue(type, out var validator))
                return validator;

            //throw new ArgumentException($"Validator for type '{type}' not found.");
            return null; // Return null if the validator is not found, or handle it as needed
        }
    }
}
