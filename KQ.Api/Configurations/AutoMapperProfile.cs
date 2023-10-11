using AutoMapper;
using KQ.DataAccess.Entities;
using KQ.DataDto.User;

namespace KQ.Api.Configurations
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<LoginResponse, User>().ReverseMap();
        }
    }
}
