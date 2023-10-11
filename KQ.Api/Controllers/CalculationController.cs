using Microsoft.AspNetCore.Mvc;
using KQ.Data.Base;
using KQ.Services.CommonServices;
using KQ.DataDto.Calculation;
using KQ.Services.Calcualation;

namespace KQ.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculationController : ControllerBase
    {
        private readonly ICalcualationService _calcualationService;

        public CalculationController(ICalcualationService calcualationService)
        {
            _calcualationService = calcualationService;
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
            var items = _calcualationService.Cal2Request(cal2);
            return items;
        }
    }
}
