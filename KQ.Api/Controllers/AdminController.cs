using Microsoft.AspNetCore.Mvc;
using KQ.Data.Base;
using KQ.Services.Users;
using KQ.DataDto.User;
using KQ.Services.Admin;
using KQ.DataDto.Admin;
using KQ.DataAccess.Entities;

namespace KQ.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }
        [HttpGet("listing")]
        public ResponseBase Listing()
        {
            var items = _adminService.UserListing();
            return items;
        }
        [HttpPost("add")]
        public ResponseBase Add(AddUserDto request)
        {
            var items = _adminService.AddUser(request);
            return items;
        }
        [HttpPost("change-pass")]
        public ResponseBase ChangePass(UserChangePassDto request)
        {
            var items = _adminService.ChangePass(request);
            return items;
        }
        [HttpPost("Renew")]
        public ResponseBase Renew(RenewUserDto request)
        {
            var items = _adminService.RenewUser(request);
            return items;
        }
        [HttpGet("Reset/{userId}")]
        public ResponseBase Reset(int userId)
        {
            var items = _adminService.ResetUser(userId);
            return items;
        }
        [HttpDelete("{userId}")]
        public ResponseBase Delete(int userId)
        {
            var items = _adminService.DeleteUser(userId);
            return items;
        }
        [HttpPost("update")]
        public ResponseBase Update(UpdateUserDto request)
        {
            var items = _adminService.UpdateUser(request);
            return items;
        }
    }
}
