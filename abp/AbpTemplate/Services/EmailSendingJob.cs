//using AbpTemplate.Services.Dtos;
//using Volo.Abp.BackgroundJobs;
//using Volo.Abp.DependencyInjection;

//namespace AbpTemplate.Services
//{
//    public class EmailSendingJob
//        : AsyncBackgroundJob<EmailSendingArgs>, ITransientDependency
//    {
//        private readonly ILogger<EmailSendingJob> _logger;

//        public EmailSendingJob(ILogger<EmailSendingJob> logger)
//        {
//            _logger = logger;
//        }

//        public override async Task ExecuteAsync(EmailSendingArgs args)
//        {
//            //await _emailSender.SendAsync(
//            //    args.EmailAddress,
//            //    args.Subject,
//            //    args.Body
//            //);
//            _logger.LogInformation(args.Body);
//        }
//    }
//}
