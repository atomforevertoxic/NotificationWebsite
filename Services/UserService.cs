using NotificationWebsite.Data;
using NotificationWebsite.Models;

namespace NotificationWebsite.Services
{
    public class UserService
    {
        private readonly WebDbContext _context = default!;

        public UserService(WebDbContext context)
        {
            _context = context;
        }

        public IList<User> GetUsers()
        {
            if (_context.Users != null)
            {
                return _context.Users.ToList();
            }
            return new List<User>();
        }


        public string AddUser(User user)
        {
            if (_context.Users != null)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    return "This email is already subscribed";
                }
                _context.Users.Add(user);
                _context.SaveChanges();
                return "Email was successfully signed";
            }

            return "Database access error";
        }

    }
}