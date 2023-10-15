using System.ComponentModel;

namespace KQ.DataDto.Enum
{
    public enum LotteryType
    {
        [Description("Bao lô")]
        Lo = 0,
        [Description("Đá xiên")]
        Xien = 1,
        [Description("Đầu")]
        LoDau = 2,
        [Description("Đuôi")]
        LoDuoi = 3,
        [Description("Đầu đuôi")]
        LoDauDuoi = 4,
        [Description("Bao ba con")]
        BaoBaCang = 5,
        [Description("Ba con đầu")]
        BaCangDau = 6,
        [Description("Ba con đuôi")]
        BaCangDuoi = 7,
        [Description("Xỉu chủ")]
        BaCangDauDuoi = 8,
        [Description("Bốn số")]
        BaoBonSo = 9,
        [Description("Đá xiên 2 đài")]
        DaXien = 10,
        [Description("Bao Đảo")]
        BaoDao = 11,
        [Description("Xỉu chủ đảo")]
        XCDao = 12,
        [Description("Xỉu chủ đầu đảo")]
        XCDauDao = 13,
        [Description("Xỉu chủ đuôi đảo")]
        XCDuoiDao = 14,
    }
}