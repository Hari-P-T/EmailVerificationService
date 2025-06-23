using Integrate.EmailVerification.Models.Domains;

namespace Integrate.EmailVerification.Infrastructure.Repositories;

public interface IEmailValidationChecksMappingRepository
{
    Task<bool> AddEmailValidationCheckMapping(List<EmailValidationCheckMappings> mapping);
}
