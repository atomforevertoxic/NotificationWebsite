using NotificationWebsite.Data;
using NotificationWebsite.Models;
using Hangfire;
using Microsoft.Extensions.Options;

namespace NotificationWebsite.Services
{

    public enum ServiceState
    {
        Success,
        DuplicateMailError,
        DatabaseAccessError,
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


        public ServiceState AddUser(User user)
        {
            if (_context.Users != null)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    return ServiceState.DuplicateMailError;
                }


                _context.Users.Add(user);
                _context.SaveChanges();


                if (GetUsers().Count==1)
                {
                    ScheduleNotifications();
                }

                //добавить обработчик ошибок
                _emailService.SendTemplateEmail(user, "Welcome", _notificationSettings.GreetTemplate);


                return ServiceState.Success;
            }

            return ServiceState.DatabaseAccessError;
        }

        public User? GetUserById(int id)
        {
            if (_context == null) return null;
            else if (_context.Users == null || _context.Users.Count() == 0) return null;

            return _context?.Users?.FirstOrDefault(user => user.Id == id);
        }

        private void ScheduleNotifications()
        {
            //добавить обработчик ошибок
            RecurringJob.AddOrUpdate("Notification",
                () => _emailService.NotifySubscribers(GetUsers()),
                "* * * * *");
        }

        public void InstantNotify(User receiver)
        {
            _emailService.SendTemplateEmail(
                receiver, 
                "Notify",
                _notificationSettings.RemindTemplate);
        }
    }
}