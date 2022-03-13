﻿using System.Collections.Generic;

namespace DotNetCoreSqlDb.Common
{
    public static class ConstantData
    {
        public static List<string> Condition = new List<string> { "True", "true", "1" };

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
    }
}
