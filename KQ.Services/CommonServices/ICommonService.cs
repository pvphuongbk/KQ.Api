using KQ.Data.Base;

namespace KQ.Services.CommonServices
{
    public interface ICommonService
    {
        public ResponseBase GetChanel(DateTime? date);
        public ResponseBase CheckKQ(DayOfWeek? day);
        public ResponseBase CheckChanelCode();
        public ResponseBase UnitTest();
    }
}
