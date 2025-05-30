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

            if (!ModelState.IsValid) //�� ������ ��� ��� �������� ����� �.�. ����� ��� ������������...
            {
                _logger.LogWarning("Model state is invalid for user with email: {Email}", newUser.Email);
                return ValidationProblem(ModelState);
            }

            //���� ����� �� ���������������, ����������� ������
            newUser.Timestamp = DateTime.UtcNow; //����� UTC �.�. ��� ����� ���� ������� �� �������


            var (message, isSuccess) = await _userService.AddUserAsync(newUser);
            if (isSuccess)
            {
                TempData["SuccessSubscription"] = message;
            }
            else
            {
                TempData["ErrorSubscription"] = message;
            }

            return Redirect("/");
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

            _logger.LogInformation("Returning ({UserCount}) users", users.Count);
            return Ok(users);
        }


        [HttpPost("{id}/email")]
        public async Task<IActionResult> SendEmailInstantly(int id)
        {
            _logger.LogInformation("Received request to send email instantly to user with ID ({UserId})", id);

            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID = ({UserId}) not found", id);

                TempData["ErrorInstantNotify"] = $"User with ID = ({id}) not found";
                return Redirect("/");
            }

            try
            {
                _logger.LogInformation("Sending instant email to user with ID ({UserId})", id);

                await _userService.InstantNotifyAsync(user);
                _logger.LogInformation("User with ID ({UserId}) successfully received instant message", id);

                TempData["SuccessInstantNotify"] = $"User with ID ({id}) successfuly received instant message";
            }
            catch
            {
                _logger.LogError("Error occurred while sending instant email to user with ID ({UserId})", id);
                TempData["ErrorInstantNotify"] = "Error instant sending email";
            }
            return Redirect("/");
        }
    }
}