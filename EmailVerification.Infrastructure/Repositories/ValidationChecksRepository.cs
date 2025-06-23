using Integrate.EmailVerification.Migrations;
using Integrate.EmailVerification.Models.Domains;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Integrate.EmailVerification.Infrastructure.Repositories;
[ExcludeFromCodeCoverage]
public class ValidationChecksRepository : IValidationChecksRepository
{
    private readonly AppDbContext _dbContext;

    public ValidationChecksRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<ValidationChecks>> RetrieveAllValidationChecks()
    {

        try
        {
            return await _dbContext.ValidationChecks.Where(x => x.IsActive).ToListAsync();
        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }
        


    }
}
