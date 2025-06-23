using Integrate.EmailVerification.Migrations;
using Integrate.EmailVerification.Models.Domains;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Integrate.EmailVerification.Infrastructure.Repositories;
[ExcludeFromCodeCoverage]
public class EmailValidationResultsRepository : IEmailValidationResultsRepository
{
    private readonly AppDbContext _dbContext;

    public EmailValidationResultsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<bool> CreateEmailValidationResults(List<EmailValidationResults> emailValidationResults)
    {
        try
        {
            // Logic to add the mapping to the database
            await _dbContext.EmailValidationResults.AddRangeAsync(emailValidationResults);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }
    }
}
