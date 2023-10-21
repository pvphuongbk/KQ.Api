using KQ.Data.Base;
using KQ.DataDto.Calculation;

namespace KQ.Services.CommonServices
{
    public interface ICommonService
    {
        public ResponseBase GetChanel(DateTime? date);
        public ResponseBase CheckKQ(DayOfWeek? day);
        public ResponseBase CheckChanelCode();
        public ResponseBase UnitTest();
        public ResponseBase UnitTestCal3();
    }
}
