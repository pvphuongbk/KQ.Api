using KQ.Data.Base;
using KQ.DataAccess.Entities;
using KQ.DataAccess.Enum;
using KQ.DataDto.Calculation;
using KQ.DataDto.HandlMessage;
using Newtonsoft.Json;

namespace KQ.Services.HandlMessageService
{
    public interface IHandlMessageService
    {
        public ResponseBase MessageByDay(MessgeByDayRequest request);
        public ResponseBase Delete(int id);
        ResponseBase CountByDayRequest(CountByDayRequest request);
        ResponseBase CountByManyDayRequest(CountByManyDayRequest request);
        ResponseBase MessageByID(int messageID);
        ResponseBase HandleMessage(MessgeByDayRequest request);
        ResponseBase DeleteMulti(DeleteMultiRequest dto);
    }
}
