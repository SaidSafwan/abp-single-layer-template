using Volo.Abp.BackgroundJobs;

namespace AbpTemplate.Services.Dtos
{
    [BackgroundJobName("emails")]
    public class EmailSendingArgs
    {
        public string EmailAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
