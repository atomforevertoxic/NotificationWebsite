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
        
        public User NewUser { get; set; } = default!;

        public IList<User> Users { get; set; } = default!;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] User NewUser)
        {
            if (!ModelState.IsValid) //не уверен что эта проверка нужна т.к. форма уже валидируется...
            {
                return ValidationProblem(ModelState);
            }

            //если время не устанавливается, перезапусти сервер
            NewUser.Timestamp = DateTime.UtcNow; //время UTC т.к. нам важна лишь разница во времени


            ServiceState serviceRespond = await _userService.AddUserAsync(NewUser);
            HandleSubscriptionServiceRespond(serviceRespond);

            return Redirect("/");
        }

        private void HandleSubscriptionServiceRespond(ServiceState respond)
        {
            switch(respond)
            {
                case ServiceState.Success:
                    TempData["SuccessSubscription"] += "The user was successfully subscribed";
                    break;

                case ServiceState.DuplicateMailError:
                    TempData["ErrorSubscription"] = "The user with this email is already subscribed";
                    break;

                case ServiceState.DatabaseAccessError:
                    TempData["ErrorSubscription"] = "Database access error. User is not subscribed";
                    break;

                case ServiceState.UserSavingError:
                    TempData["ErrorSubscription"] = "Error saving user to database. User is not subscribed";
                    break;

                case ServiceState.ScheduleConfigurationError:
                    TempData["ErrorSubscription"] = "Error creating notification schedule. User is not subscribed";
                    break;

                case ServiceState.EmailSendingError:
                    TempData["ErrorSubscription"] = "Error sending email. User is not subscribed";
                    break;

                case ServiceState.OtherError:
                    TempData["ErrorSubscription"] = "An unknown error occurred while subscribing the user. User is not subscribed";
                    break;

            }
        }


        [HttpGet]
        public IActionResult GetUsers()
        {
            IList<User> users = _userService.GetUsers();
            if (users == null || users.Count == 0)
            {
                return NotFound("No users found");
            }
            return Ok(users);
        }


        [HttpPost("{id}/email")]
        public async Task<IActionResult> SendEmailInstantly(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                TempData["ErrorInstantNotify"] = $"User with ID {id} not found";
                return Redirect("/");
            }

            try
            {
                await _userService.InstantNotifyAsync(user);
                TempData["SuccessInstantNotify"] = $"User with ID {id} successfuly received instant message";
            }
            catch
            {
                TempData["ErrorInstantNotify"] = "Error instant sending email";
            }
            return Redirect("/");
        }
    }
}