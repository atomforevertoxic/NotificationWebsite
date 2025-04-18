using Microsoft.AspNetCore.Mvc;
using NotificationWebsite.Services;
using NotificationWebsite.Models;
using System.Security.AccessControl;
using NotificationWebsite.Data;

namespace NotificationWebsite.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly EmailService _emailService;
        public User NewUser { get; set; } = default!;

        public IList<User> Users { get; set; } = default!;

        public UsersController(UserService userService, EmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }


        [HttpPost]
        public IActionResult CreateUser([FromForm] User NewUser)
        {

            if (!ModelState.IsValid) //не уверен что эта проверка нужна т.к. форма уже валидируется...
            {
                return ValidationProblem(ModelState);
            }

            //если время не устанавливается, перезапусти сервер
            NewUser.Timestamp = DateTime.UtcNow; //время UTC т.к. нам важна лишь разница во времени
            
            ServiceState serviceRespond = _userService.AddUser(NewUser);
            HandleServiceRespond(serviceRespond);

            return Redirect("/");
        }

        private void HandleServiceRespond(ServiceState respond)
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
                    TempData["ErrorSubscription"] = "Database access error";
                    break;

                case ServiceState.OtherError:
                    TempData["ErrorSubscription"] = "An unknown error occurred while subscribing the user";
                    break;

            }
        }


        [HttpGet]
        public IActionResult GetUsers()
        {
            Users = _userService.GetUsers();
            return Ok(Users);
        }
    }
}