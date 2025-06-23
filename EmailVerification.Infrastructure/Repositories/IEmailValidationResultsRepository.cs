using Integrate.EmailVerification.Models.Domains;

namespace Integrate.EmailVerification.Infrastructure.Repositories;

public interface IEmailValidationResultsRepository
{
    Task<bool> CreateEmailValidationResults(List<EmailValidationResults> emailValidationResults);
}
