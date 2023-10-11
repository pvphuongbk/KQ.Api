using KQ.Data.Base;
using KQ.DataDto.Calculation;

namespace KQ.Services.Calcualation
{
    public interface ICalcualationService
    {
        public ResponseBase Cal1Request(List<Cal1RequestDto> dtos);
        public ResponseBase Cal2Request(Cal2RequestDto syntaxes);
    }
}
