using Integrate.EmailVerification.Models.Domains;

namespace Integrate.EmailVerification.Infrastructure.Repositories;

public interface IValidationChecksRepository
{
    Task<List<ValidationChecks>> RetrieveAllValidationChecks();
}
