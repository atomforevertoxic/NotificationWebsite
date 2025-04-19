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
        public IActionResult CreateUser([FromForm] User NewUser)
        {

            if (!ModelState.IsValid) //�� ������ ��� ��� �������� ����� �.�. ����� ��� ������������...
            {
                return ValidationProblem(ModelState);
            }

            //���� ����� �� ���������������, ����������� ������
            NewUser.Timestamp = DateTime.UtcNow; //����� UTC �.�. ��� ����� ���� ������� �� �������
            
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
            IList<User> users = _userService.GetUsers();
            if (users == null || users.Count == 0)
            {
                return NotFound("No users found");
            }
            return Ok(users);
        }


        [HttpPost("{id}/email")]
        public IActionResult SendEmailInstantly(int id)
        {

            var user = _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            _userService.InstantNotify(user);

            return Redirect("/");
        }
    }
}