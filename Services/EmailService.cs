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
    }
}
