using KQ.Data.Base;
using KQ.DataDto.User;

namespace KQ.Services.Users
{
    public interface IUserService
    {
        public ResponseBase Login(LoginRequest request);
        public ResponseBase UpdatePhonebook(List<TileUserDto> request);
        public ResponseBase UpdateUser(UserUpdateDto request);
        public ResponseBase GetDanhBa(int userId);
        public ResponseBase UserInfo(int userId);
    }
}