using NotificationWebsite.Data;
using NotificationWebsite.Models;
using Hangfire;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

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

        public UserService(WebDbContext context, EmailService emailService, IOptions<NotificationSettings> notificationSettings)
        {
            _context = context;
            _emailService = emailService;
            _notificationSettings = notificationSettings.Value;
        }

        public IList<User> GetUsers()
        {
            if (_context.Users != null)
            {
                return _context.Users.ToList();
            }
            return new List<User>();
        }


        public async Task<ServiceState> AddUserAsync(User user)
        {
            if (_context.Users != null)
            {
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    return ServiceState.DuplicateMailError;
                }



                try
                {
                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    return ServiceState.UserSavingError;
                }



                try
                {
                    if (_context.Users.Count() == 1)
                    {
                        ScheduleNotifications();
                    }
                }
                catch
                {
                    return ServiceState.ScheduleConfigurationError;
                }



                try
                {
                    await _emailService.SendTemplateEmailAsync(user, "Welcome", _notificationSettings.GreetTemplate);
                }
                catch
                {
                    return ServiceState.EmailSendingError;
                }

                return ServiceState.Success;
            }

            return ServiceState.DatabaseAccessError;
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
            var users = GetUsers(); 
            await _emailService.NotifySubscribersAsync(users);
        }

        public async Task InstantNotifyAsync(User receiver)
        {
            await _emailService.SendTemplateEmailAsync(
                receiver,
                "Instant Notification",
                _notificationSettings.RemindTemplate);
        }
    }
}