using Integrate.EmailVerification.Migrations;
using Integrate.EmailVerification.Models.Domains;
using System.Diagnostics.CodeAnalysis;

namespace Integrate.EmailVerification.Infrastructure.Repositories;
[ExcludeFromCodeCoverage]
public class EmailValidationChecksMappingRepository : IEmailValidationChecksMappingRepository
{
    private readonly AppDbContext _dbContext;

    public EmailValidationChecksMappingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<bool> AddEmailValidationCheckMapping(List<EmailValidationCheckMappings> mapping)
    {
        try
        {
            // Logic to add the mapping to the database
            await _dbContext.EmailValidationCheckMappingsTable.AddRangeAsync(mapping);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }
    }
}
