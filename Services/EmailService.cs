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


        public async Task NotifySubscribersAsync(IList<User> subscribers)
        {
            var tasks = new List<Task>(); 
            foreach (User subscriber in subscribers)
            {
                tasks.Add(SendTemplateEmailAsync(subscriber, "Notification", _notificationSettings.RemindTemplate));
            }
            await Task.WhenAll(tasks); 
        }

        public async Task SendTemplateEmailAsync(User receiver, string subject, string templateFile)
        {
            var emailTemplatePath = Directory.GetCurrentDirectory() + _notificationSettings.RelativePath + templateFile;
            emailTemplatePath = emailTemplatePath.Replace('\\', Path.DirectorySeparatorChar);

            await _fluentEmail.To(receiver.Email)
                .Subject(subject)
                .UsingTemplateFromFile(emailTemplatePath, receiver)
                .SendAsync();
        }
    }
}
