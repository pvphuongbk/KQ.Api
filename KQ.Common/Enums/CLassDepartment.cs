using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQ.Common.Enums
{
    public enum ClassDepartment
    {
        [Description("Tổng Giám Đốc")]
        TGĐ = 9,
		[Description("Trưởng Ban")]
        TB = 8,
		[Description("Giám Đốc")]
        GĐ = 7,
        [Description("Phó Ban")]
        PB = 6,
        [Description("Phó Giám Đốc")]
        PGĐ = 5,
        [Description("Trợ lý")]
        TL = 4,
        [Description("Trưởng Phòng")]
        TP = 2,
        [Description("Nhân Viên")]
        NV = 0
    }
}
