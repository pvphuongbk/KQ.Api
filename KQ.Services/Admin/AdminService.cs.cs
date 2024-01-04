using KQ.Common.Extention;
using KQ.Data.Base;
using KQ.DataAccess.Entities;
using KQ.DataAccess.Interface;
using KQ.DataAccess.UnitOfWork;
using KQ.DataDto.Admin;
using KQ.DataDto.Enum;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly ICommonRepository<User> _userRepository;
        private readonly ICommonRepository<TileUser> _tileRepository;
        private readonly ICommonRepository<Details> _detailsRepository;
        private readonly ICommonUoW _commonUoW;
        public AdminService(ICommonRepository<User> userRepository, ICommonUoW commonUoW, ICommonRepository<TileUser> tileRepository, ICommonRepository<Details> detailsRepository)
        {
            _userRepository = userRepository;
            _commonUoW = commonUoW;
            _tileRepository = tileRepository;
            _detailsRepository = detailsRepository;
        }

        public ResponseBase ChangePass(UserChangePassDto dto)
        {
            try
            {
                _commonUoW.BeginTransaction();
                var user = _userRepository.FindAll(x => x.ID == dto.UserId).FirstOrDefault();
                if (user == null)
                {
                    return new ResponseBase { Code = 400, Message = "Người dùng không tồn tại" };
                }
                user.Password = dto.NewPass.Encrypt();
                _userRepository.Update(user);
                _commonUoW.Commit();

                return new ResponseBase();
            }
            catch (Exception ex)
            {
                _commonUoW.RollBack();
                return new ResponseBase { Code = 500, Message = ex.Message };
            }
        }

        public ResponseBase DeleteUser(int userId)
        {
            try
            {
                _commonUoW.BeginTransaction();
                var user = _userRepository.FindAll(x => x.ID == userId).FirstOrDefault();
                if (user == null)
                {
                    return new ResponseBase { Code = 400, Message = "Người dùng không tồn tại" };
                }
                var tiles = _tileRepository.FindAll(x => x.UserID == userId);
                var details = _detailsRepository.FindAll(x => x.UserID == userId);
                _tileRepository.RemoveMultiple(tiles);
                _detailsRepository.RemoveMultiple(details);
                _userRepository.Remove(user);
                _commonUoW.Commit();

                return new ResponseBase();
            }
            catch (Exception ex)
            {
                _commonUoW.RollBack();
                return new ResponseBase { Code = 500, Message = ex.Message };
            }
        }

        public ResponseBase RenewUser(RenewUserDto dto)
        {
            try
            {
                _commonUoW.BeginTransaction();
                var user = _userRepository.FindAll(x => x.ID == dto.UserId).FirstOrDefault();
                if (user == null)
                {
                    return new ResponseBase { Code = 400, Message = "Người dùng không tồn tại" };
                }
                user.ExpireDate = dto.NewExpireDate;
                _userRepository.Update(user);
                _commonUoW.Commit();

                return new ResponseBase();
            }
            catch (Exception ex)
            {
                _commonUoW.RollBack();
                return new ResponseBase { Code = 500, Message = ex.Message };
            }
        }

        public ResponseBase ResetUser(int userId)
        {
            try
            {
                _commonUoW.BeginTransaction();
                var user = _userRepository.FindAll(x => x.ID == userId).FirstOrDefault();
                if (user == null)
                {
                    return new ResponseBase { Code = 400, Message = "Người dùng không tồn tại" };
                }
                user.Imei = null;
                _userRepository.Update(user);
                _commonUoW.Commit();

                return new ResponseBase();
            }
            catch (Exception ex)
            {
                _commonUoW.RollBack();
                return new ResponseBase { Code = 500, Message = ex.Message };
            }
        }

        public ResponseBase UserListing()
        {
            try
            {
                List<UserListingDto> lst = new List<UserListingDto>();
                var users = _userRepository.FindAll().ToList();
                foreach (var item in users)
                {
                    lst.Add(new UserListingDto
                    {
                        ID = item.ID,
                        Name = item.UserName,
                        Account = item.Account,
                        ExpireDate = item.ExpireDate,
                        Note = item.Note.Decrypt(),
                        Status = GetUserStatus(item.ExpireDate, item.Imei),
                    });
                }
                lst = lst.OrderBy(x => x.Account).ToList();
                return new ResponseBase { Code = 200, Data = lst };
            }
            catch (Exception ex)
            {
                _commonUoW.RollBack();
                return new ResponseBase { Code = 500, Message = ex.Message };
            }
        }
        private UserStatus GetUserStatus(DateTime expire, string? imei)
        {
            if (expire <= DateTime.Now)
                return UserStatus.Expire;
            if (string.IsNullOrEmpty(imei))
                return UserStatus.NotUse;
            if (expire <= DateTime.Now.AddDays(3))
                return UserStatus.ToExpire;
            else return UserStatus.Active;
        }
        public ResponseBase AddUser(AddUserDto dto)
        {
            try
            {
                _commonUoW.BeginTransaction();
                var userCheck = _userRepository.FindAll(x => x.Account == dto.Account).FirstOrDefault();
                if (userCheck != null)
                {
                    return new ResponseBase { Code = 400, Message = "Người dùng đã tồn tại" };
                }
                if (string.IsNullOrEmpty(dto.Password))
                {
                    return new ResponseBase { Code = 400, Message = "Pass không được rỗng" };
                }
                var user = new User
                {
                    Account = dto.Account,
                    UserName = dto.Name,
                    Password = dto.Password.Encrypt(),
                    PhoneNumber = dto.PhoneNumber,
                    ExpireDate = dto.ExpireDate,
                    Note = dto.Note.Encrypt(),
                    StartDate = DateTime.Now,
                };
                _userRepository.Insert(user);
                _commonUoW.Commit();

                return new ResponseBase();
            }
            catch (Exception ex)
            {
                _commonUoW.RollBack();
                return new ResponseBase { Code = 500, Message = ex.Message };
            }
        }
    }
}
