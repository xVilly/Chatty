using Chatty.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chatty.Controllers
{
    
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserService _userService;

        public UserController(ILogger<UserController> logger, UserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpPost("create")]
        public ActionResult CreateUser
            ([FromForm(Name = "Name")] string Name, 
            [FromForm(Name = "Password")] string Password)
        {
            var checkName = _userService.GetByName(Name);
            if (checkName != null)
                return BadRequest("User name already exists.");
            _userService.Create(new Models.User
            {
                Name = Name,
                Password = Password,
                Permission = 0
            });
            return Ok("User has been created.");
        }

        [HttpGet("get/all")]
        public ActionResult<IEnumerable<string>> GetUsers()
        {
            var users = _userService.GetAll();
            var userNames = new List<string>();
            foreach (var user in users)
            {
                userNames.Add(user.Name);
            }
            return Ok(userNames);
        }
    }
}