using FluentEmail.Core;
using Hangfire;
using Microsoft.Extensions.Options;
using NotificationWebsite.Data;
using NotificationWebsite.Models;

namespace NotificationWebsite.Services
{
    public class EmailService
    {
        private readonly IFluentEmail _fluentEmail;
        private readonly NotificationSettings _notificationSettings;
        public EmailService(IFluentEmail email, IOptions<NotificationSettings> notificationSettings)
        {
            _fluentEmail = email;
            _notificationSettings = notificationSettings.Value;
        }


        public void NotifySubscribers(IList<User> subscribers)
        {
            foreach (User subscriber in subscribers)
            {
                SendTemplateEmail(subscriber, "Notification", _notificationSettings.RemindTemplate);
            }
        }

        public void SendTemplateEmail(User receiver, string subject, string templateFile)
        {
            string EmailTemplatePath = Directory.GetCurrentDirectory() + _notificationSettings.RelativePath + templateFile;

            EmailTemplatePath= EmailTemplatePath.Replace('\\', Path.DirectorySeparatorChar);

            _fluentEmail.To(receiver.Email)
                .Subject(subject)
                .UsingTemplateFromFile(EmailTemplatePath, receiver)
                .Send();
        }
    }
}
