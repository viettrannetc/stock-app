using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Common
{
    public static partial class CongThuc
    {

        /*
         * Công thức chung
         *      + Chart ngày / Chart tuần / Chart tháng
         *          + Đang trên đà tăng sau khi có MACD 2 đáy từ 6 phiên trước
         *          + Có phân kỳ dương RSI hay MACD từ 2 phiên trước
         *          + Có tín hiệu đảo chiều tăng như
         *              + nến đảo chiều
         *              + bao phủ tăng
         *              + phá vỡ tăng
         *              + giá đang tích lũy ở đáy
         *              + giá đang tích lũy trên MA 20
         *              + giá đang rơi tìm đáy
         *          => nên mua hay theo dõi
         *          - Nếu mua thì giá mua? - Ko cần xác định giá mua, giá mua tự coi trong phiên
         *          - Tỉ lệ đúng, sai -> tính T+10 - ăn > 3%? -> ngược lại lỗ
         *  
         *  
         * VND
         *      + Chart ngày
         *          + Trong vòng 16 ngày mới xuất hiện 1 đáy MACD, chưa có đáy 2
         *          + Không có Phân kỳ dương RSI hay MACD
         *          + Không có tín hiệu đảo chiều tăng như 
         *              + nến đảo chiều
         *              + bao phủ tăng
         *              + phá vỡ tăng
         *              + giá đang tích lũy ở đáy
         *              + giá đang tích lũy trên MA 20
         *              + giá đang rơi tìm đáy
         *      + Chart tuần
         *          + xuất hiện nến spin giữa - chưa rõ xu thế
         *      + Chart tháng
         *          + đang ngoi lên MA 20, chưa rõ ràng xu thế
         *      => ko mua
         *  
         *  
         */
    }
}
