using Microsoft.AspNetCore.Mvc;
using KQ.Data.Base;
using KQ.Services.Users;
using KQ.DataDto.User;

namespace KQ.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost("login")]
        public ResponseBase Login(LoginRequest request)
        {
            var items = _userService.Login(request);
            return items;
        }

        [HttpPut("update-phonebook")]
        public ResponseBase UpdatePhonebook(List<TileUserDto> request)
        {
            var items = _userService.UpdatePhonebook(request);
            return items;
        }
        [HttpPut("update-user")]
        public ResponseBase UpdateUser(UserUpdateDto request)
        {
            var items = _userService.UpdateUser(request);
            return items;
        }
        [HttpGet("phone-book/{userId}")]
        public ResponseBase GetDanhBa(int userId)
        {
            var items = _userService.GetDanhBa(userId);
            return items;
        }
    }
}
