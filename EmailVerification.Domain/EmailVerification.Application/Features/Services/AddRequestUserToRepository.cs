using Integrate.EmailVerification.Application.Features.Interfaces;
using Integrate.EmailVerification.Infrastructure.Repositories;
using Integrate.EmailVerification.Models.Domains;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services
{
    public class AddRequestUserToRepository : IAddRequestUserToRepository
    {
        private readonly IRequestsRepository _requestsRepository;

        public AddRequestUserToRepository(IRequestsRepository requestsRepository)
        {
            _requestsRepository = requestsRepository;
        }
        public async Task<bool> AddRequestToRespository(EmailValidationInfo emailValidation)
        {

            
                var requests = new Requests()
                {
                    Id = emailValidation.RequestId,
                    CreatedBy = emailValidation.CreatedBy,
                    CreatedAt = emailValidation.CreatedAt
                };

                await _requestsRepository.AddRequest(requests);
                return true;
            
            
        }
    }
}
