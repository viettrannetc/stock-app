using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Common
{
    public static partial class CongThuc
    {
        /// <summary>
        /// Tình huống:
        ///     CP đánh giá là mua được khi
        ///     1 - CP trước đó rơi vô đà giảm                                   - Chú ý mức độ 1
        ///     2 - Sau đà giảm, CP đã bật tăng lại từ bands dưới lên trên MA 20 - Chú ý mức độ 2
        ///     3 - Sau khi bật tăng lên trên MA 20, sẽ có 1 giai đoạn đi ngang tích lũy hoặc test vùng giá mới - Chú ý mức độ 3
        ///         + lúc này có thể giảm về MA 20 hoặc duy trì đi ngang ở đỉnh - ko có vấn đề) 
        ///         + chú ý cả tt chung nữa, nếu tt chung cũng đang tích lũy thì càng tốt
        ///     4 - Trong thời gian tích lũy, sẽ có lúc vol tăng cao với giá tăng cao, đây sẽ là lúc vô hàng - Chú ý mức độ 4        
        /// 
        /// Chi tiết:
        ///     1 + Tìm ra cây nến cuối cùng có nến bot vượt ra ngoài bands dưới gọi là A1
        ///       + Tìm ra cây nến cuối cùng trước nến A1 có giá cao nhất trong vòng 60 phiên (12 ngày - 1 ngày 5 phiên) gọi là nến A
        ///       + Giá từ nến top A giảm xuống nến bot A1 phải lớn hơn 20%
        ///     
        ///     2 + Tìm ra cây nến cuối cùng có nến bot vượt ra ngoài bands dưới gọi là A
        ///       + Tìm ra cây nến cuối cùng sau nến A1 và có nen top vượt lên trên MA 20 là nến A1
        ///       
        ///     3 + Tìm ra cây nến đầu tiên có nến top vượt lên trên MA 20 gọi là nến A
        ///       + Tìm ra tất cả những cây nến sau cây nến A tới hiện tại
        ///             - Từ trong DS này, lọc ra những cây nến cuối cùng (tính từ hiện tại trở về trước) nơi mà giá nến top thay đổi không quá 2% so với MA 20
        /// </summary>
        public static LocCoPhieuFilterRequest CT2G = new LocCoPhieuFilterRequest("CT2G")
        {
            Confirmed = true,
            Note = "CT2G - Bắt đầu theo dõi thôi, chưa gợi ý mua ngay. Để ý né mây xấu và MACD cắt xuống Signal trong 3 phiên trước. chung quy lại là nến đỏ ngoài ba",
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "BandsBot", Property2 = "C", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "O", Property2 = "C", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
            },
        };

        /*
         *  Chú ý: có 1 số mã sẽ có 1 vài cách chơi riêng biệt
         *      - PVD, BSR, PVC, VCG, CEO, KBC:
         *          D - Tenkan cắt hẳn lên Kijun thì mua cây sau, lúc chạm Kijun cũng chưa mua, đợi vượt hẳn rùi hãy chơi
         * Xác định 1 số CP để chơi lâu dài
         *      + BDS:
         *          CEO, DIG, DXG, KBC
         *      + Dầu
         *          PVD, BSR
         *      + DTC
         *          VCG, HHV, FCN
         *      + Thép
         *          NKG, HPG, HSG
         *      + CK
         *          CTS, HCM, SSI, VCI, VND
         *      + NH
         *          ACB, BID, CTG, LPB, MBB, STB, VIB, VPB
         *      + Nông
         *          DBC, HAG, ASM
         *      + Cao su
         *          GVR (>25k), PHR
         *      + Năng Lượng
         *          POW (>30k), GEG (>15k)
         *          
         *  Cách theo dõi chart từng CP
         *      + Chart H
         *          - Có PKD/PKA gì chưa -> mua or bán or giữ
         *          - Có đang down trend và cbi bắt dc chưa -> giảm từ đỉnh > 20%, giá chạm đáy liên hồi, chờ vol nổ để cứu thì sẽ tham gia
         *          - nếu đang sideway, thì mua giá bands dưới +- 1/2 lines, bán ở bands trên +-1/2 lines
         *          - Giá đang bám quanh MA 20 hoặc MA 10
         *      + Chart D
         *          DK Bình thường
         *              - Có PKD/PKA gì chưa -> mua or bán or giữ
         *              - Có đang down trend và cbi bắt dc chưa -> giảm từ đỉnh > 20%, giá chạm đáy liên hồi, chờ vol nổ để cứu thì sẽ tham gia
         *              - nếu đang sideway, thì mua giá bands dưới +- 1/2 lines, bán ở bands trên +-1/2 lines
         *              - Giá đang bám quanh MA 20 hoặc MA 10
         *          
         *          DK ưu tiên sau khi DK 1 thỏa
         *              - Mây ichimoku, đang trên mây ? -> giữ được, hoặc canh mua giá khi về mây cân bằng (tenkan hoặc mây flat)
         *              - Mây ichimoku, đang trong mây ? -> sẽ có rung lắc, không nên full tiền, đặt mua 10-20% quanh giá mây flat dưới
         *              - Mây ichimoku, đang dưới mây ? -> giữ được, hoặc canh mua giá khi về mây cân bằng (tenkan hoặc mây flat)
         *              - Coi Ami
         *          
         *          Nếu có tín hiệu mua thì canh mua theo chart H
         *          
         *      + Chart W
         *          DK Bình thường
         *              - Có PKD/PKA gì chưa -> mua or bán or giữ
         *              - Có đang down trend và cbi bắt dc chưa -> giảm từ đỉnh > 20%, giá chạm đáy liên hồi, chờ vol nổ để cứu thì sẽ tham gia
         *              - nếu đang sideway, thì mua giá bands dưới +- 1/2 lines, bán ở bands trên +-1/2 lines
         *              - Giá đang bám quanh MA 20 hoặc MA 10
         *          
         *          DK ưu tiên sau khi DK 1 thỏa
         *              - Mây ichimoku, đang trên mây ? -> giữ được, hoặc canh mua giá khi về mây cân bằng (tenkan hoặc mây flat)
         *              - Mây ichimoku, đang trong mây ? -> sẽ có rung lắc, không nên full tiền, đặt mua 10-20% quanh giá mây flat dưới
         *              - Mây ichimoku, đang dưới mây ? -> giữ được, hoặc canh mua giá khi về mây cân bằng (tenkan hoặc mây flat)
         *              - Coi Ami
         *          
         *          Nếu chart D đang sideway thì canh mua theo chart D, ko mua full tiền, vì tuần rất dài, trong tuần có thể bị call margin
         *          
         *      + Chart M
         *          DK Bình thường
         *              - Có PKD/PKA gì chưa -> mua or bán or giữ
         *              - Có đang down trend và cbi bắt dc chưa -> giảm từ đỉnh > 20%, giá chạm đáy liên hồi, chờ vol nổ để cứu thì sẽ tham gia
         *              - nếu đang sideway, thì mua giá bands dưới +- 1/2 lines, bán ở bands trên +-1/2 lines
         *              - Giá đang bám quanh MA 20 hoặc MA 10
         *          
         *          DK ưu tiên sau khi DK 1 thỏa
         *              - Mây ichimoku, đang trên mây ? -> giữ được, hoặc canh mua giá khi về mây cân bằng (tenkan hoặc mây flat)
         *              - Mây ichimoku, đang trong mây ? -> sẽ có rung lắc, không nên full tiền, đặt mua 10-20% quanh giá mây flat dưới
         *              - Mây ichimoku, đang dưới mây ? -> giữ được, hoặc canh mua giá khi về mây cân bằng (tenkan hoặc mây flat)
         *              - Coi Ami
         *          
         *          Nếu chart D đang sideway thì canh mua theo chart W, ko mua full tiền, vì month rất dài, trong tháng có thể bị call margin
         *          
         *          

         *
         *
         */

    }
}
