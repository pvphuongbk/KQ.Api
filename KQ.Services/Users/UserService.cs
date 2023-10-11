using AutoMapper;
using KQ.Data.Base;
using KQ.DataAccess.Entities;
using KQ.DataAccess.Interface;
using KQ.DataDto.User;
using Microsoft.EntityFrameworkCore;
namespace KQ.Services.Users
{
    public class UserService : IUserService
    {
        private readonly ICommonRepository<User> _userRepository;
        private readonly ICommonRepository<TileUser> _tileUserRepository;

        private readonly ICommonUoW _commonUoW;
        private readonly IMapper _mapper;

        public UserService(ICommonRepository<User> userRepository, IMapper mapper, ICommonRepository<TileUser> tileUserRepository, ICommonUoW commonUoW)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _tileUserRepository = tileUserRepository;
            _commonUoW = commonUoW;
        }
        public ResponseBase GetDanhBa(int userId)
        {
            ResponseBase response = new ResponseBase();
            var phones = _tileUserRepository.FindAll(x => x.UserID == userId).ToList();
            var items = new List<Phonebook>();
            foreach (var u in phones)
            {
                items.Add(new Phonebook
                {
                    ID = u.ID,
                    Name = u.Name,
                    TileXac = u.TileXac,
                    TileThuong = u.TileThuong,
                    TileBaSo = u.TileBaSo,
                    PhoneNumber = u.PhoneNumber
                });
            }
            response.Data = items;
            return response;
        }
        public ResponseBase Login(LoginRequest request)
        {
            ResponseBase response = new ResponseBase();
            var user = _userRepository.FindAll(x => !x.IsDeleted && (x.Account == request.LoginName || x.PhoneNumber == request.LoginName)
                                && x.Password == request.Password)
                            .Include(x => x.TileUser)
                            .FirstOrDefault();
            LoginResponse result = new LoginResponse();
            if(user != null)
            {
                result = _mapper.Map<LoginResponse>(user);
                result.Phonebooks = new List<Phonebook>();
                foreach (var u in user.TileUser)
                {
                    result.Phonebooks.Add(new Phonebook
                    {
                        ID = u.ID,
                        Name = u.Name,
                        TileXac = u.TileXac,
                        TileThuong = u.TileThuong,
                        TileBaSo = u.TileBaSo,
                        PhoneNumber = u.PhoneNumber
                    });
                }
                result.IsLoginSuccess = true;
            }
            response.Data = result;
            return response;
        }

        public ResponseBase UpdatePhonebook(List<TileUserDto> request)
        {
            _commonUoW.BeginTransaction();
            try
            {
                if(request != null && request.Any())
                {
                    var delIds = request.Where(x => x.IsDeleted).Select(x => x.ID).ToList();
                    var dels = _tileUserRepository.FindAll(x => delIds.Contains(x.ID));
                    _tileUserRepository.RemoveMultiple(dels);

                    var upIds = request.Where(x => x.ID != null && x.ID != 0 && !x.IsDeleted).Select(x => x.ID).ToList();
                    var ups = _tileUserRepository.FindAll(x => upIds.Contains(x.ID));
                    foreach (var item in ups)
                    {
                        var up = request.FindAll(x => x.ID == item.ID).FirstOrDefault();
                        if(up != null)
                        {
                            item.Name = up.Name;
                            item.TileXac = up.TileXac;
                            item.TileThuong = up.TileThuong;
                            item.TileBaSo = up.TileBaSo;
                            item.PhoneNumber = up.PhoneNumber;
                        }
                    }
                    _tileUserRepository.UpdateMultiple(ups);

                    List<TileUser> tileUsers = new List<TileUser>();
                    foreach (var phonebook in request.Where(x => x.ID == null || x.ID == 0))
                    {
                        tileUsers.Add(new TileUser
                        {
                            Name = phonebook.Name,
                            TileThuong = phonebook.TileThuong,
                            TileXac = phonebook.TileXac,
                            UserID = phonebook.UserID,
                            TileBaSo = phonebook.TileBaSo,
                            PhoneNumber = phonebook.PhoneNumber,
                        });
                    }
                    _tileUserRepository.InsertMultiple(tileUsers.AsQueryable());
                }
                _commonUoW.Commit();
                return new ResponseBase();
            }
            catch (Exception ex)
            {
                _commonUoW.RollBack();
                return new ResponseBase { Code = 501, Message = ex.Message };
            }
        }

        public ResponseBase UpdateUser(UserUpdateDto request)
        {
            _commonUoW.BeginTransaction();
            try
            {
                var user = _userRepository.FindAll(x => x.ID == request.UserID).FirstOrDefault();
                if (user == null)
                {
                    return new ResponseBase { Code = 501, Message = "User không tồn tại" };
                }
                user.UserName = request.UserName;
                user.PhoneNumber = request.PhoneNumber;
                user.TileXac = request.TileXac;
                user.TileThuong = request.TileThuong;
                user.TileBaSo = request.TileBaSo;
                _commonUoW.Commit();
                return new ResponseBase();
            }
            catch (Exception ex)
            {
                _commonUoW.RollBack();
                return new ResponseBase { Code = 501, Message = ex.Message };
            }
        }
    }
}
