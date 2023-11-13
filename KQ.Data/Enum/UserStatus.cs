using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.DataDto.Enum
{
    public enum UserStatus
    {
        [Description("Hoạt động")]
        Active = 0,
        [Description("Sắp hết hạn")]
        ToExpire = 1,
        [Description("Hết hạn")]
        Expire = 2,
        [Description("Chưa dùng")]
        NotUse = 3,
    }
}
