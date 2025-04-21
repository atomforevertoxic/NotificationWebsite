using NotificationWebsite.Data;
using NotificationWebsite.Models;
using Hangfire;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using NotificationWebsite.Controllers;

namespace NotificationWebsite.Services
{

    public enum ServiceState
    {
        Success,
        DuplicateMailError,
        DatabaseAccessError,
        UserSavingError,
        EmailSendingError,
        ScheduleConfigurationError,
        OtherError
    }

    public class UserService
    {
        private readonly WebDbContext _context = default!;

        private readonly EmailService _emailService = default!;

        private readonly NotificationSettings _notificationSettings;

        private readonly ILogger<UsersController> _logger;

        public UserService(WebDbContext context, EmailService emailService, IOptions<NotificationSettings> notificationSettings, ILogger<UsersController> logger)
        {
            _context = context;
            _emailService = emailService;
            _notificationSettings = notificationSettings.Value;
            _logger = logger;
        }

        public IList<User> GetUsers()
        {
            if (_context.Users != null)
            {
                return _context.Users.ToList();
            }
            return new List<User>();
        }


        public async Task<(string Message, bool IsSuccess)> AddUserAsync(User user)
        {
            if (_context!=null && _context.Users != null)
            {
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    _logger.LogError($"{ServiceStateLogText.DuplicateEmailError}");
                    return (ServiceStateLogText.DuplicateEmailError, false);
                }


                try
                {
                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogError($"{ServiceStateLogText.UserSavingError}\n{ex.Message}");
                    return (ServiceStateLogText.UserSavingError, false);
                }

                try
                {
                    if (_context.Users.Count() == 1) 
                    {
                        _logger.LogInformation("Start scheduling notifications");
                        ScheduleNotifications();
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError($"{ServiceStateLogText.ScheduleConfigurationError}\n{ex.Message}");
                    return (ServiceStateLogText.ScheduleConfigurationError, false);
                }

                try
                {
                    _logger.LogInformation("Sending welcome email to new user with email: {Email}.", user.Email);
                    await _emailService.SendTemplateEmailAsync(user, "Welcome", _notificationSettings.GreetTemplate); 
                }
                catch(Exception ex)
                {
                    _logger.LogError($"{ServiceStateLogText.EmailSendingError}\n{ex.Message}");
                    return (ServiceStateLogText.EmailSendingError, false);
                }


                _logger.LogInformation(ServiceStateLogText.UserCreationSuccess);
                return (ServiceStateLogText.UserCreationSuccess, true);
            }

            _logger.LogError($"{ServiceStateLogText.DatabaseAccessError}");
            return (ServiceStateLogText.DatabaseAccessError, false);
        }

        public async Task<User?> GetUserById(int id)
        {
            if (_context == null) return null;
            else if (_context.Users == null || _context.Users.Count() == 0) return null;

            return await _context.Users.FindAsync(id);
        }

        private void ScheduleNotifications()
        {
            RecurringJob.AddOrUpdate(
                "Notification",
                () => NotifySubscribers(),
                _notificationSettings.Cron
            );
        }

        public async Task NotifySubscribers()
        {
            _logger.LogInformation("Starting notification process for subscribers");
            var users = GetUsers(); 
            await _emailService.NotifySubscribersAsync(users);
        }

        public async Task InstantNotifyAsync(User receiver)
        {
            _logger.LogInformation("Sending instant notification email to user with email: {Email}.", receiver.Email);
            await _emailService.SendTemplateEmailAsync(
                receiver,
                "Instant Notification",
                _notificationSettings.RemindTemplate);
        }
    }
}