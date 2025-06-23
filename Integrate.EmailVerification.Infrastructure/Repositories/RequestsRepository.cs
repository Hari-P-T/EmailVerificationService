using Integrate.EmailVerification.Migrations;
using Integrate.EmailVerification.Models.Domains;
using System.Diagnostics.CodeAnalysis;

namespace Integrate.EmailVerification.Infrastructure.Repositories
{
    [ExcludeFromCodeCoverage]
    public class RequestsRepository : IRequestsRepository
    {
        private readonly AppDbContext _dbContext;
        public RequestsRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> AddRequest(Requests request)
        {
            try
            {
                await _dbContext.Requests.AddRangeAsync(request);
                var result = await _dbContext.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
    }
}