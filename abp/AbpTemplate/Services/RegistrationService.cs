using AbpTemplate.Services.Dtos;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Services;

namespace AbpTemplate.Services
{
    public class RegistrationService
        : DomainService
    {
        private readonly IBackgroundJobManager _backgroundJobManager;

        public RegistrationService(IBackgroundJobManager backgroundJobManager)
        {
            _backgroundJobManager = backgroundJobManager;
        }

        public async Task RegisterAsync(string userName, string emailAddress, string password)
        {
            await _backgroundJobManager.EnqueueAsync(
                new EmailSendingArgs
                {
                    EmailAddress = emailAddress,
                    Subject = "You've successfully registered!",
                    Body = "..."
                }
            );
        }
    }

}
