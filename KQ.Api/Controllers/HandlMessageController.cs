using Microsoft.AspNetCore.Mvc;
using KQ.Data.Base;
using KQ.Services.CommonServices;
using KQ.DataDto.Calculation;
using KQ.Services.Calcualation;
using KQ.Common.Enums;
using KQ.Common.Helpers;
using KQ.DataAccess.Entities;
using KQ.DataAccess.Interface;
using KQ.Services.HandlMessageService;
using KQ.DataDto.HandlMessage;

namespace KQ.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HandlMessageController : ControllerBase
    {
        private readonly IHandlMessageService _handlMessageService;

        public HandlMessageController(IHandlMessageService handlMessageService)
        {
            _handlMessageService = handlMessageService;
        }


        [HttpPost("message-by-day")]
        public ResponseBase MessageByDay(MessgeByDayRequest request)
        {
            var items = _handlMessageService.MessageByDay(request);
            return items;
        }
        [HttpDelete("{id}")]
        public ResponseBase Delete(int id)
        {
            var items = _handlMessageService.Delete(id);
            return items;
        }
    }
}
