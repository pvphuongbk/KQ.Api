using Microsoft.AspNetCore.Mvc;
using KQ.Data.Base;
using KQ.Services.CommonServices;
using KQ.DataDto.Calculation;
using KQ.Services.Calcualation;
using KQ.Common.Enums;
using KQ.Common.Helpers;
using KQ.DataAccess.Entities;
using KQ.DataAccess.Interface;

namespace KQ.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculationController : ControllerBase
    {
        private readonly ICalcualationService _calcualationService;
        private readonly ICommonRepository<User> _userRepository;

        public CalculationController(ICalcualationService calcualationService, ICommonRepository<User> userRepository)
        {
            _calcualationService = calcualationService;
            _userRepository = userRepository;
        }


        [HttpPost("cal-1")]
        public ResponseBase Cal1(List<Cal1RequestDto> cal1)
        {
            var items = _calcualationService.Cal1Request(cal1);
            return items;
        }
        [HttpPost("cal-2")]
        public ResponseBase Cal2(Cal2RequestDto cal2)
        {
            var user = _userRepository.FindAll(x => !x.IsDeleted && x.ID == cal2.UserId).FirstOrDefault();
            var useName = user == null ? "" : $"ID: {user.ID}. Account {user.Account}. Name : {user.UserName}";
            FileHelper.GeneratorFileByDay(FileStype.Log, $"{useName} \r\n.  {string.Join("\r\n  ", cal2.SynTaxes)}", "Cal2Request");
            var items = _calcualationService.Cal2Request(cal2);
            return items;
        }
    }
}
