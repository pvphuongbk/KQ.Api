using Microsoft.AspNetCore.Mvc;
using KQ.Data.Base;
using KQ.Services.CommonServices;
using KQ.Common.Helpers;

namespace KQ.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommonController : ControllerBase
    {
        private readonly ICommonService _commonService;

        public CommonController(ICommonService commonService)
        {
            _commonService = commonService;
        }

        [HttpGet("chanels")]
        public ResponseBase GetChanels(DateTime? date)
        {
            var items = _commonService.GetChanel(date);
            return items;
        }
        [HttpGet("check")]
        public ResponseBase CheckKq(DayOfWeek? day)
        {
            var items = _commonService.CheckKQ(day);
            return items;
        }
        [HttpGet("check-chanel-code")]
        public ResponseBase CheckChanelCode()
        {
            var items = _commonService.CheckChanelCode();
            return items;
        }
        [HttpGet("unit-test")]
        public ResponseBase UnitTest()
        {
            var items = _commonService.UnitTest();
            return items;
        }
        [HttpGet("unit-test2")]
        public ResponseBase UnitTest2()
        {
            var items = _commonService.UnitTestCal3();
            return items;
        }
        [HttpPost("read-logs")]
        public ResponseBase ReadLogs(DateTime? date)
        {
            var items = _commonService.ReadLogs(date);
            return items;
        }

        [HttpGet("update-store")]
        public bool UpdateStore(DateTime start, DateTime end)
        {
            var items = InnitRepository.InsertUpdateManyDate(start, end);
            return items;
        }
    }
}
