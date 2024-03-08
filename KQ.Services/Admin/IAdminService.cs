using KQ.Data.Base;
using KQ.DataDto.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.Services.Admin
{
    public interface IAdminService
    {
        public ResponseBase UserListing();
        public ResponseBase AddUser(AddUserDto dto);
        public ResponseBase RenewUser(RenewUserDto dto);
        public ResponseBase ChangePass(UserChangePassDto dto);
        public ResponseBase ResetUser(int userId);
        public ResponseBase DeleteUser(int userId);
        public ResponseBase UpdateUser(UpdateUserDto dto);
    }
}
