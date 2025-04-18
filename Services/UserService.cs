using NotificationWebsite.Data;
using NotificationWebsite.Models;

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

        public UserService(WebDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

                _emailService.SendWelcomeEmail(user);

                return ServiceState.Success;
            }

            return ServiceState.DatabaseAccessError;
        }

    }
}