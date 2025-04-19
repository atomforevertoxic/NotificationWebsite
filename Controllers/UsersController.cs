using Microsoft.AspNetCore.Mvc;
using NotificationWebsite.Services;
using NotificationWebsite.Models;
using System.Security.AccessControl;
using NotificationWebsite.Data;
using System.Net.Http;
using System.Net.Mail;

namespace NotificationWebsite.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly ILogger<UsersController> _logger;
        
        public User NewUser { get; set; } = default!;

        public IList<User> Users { get; set; } = default!;

        public UsersController(UserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }


        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] User newUser)
        {
            _logger.LogInformation("Received request to create a new user with email: {Email}", newUser.Email);

            if (!ModelState.IsValid) //не уверен что эта проверка нужна т.к. форма уже валидируется...
            {
                _logger.LogWarning("Model state is invalid for user with email: {Email}", newUser.Email);
                return ValidationProblem(ModelState);
            }

            //если время не устанавливается, перезапусти сервер
            newUser.Timestamp = DateTime.UtcNow; //время UTC т.к. нам важна лишь разница во времени


            ServiceState serviceRespond = await _userService.AddUserAsync(newUser);
            _logger.LogInformation("User creation process completed for {@Email} with response: {Response}", newUser.Email, serviceRespond);

            HandleSubscriptionServiceRespond(serviceRespond);

            return Redirect("/");
        }

        private void HandleSubscriptionServiceRespond(ServiceState respond)
        {
            switch(respond)
            {
                case ServiceState.Success:
                    
                    TempData["SuccessSubscription"] += "The user was successfully subscribed";
                    
                    _logger.LogInformation("User successfully subscribed.");
                    break;

                case ServiceState.DuplicateMailError:
                    
                    _logger.LogWarning("Attempt to subscribe a user with a duplicate email.");
                    
                    TempData["ErrorSubscription"] = "The user with this email is already subscribed";
                    break;

                case ServiceState.DatabaseAccessError:
                    
                    _logger.LogError("Database access error while subscribing a user.");
                    
                    TempData["ErrorSubscription"] = "Database access error. User is not subscribed";
                    break;

                case ServiceState.UserSavingError:
                    
                    _logger.LogError("Error saving user to database.");
                    
                    TempData["ErrorSubscription"] = "Error saving user to database. User is not subscribed";
                    break;

                case ServiceState.ScheduleConfigurationError:
                    
                    _logger.LogError("Error creating notification schedule for user.");
                    
                    TempData["ErrorSubscription"] = "Error creating notification schedule. User is not subscribed";
                    break;

                case ServiceState.EmailSendingError:

                    _logger.LogError("Error sending email to user during subscription.");
                    
                    TempData["ErrorSubscription"] = "Error sending email. User is not subscribed";
                    break;

                case ServiceState.OtherError:
                    
                    _logger.LogError("An unknown error occurred while subscribing the user.");

                    TempData["ErrorSubscription"] = "An unknown error occurred while subscribing the user. User is not subscribed";
                    break;

            }
        }


        [HttpGet]
        public IActionResult GetUsers()
        {
            _logger.LogInformation("Received request to get users.");
            IList<User> users = _userService.GetUsers();
            
            if (users == null || users.Count == 0)
            {
                _logger.LogWarning("No users found in the database.");
                return NotFound("No users found");
            }

            _logger.LogInformation("Returning {UserCount} users.", users.Count);
            return Ok(users);
        }


        [HttpPost("{id}/email")]
        public async Task<IActionResult> SendEmailInstantly(int id)
        {
            _logger.LogInformation("Received request to send email instantly to user with ID {UserId}.", id);

            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID = ({UserId}) not found.", id);

                TempData["ErrorInstantNotify"] = $"User with ID = ({id}) not found";
                return Redirect("/");
            }

            try
            {
                _logger.LogInformation("Sending instant email to user with ID {UserId}.", id);

                await _userService.InstantNotifyAsync(user);
                _logger.LogInformation("User with ID {UserId} successfully received instant message.", id);

                TempData["SuccessInstantNotify"] = $"User with ID {id} successfuly received instant message";
            }
            catch
            {
                _logger.LogError("Error occurred while sending instant email to user with ID {UserId}.", id);
                TempData["ErrorInstantNotify"] = "Error instant sending email";
            }
            return Redirect("/");
        }
    }
}