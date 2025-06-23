using Integrate.EmailVerification.Models.Domains;

namespace Integrate.EmailVerification.Infrastructure.Repositories
{
    public interface IRequestsRepository
    {
        Task<bool> AddRequest(Requests request);
    }
}