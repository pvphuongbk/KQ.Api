﻿using KQ.Data.Base;
using KQ.DataDto.Calculation;
using KQ.DataDto.HandlMessage;

namespace KQ.Services.HandlMessageService
{
    public interface IHandlMessageService
    {
        public ResponseBase MessageByDay(MessgeByDayRequest request);
    }
}