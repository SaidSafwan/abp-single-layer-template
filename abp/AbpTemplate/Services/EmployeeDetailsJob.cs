using AbpTemplate.Data;
using AbpTemplate.Services.Dtos;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace AbpTemplate.Services
{
    public class EmployeeDetailsJob : AsyncBackgroundJob<EmailSendingArgs>, ITransientDependency
    {
        private readonly AbpTemplateDbContext _dbContext;

        public EmployeeDetailsJob(AbpTemplateDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task ExecuteAsync(EmailSendingArgs args)
        {
            var employees = await _dbContext.EmpDetails.ToListAsync();
            foreach (var employee in employees)
            {
                Console.WriteLine($"Employee: {employee.EmpName}, Role: {employee.Role}, Email: {employee.Email}");

            }
        }
    }
}
