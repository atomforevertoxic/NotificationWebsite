using FluentEmail.Core;
using NotificationWebsite.Data;

namespace NotificationWebsite.Services
{
    public class EmailService
    {
        private readonly IFluentEmail _fluentEmail;

        public EmailService(IFluentEmail email)
        {
            _fluentEmail = email;
        }

        public void Send(EmailMetadata metadata)
        {
            _fluentEmail.To(metadata.ToAddress)
                .Subject(metadata.Subject)
                .Body(metadata.Body)
                .Send();
        }
    }
}
