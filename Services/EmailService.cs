using FluentEmail.Core;
using NotificationWebsite.Data;
using NotificationWebsite.Models;

namespace NotificationWebsite.Services
{
    public class EmailService
    {
        private readonly IFluentEmail _fluentEmail;

        public EmailService(IFluentEmail email)
        {
            _fluentEmail = email;
        }

        public void SendWelcomeEmail(User receiver)
        {

            var EmailTemplate = $"{Directory.GetCurrentDirectory()}/Pages/Templates/Greeting.cshtml";
            _fluentEmail.To(receiver.Email)
                .Subject("Welcome!")
                .UsingTemplateFromFile(EmailTemplate, receiver)
                .Send();
        }
    }
}
