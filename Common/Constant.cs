using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Common
{
    public static class ConstantData
    {
        public static int minMA20VolDaily = 800000;
        public static int minMA20VolHourly = 150000;//300000

        public static List<string> Condition = new List<string> { "True", "true", "1" };

        public static List<string> BadCodes = new List<string> {
            "FLC", "ROS", "ART", "AMD",
            "SHB", //vẽ chart sàn HNX
            "SHS", "ITA", "BII",
            "CTF", "IBC",
            "DHM", "HTP", //lãnh đạo bán cổ phiếu sau khi tăng - 22/8/22
            "OGC", "SSB",
            "DDG", "HAI",
            "IBC",
            "OGC",          //chỉ giao dich buoi chieu
            "KLF"

        };

        public const string CT01 = "Trend Giảm";
        public const string CT02 = "Đáy 2";
        public const string CT03 = "Theo dõi giá Giảm mạnh";
        public const string CT04 = "Sideway 3 phiên trước";
        public const string CT05 = "Hôm nay VOL Tăng";
        public const string CT06 = "Hôm nay VOL Giảm";
        public const string CT07 = "Hôm nay Giá Tăng";
        public const string CT08 = "Hôm nay Giá Giảm";
        public const string CT09 = "Vol Giảm 3 phiên trước";
        public const string CT10 = "Vol 3 phiên trước biến động <10%";
        public const string CT11 = "Tăng hơn 10% 3 phiên liên tục";

        public const string CT12 = "T3 bán có lời";

        /* Nến xác nhận đáy: ngày sau đáy 2 
            Nến đáy 2: đáy 2  
        */
        public const string CT13 = "Nến xác nhận đáy có Vol > tb 20 phiên";
        public const string CT14 = "Nến xác nhận đáy có CP < OP < CP * 1.02";
        public const string CT15 = "Nến xác nhận đáy có OP * 1.04 < CP";
        //public const string CT16 = "Nến xác nhận đáy có Vol < tb 20 phiên";
        public const string CT17 = "Nến xác nhận đáy có HP < CP * 1.01";

        public const string CT18 = "Nến đáy 2 có Vol > tb 20 phiên";
        //public const string CT19 = "Nến đáy 2 có Vol < tb 20 phiên";
        public const string CT20 = "Nến đáy 2 có CP < OP < CP * 1.02";
        public const string CT21 = "Nến đáy 2 có HP < CP * 1.01";

        public const string CT22 = "Price T4 -> T10 > CP X 1.1";

        public const string CT23 = "Có đáy 1";
        public const string CT24 = "Có đáy 2";

        public static class NameEn
        {
            public const string doanhThuThuan = "Net revenue";
            public const string loiNhuanGop = "Gross profit";
            public const string lnthuanTuHdKinhDoanh = "Operating profit";
            public const string lnstTuThuNhapDoanhNghiep = "Profit after tax";
            /// <summary>
            /// Normal, Bank, HAC
            /// </summary>
            public static List<string> lnstTuTCDCtyMe = new List<string> {
                "Net profit",
                "XV. Net profit atttributable to the equity holders of the Bank (XIII-XIV)",
                "Profit after tax for shareholders of parent company",
                "XV. Net profit atttributable to the equity holders of the Bank ",
                "11.1. Profit after tax for shareholders of the parents company",
            };
            public const string tsNganHan = "Current assets";
            public const string tongTs = "Total assets";
            public const string NoPhaiTra = "Liabilities";
            public const string NoNganHan = "Short -term liabilities";
            public const string VonChuSoHuu = "Owner's equity";
            public const string EPS4QuyGanNhat = "Trailing EPS";
            public const string BVPSCoBan = "Book value per share";
            public const string PECoBan = "P/E";
            public const string Ros = "ROS";
            public const string Roea = "ROEA";
            public const string RoAA = "ROAA";
            public const string LuuChuyenTienThuanTuHDKD = "Net cash flows from operating activities";
        }

        public static class TimeQuarter
        {
            public static List<Tuple<int, int, int>> LstOfQuarters = new List<Tuple<int, int, int>> {
                new Tuple<int, int, int> (1, 1, 2000),
                new Tuple<int, int, int> (2, 2, 2000),
                new Tuple<int, int, int> (3, 3, 2000),
                new Tuple<int, int, int> (4, 4, 2000),
                new Tuple<int, int, int> (5, 1, 2001),
                new Tuple<int, int, int> (6, 2, 2001),
                new Tuple<int, int, int> (7, 3, 2001),
                new Tuple<int, int, int> (8, 4, 2001),
                new Tuple<int, int, int> (9, 1, 2002),
                new Tuple<int, int, int> (10, 2, 2002),
                new Tuple<int, int, int> (11, 3, 2002),
                new Tuple<int, int, int> (12, 4, 2002),
                new Tuple<int, int, int> (13, 1, 2003),
                new Tuple<int, int, int> (14, 2, 2003),
                new Tuple<int, int, int> (15, 3, 2003),
                new Tuple<int, int, int> (16, 4, 2003),
                new Tuple<int, int, int> (17, 1, 2004),
                new Tuple<int, int, int> (18, 2, 2004),
                new Tuple<int, int, int> (19, 3, 2004),
                new Tuple<int, int, int> (20, 4, 2004),
                new Tuple<int, int, int> (21, 1, 2005),
                new Tuple<int, int, int> (22, 2, 2005),
                new Tuple<int, int, int> (23, 3, 2005),
                new Tuple<int, int, int> (24, 4, 2005),
                new Tuple<int, int, int> (25, 1, 2006),
                new Tuple<int, int, int> (26, 2, 2006),
                new Tuple<int, int, int> (27, 3, 2006),
                new Tuple<int, int, int> (28, 4, 2006),
                new Tuple<int, int, int> (29, 1, 2007),
                new Tuple<int, int, int> (30, 2, 2007),
                new Tuple<int, int, int> (31, 3, 2007),
                new Tuple<int, int, int> (32, 4, 2007),
                new Tuple<int, int, int> (33, 1, 2008),
                new Tuple<int, int, int> (34, 2, 2008),
                new Tuple<int, int, int> (35, 3, 2008),
                new Tuple<int, int, int> (36, 4, 2008),
                new Tuple<int, int, int> (37, 1, 2009),
                new Tuple<int, int, int> (38, 2, 2009),
                new Tuple<int, int, int> (39, 3, 2009),
                new Tuple<int, int, int> (40, 4, 2009),
                new Tuple<int, int, int> (41, 1, 2010),
                new Tuple<int, int, int> (42, 2, 2010),
                new Tuple<int, int, int> (43, 3, 2010),
                new Tuple<int, int, int> (44, 4, 2010),
                new Tuple<int, int, int> (45, 1, 2011),
                new Tuple<int, int, int> (46, 2, 2011),
                new Tuple<int, int, int> (47, 3, 2011),
                new Tuple<int, int, int> (48, 4, 2011),
                new Tuple<int, int, int> (49, 1, 2012),
                new Tuple<int, int, int> (50, 2, 2012),
                new Tuple<int, int, int> (51, 3, 2012),
                new Tuple<int, int, int> (52, 4, 2012),
                new Tuple<int, int, int> (53, 1, 2013),
                new Tuple<int, int, int> (54, 2, 2013),
                new Tuple<int, int, int> (55, 3, 2013),
                new Tuple<int, int, int> (56, 4, 2013),
                new Tuple<int, int, int> (57, 1, 2014),
                new Tuple<int, int, int> (58, 2, 2014),
                new Tuple<int, int, int> (59, 3, 2014),
                new Tuple<int, int, int> (60, 4, 2014),
                new Tuple<int, int, int> (61, 1, 2015),
                new Tuple<int, int, int> (62, 2, 2015),
                new Tuple<int, int, int> (63, 3, 2015),
                new Tuple<int, int, int> (64, 4, 2015),
                new Tuple<int, int, int> (65, 1, 2016),
                new Tuple<int, int, int> (66, 2, 2016),
                new Tuple<int, int, int> (67, 3, 2016),
                new Tuple<int, int, int> (68, 4, 2016),
                new Tuple<int, int, int> (69, 1, 2017),
                new Tuple<int, int, int> (70, 2, 2017),
                new Tuple<int, int, int> (71, 3, 2017),
                new Tuple<int, int, int> (72, 4, 2017),
                new Tuple<int, int, int> (73, 1, 2018),
                new Tuple<int, int, int> (74, 2, 2018),
                new Tuple<int, int, int> (75, 3, 2018),
                new Tuple<int, int, int> (76, 4, 2018),
                new Tuple<int, int, int> (77, 1, 2019),
                new Tuple<int, int, int> (78, 2, 2019),
                new Tuple<int, int, int> (79, 3, 2019),
                new Tuple<int, int, int> (80, 4, 2019),
                new Tuple<int, int, int> (81, 1, 2020),
                new Tuple<int, int, int> (82, 2, 2020),
                new Tuple<int, int, int> (83, 3, 2020),
                new Tuple<int, int, int> (84, 4, 2020),
                new Tuple<int, int, int> (85, 1, 2021),
                new Tuple<int, int, int> (86, 2, 2021),
                new Tuple<int, int, int> (87, 3, 2021),
                new Tuple<int, int, int> (88, 4, 2021),
                new Tuple<int, int, int> (89, 1, 2022),
                new Tuple<int, int, int> (90, 2, 2022),
                new Tuple<int, int, int> (91, 3, 2022),
                new Tuple<int, int, int> (92, 4, 2022),
                new Tuple<int, int, int> (93, 1, 2023),
                new Tuple<int, int, int> (94, 2, 2023),
                new Tuple<int, int, int> (95, 3, 2023),
                new Tuple<int, int, int> (96, 4, 2023),
            };
        }
    }
}
