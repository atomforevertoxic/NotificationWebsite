using Microsoft.AspNetCore.Mvc;
using NotificationWebsite.Services;
using NotificationWebsite.Models;

namespace NotificationWebsite.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
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

            NewUser.Timestamp = DateTime.UtcNow; //����� UTC �.�. ��� ����� ���� ������� �� �������
            _userService.AddUser(NewUser);
            return Redirect("/");
        }


        [HttpGet]
        public IActionResult GetUsers()
        {
            Users = _userService.GetUsers();
            return Ok(Users);
        }
    }
}