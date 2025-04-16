using Microsoft.AspNetCore.Mvc;
using NotificationWebsite.Services;

[Route("api/[controller]")]
[ApiController]
public class UsersController: ControllerBase
{
    private readonly UserService _userService;
    public User NewUser { get; set; } = default!;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }


    [HttpPost]
    public IActionResult CreateUser([FromBody] User user)
    {
        //if (!ModelState.IsValid || newUser == null)
        //{
        //    return BadRequest(ModelState);
        //}

        //newUser.Timestamp = DateTime.UtcNow;
        //_userService.AddUser(newUser);

        //return CreatedAtAction("...", new { email = newUser.Email }, newUser);

        if (user == null)
        {
            return BadRequest("Invalid user data");
        }

        return CreatedAtAction("....", new { id = user.Id }, user);
    }

}