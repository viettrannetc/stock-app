using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Common
{
    public static partial class CongThuc
    {
        /* TODO
         * CT mới
         *      + Đặt mua theo biên độ dao động của giá trong TT đi ngang
         *      + Mua khi lái cbi đánh sau khi thị trường tích lũy 1 thời gian dài - IPA - 23/08/21 -> 08/09/21 - Biên độ dao động ngang chỉ < 7%, chờ 1 cây break
         */

        /* TODO
         * NToai
         *  CT1:
         *      Cây tăng t2 bật lên từ MA 20
         */

        /// <summary>
        /// CT Tìm những cổ phiếu có nến đang tăng, cây nến đóng lên chạm MA 20, ma 5 dưới MA 20
        /// 
        /// NenTangGia = true,
        /// NenTopSoVoiGiaMA20 = new LocCoPhieuFilter { Ope = LocCoPhieuFilterEnum.LonHonHoacBang },
        /// NenBotSoVoiGiaMA20 = new LocCoPhieuFilter { Ope = LocCoPhieuFilterEnum.NhoHon },
        /// 
        /// Nếu đếm ngược ngày, trong vòng 5 phiên trước mà xảy ra việc MA 5 cắt xuống MA 20, vậy thì bỏ qua CT này (CT1A1)
        ///         Ví dụ: + IPA: 9/6/22, 4/4/22 (4 Phiên)
        ///             + Tăng lên 5 phiên để tránh TH DRI 18/4/22, DRI 4/4/22
        ///             + SRC 6/2/22
        ///             + HSG 9/6/22
        ///             + ITQ 9/6/22, 6/1/22
        ///             + TDC 8/6/22
        /// Nếu trong vòng 4 cây nến trước, giá đã từng vượt MA 20 rùi mà bị bật ngược xuống dưới MA (có cây nến sau cây giá vượt MA ở dưới mA 20) thì lúc này (CT1A2)
        ///         + MA 20 dc xem như kháng cự, giá lình xình rất khó ăn T3
        ///         + Ví dụ: 
        ///             IPA: 23/3/22, LAS 19/4/22
        ///             PVB 5/4/22
        ///             Ngoại lệ: PVB 11/2/22 -> đồng ý là lỗ 1 tí nhưng sau đó tăng rất mạnh trong vòng 6 phiên tương lai, có thể thay đổi điều kiện cho TH này là nhưng nếu sau coi MA 20 là hỗ trợ
        ///             TDC 8/6/22, 5/4/22
        /// Nếu nến tăng mà cbi chạm span B thì thôi ko mua nữa, span B đang là kháng cự, ko vượt nỗi MA 20 và ko ăn đủ T3                                      (CT1A3A & CT1A3B) - có vẻ dùng làm KN thôi
        ///         Ví dụ: IPA 11/2/22
        ///         
        /// IPA 7/1/22 - Nến tăng trần, ichimoku mây xanh dày bên dưới, MACD cbi tạo đáy, giao cắt Signal, hướng lên, tenkan,kijun,ma5 hướng lên,               (CT1A4)
        ///     - MA 20 tăng, vol tăng mãnh liệt => mọi yếu tố đều ủng hộ tăng
        ///     - Nhưng:
        ///         + Wa ngày 8/1/22: đó là 1 cây nến doji giảm nhẹ ở ngay bands trên, râu rất dài, vol đỏ cũng > MA 20, MA 5 cắt lên MA 20, tenkan hunog71 lên chạm kijun trên mây => nhưng cây nến thể hiện sự đạp giá kinh khủng
        ///         => sau đó là nhiều phiên giảm trần liên tiếp .......
        ///     Phòng ngừa:
        ///         - C1: Nếu giá tăng mà khoảng cách từ nến top > bands top hoặc khoảng cách từ bands top -> nến top <= 1/5 thân nến thì bỏ đi, vì khó ăn dc T3                    (Không đúng trong TH CIG: 6/1 & 10/1/22)
        ///         - C2: MACD phân kì giảm? -> Giá so với đỉnh cao nhất trước đó trong vòng 60 ngày nếu chênh lệch trong 7%, và MACD hiện tại bé hơn MACD đỉnh cũ, thì cũng bỏ đi  (đúng)
        ///         - C3: RSI  phân kì giảm? -> Giá so với đỉnh cao nhất trước đó trong vòng 60 ngày nếu chênh lệch trong 7%, và RSI  hiện tại bé hơn RSI  đỉnh cũ, thì cũng bỏ đi  (đúng)
        ///         => Đây là tín hiệu gợi ý mua, cần theo dõi điều kiện trong phiên mai để ra lệnh mua
        /// 
        /// Nếu nến đột xuất hiện trong 1 nhiều nến giảm liên tiếp                          TODO: Không hiểu, dữ liệu cần kiểm tra lại                         
        ///         + có thể là RSI phân kỳ giảm: DDV 14/2/22 - chưa xác định
        ///         => thì nên bỏ đi
        /// 
        /// Nếu nến xuất hiện trong chu kì tăng từ đáy tới MA 20                                                                                                CT1A5
        ///     + Từ ngày hum nay quay trở lại ngày đáy > 7 phiên -> bỏ đi, vì tăng quá yếu, MA 20 lúc này sẽ là kháng cự rồi, trong 7 phiên rùi mà chưa chạm lại dc MA 20
        ///     + Nhưng cũng ko nên tăng quá 30%, 
        ///         Ví dụ: 
        ///             + HSG 27/05/22 - 08 thanh mà chỉ tăng 9%     - chết nặng
        ///             + SCR 30/05/22 - 09                   11-12% - chết nặng
        ///             + IPA 30/05/22 - 08                   14-15% - chết nặng
        ///             + ITQ 30/05/22 - 10                   16-17% - chết nặng
        ///             + TDC 11/02/22 - 06                   20.15% - chết nặng
        ///             + DDV 21/12/21 - 11                   05-06% - chết nặng
        ///             + HUT 16/05/22 - 05                   20-21% - SAU TĂNG NỮA LÊN TỚI 53% - Bands rộng rãi thoải mái cho tăng
        ///             + MSN 25/05/22 - 06                   14-15% - SAU TĂNG NỮA LÊN TỚI 24% - Bands rộng rãi thoải mái cho tăng
        /// 
        /// 
        /// ==> CÔNG THỨC NÀY CÓ KHI KO HOÀN THIỆN, TÌM CT THAY THẾ => /
        ///     GIÁ VƯỢT MA 20
        ///     MA CẮT LÊN MA 20
        ///     CANH MUA Ở GIÁ WAY NGƯỢC VỀ CHẠM TEST MA 20 LẦN ĐẦU TIÊN (TEST HỖ TRỢ) - cứ đặt mua ở giá đó đi vì có khi ăn râu nến
        ///         VD: 
        ///             TDC: 16/12/21
        /// 
        /// </summary>
        public static LocCoPhieuFilterRequest CT1A = new LocCoPhieuFilterRequest("CT1A")
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "NenTop", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHonHoacBang, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "C", Property2 = "O", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 1  },
                new LocCoPhieuCompareModel { Property1 = "C", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.NhoHon, Result = 4  },
            }
        };


        /// <summary>
        /// CT tìm những CP đang mạnh trên thị trường
        /// RSI = new LocCoPhieuFilter { Ope = SoSanhEnum.LonHon, Value = 60 },
        /// MacdSoVoiSignal = new LocCoPhieuFilter { Ope = SoSanhEnum.LonHon },
        /// Macd = new LocCoPhieuFilter { Ope = SoSanhEnum.LonHon, Value = 0 }
        /// </summary>
        public static LocCoPhieuFilterRequest CT0A = new LocCoPhieuFilterRequest("CT0A")
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "RSI", Operation = OperationEnum.SoSanh, Sign = SoSanhEnum.LonHonHoacBang, Result = 60  },
                new LocCoPhieuCompareModel { Property1 = "MACD", Property2 = "MACDSignal", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.SoSanh, Sign = SoSanhEnum.LonHon, Result = 0  }
            }
        };

        /// <summary>
        /// Biến thể từ CT1A nhưng 
        ///     Nếu đếm ngược ngày, trong vòng 5 phiên trước mà xảy ra việc MA 5 cắt xuống MA 20, vậy thì bỏ qua CT này
        ///     Ví dụ: + IPA: 9/6/22, 4/4/22 (4 Phiên)
        ///     + Tăng lên 5 phiên để tránh TH DRI 18/4/22, DRI 4/4/22
        ///     + SRC 6/2/22
        ///     + HSG 9/6/22
        ///     + ITQ 9/6/22, 6/1/22
        ///     + TDC 8/6/22
        /// </summary>
        public static LocCoPhieuFilterRequest CT1A1 = new LocCoPhieuFilterRequest("CT1A1", CT1A.PropertiesSoSanh)
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                //trong vong X phien, Y phien xuat hien DK Property A co operation B voi Property C
                new LocCoPhieuCompareModel { Phien = 0, Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.CrossDown, Result = -1 },
                new LocCoPhieuCompareModel { Phien = -1, Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.CrossDown, Result = -1 },
                new LocCoPhieuCompareModel { Phien = -2, Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.CrossDown, Result = -1 },
                new LocCoPhieuCompareModel { Phien = -3, Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.CrossDown, Result = -1 },
                new LocCoPhieuCompareModel { Phien = -4, Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.CrossDown, Result = -1 },
                new LocCoPhieuCompareModel { Phien = -5, Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.CrossDown, Result = -1 }
            }
        };

        /// <summary>
        /// Biến thể từ CT1A nhưng 
        /// Nếu trong vòng 4 cây nến trước, giá/râu trên đã từng vượt MA 20 rùi mà bị bật ngược xuống dưới MA (có cây nến sau cây giá vượt MA ở dưới mA 20) thì lúc này
        ///         + MA 20 dc xem như kháng cự, giá lình xình rất khó ăn T3
        ///         + Ví dụ: 
        ///             IPA: 23/3/22, LAS 19/04/22
        ///             PVB 5/4/22
        ///             Ngoại lệ: PVB 11/2/22 -> đồng ý là lỗ 1 tí nhưng sau đó tăng rất mạnh trong vòng 6 phiên tương lai, có thể thay đổi điều kiện cho TH này là nhưng nếu sau coi MA 20 là hỗ trợ
        ///                         + ko cần ngoại lệ 
        ///                             1 - NToai cũng ko khuyến nghị mã này ngày này
        ///                             2 - Có thể chờ tới 22/2/22, xuất hiện CT CT3, rùi mua ngày 23/2/22 cũng dc
        ///             TDC 8/6/22, 5/4/22
        /// </summary>
        public static LocCoPhieuFilterRequest CT1A2 = new LocCoPhieuFilterRequest("CT1A2", CT1A.PropertiesSoSanh)
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Phien = -1, Property1 = "H", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                new LocCoPhieuCompareModel { Phien = -2, Property1 = "H", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                new LocCoPhieuCompareModel { Phien = -3, Property1 = "H", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                new LocCoPhieuCompareModel { Phien = -4, Property1 = "H", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
            }
        };

        /// <summary>
        /// Biến thể từ CT1A nhưng 
        ///       nếu giá dưới mây, thì Mây dưới là kháng cự nhẹ, mây trên là kháng cự mạnh
        ///       nếu giá trên mây, thì Mây dưới là hỗ trợ mạnh, mây trên là hỗ trợ nhẹ
        ///       nếu giá trong mây, thì Mây dưới là hỗ trợ nhẹ, mây trên là kháng cự nhẹ
        /// Nếu nến tăng mà cbi chạm span B thì thôi ko mua nữa, span B đang là kháng cự, ko vượt nỗi MA 20 và ko ăn đủ T3                                      (CT1A3)
        ///     - Thực chất ra Span B là kháng cự nhẹ
        ///         Ví dụ: IPA 11/2/22
        /// </summary>
        public static LocCoPhieuFilterRequest CT1A3A = new LocCoPhieuFilterRequest("CT1A3A", CT1A.PropertiesSoSanh)
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "IchimokuBot", Property2 = "C", Operation = OperationEnum.Divide, Sign = SoSanhEnum.LonHonHoacBang, Result = 1.06M  }
            }
        };
        public static LocCoPhieuFilterRequest CT1A3B = new LocCoPhieuFilterRequest("CT1A3B", CT1A.PropertiesSoSanh)
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "IchimokuTop", Property2 = "C", Operation = OperationEnum.Divide, Sign = SoSanhEnum.LonHonHoacBang, Result = 1.06M  }
            }
        };

        /// <summary>
        /// Biến thể từ CT1A nhưng 
        /// IPA 7/1/22 - Nến tăng trần, ichimoku mây xanh dày bên dưới, MACD cbi tạo đáy, giao cắt Signal, hướng lên, tenkan,kijun,ma5 hướng lên,               (CT1A4)
        ///     - MA 20 tăng, vol tăng mãnh liệt => mọi yếu tố đều ủng hộ tăng
        ///     - Nhưng:
        ///         + Wa ngày 8/1/22: đó là 1 cây nến doji giảm nhẹ ở ngay bands trên, râu rất dài, vol đỏ cũng > MA 20, MA 5 cắt lên MA 20, tenkan hunog71 lên chạm kijun trên mây => nhưng cây nến thể hiện sự đạp giá kinh khủng
        ///         => sau đó là nhiều phiên giảm trần liên tiếp .......
        ///     Phòng ngừa:
        ///         - C1: Nếu giá tăng mà khoảng cách từ nến top > bands top hoặc khoảng cách từ bands top -> nến top <= 1/5 thân nến thì bỏ đi, vì khó ăn dc T3                    (Không đúng trong TH CIG: 6/1 & 10/1/22)
        ///         - C2: MACD phân kì giảm? -> Giá so với đỉnh cao nhất trước đó trong vòng 60 ngày nếu chênh lệch trong 7%, và MACD hiện tại bé hơn MACD đỉnh cũ, thì cũng bỏ đi  (đúng)
        ///         - C3: RSI  phân kì giảm? -> Giá so với đỉnh cao nhất trước đó trong vòng 60 ngày nếu chênh lệch trong 7%, và RSI  hiện tại bé hơn RSI  đỉnh cũ, thì cũng bỏ đi  (đúng)
        ///         => Đây là tín hiệu gợi ý mua, cần theo dõi điều kiện trong phiên mai để ra lệnh mua
        /// </summary>
        public static LocCoPhieuFilterRequest CT1A4 = new LocCoPhieuFilterRequest("CT1A4", CT1A.PropertiesSoSanh)
        {
            /*  Giá của ngày hiện tại so với giá đỉnh của ngày quá khứ trong vòng 60 ngày phải có sự tương quan
             *  Ví dụ:
             *      Giá ngày hiện tại so với đỉnh quá khứ   > : 
             *                                              = : 
             *                                              < : MUA: nếu Giá hiện tại <= Giá quá khứ * (1 - ((RSI quá khứ / RSI hiện tại) / 100)        - TODO: chưa làm
             *                                                  MUA: nếu Giá hiện tại <= Giá quá khứ * (1 - ((MACD quá khứ / MACD hiện tại) / 100)      - làm trong CT này
             *                                              
             */
            KiemTraGiaVsMACD = true
        };

        /// <summary>
        /// Biến thể từ CT1A nhưng 
        /// Nếu nến xuất hiện trong chu kì tăng từ đáy tới MA 20                                                                                                CT1A5
        /// + Từ ngày hum nay quay trở lại ngày đáy > 7 phiên -> bỏ đi, vì tăng quá yếu, MA 20 lúc này sẽ là kháng cự rồi, trong 7 phiên rùi mà chưa chạm lại dc MA 20
        /// + Nhưng cũng ko nên tăng quá 30%,
        ///     Ví dụ: 
        ///         + HSG 27/05/22 - 08 thanh mà chỉ tăng 9%     - chết nặng
        ///         + SCR 30/05/22 - 09                   11-12% - chết nặng
        ///         + IPA 30/05/22 - 08                   14-15% - chết nặng
        ///         + ITQ 30/05/22 - 10                   16-17% - chết nặng
        ///         + TDC 11/02/22 - 06                   20.15% - chết nặng
        ///         + DDV 21/12/21 - 11                   05-06% - chết nặng
        ///         + HUT 16/05/22 - 05                   20-21% - SAU TĂNG NỮA LÊN TỚI 53% - Bands rộng rãi thoải mái cho tăng
        ///         + MSN 25/05/22 - 06                   14-15% - SAU TĂNG NỮA LÊN TỚI 24% - Bands rộng rãi thoải mái cho tăng
        /// </summary>
        public static LocCoPhieuFilterRequest CT1A5 = new LocCoPhieuFilterRequest("CT1A5", CT1A.PropertiesSoSanh)
        {
            KiemTraTangManhTuDay = true
        };




        /// <summary>
        /// CT Tìm những cổ phiếu MA 5 đang tăng, MA 5 cắt lên trên MA 20, giá tăng lên chạm hoặc vượt wa MA 20, nhưng nến bắt đầu dưới MA 20
        /// 
        /// NenTangGia = true,
        /// MA5CatLenMA20 = true,
        /// NenBotSoVoiGiaMA20 = new LocCoPhieuFilter { Ope = LocCoPhieuFilterEnum.NhoHon }
        /// </summary>
        public static LocCoPhieuFilterRequest CT1B = new LocCoPhieuFilterRequest("CT1B")
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.CrossUp },
                new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "C", Property2 = "O", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 1  },
            },
        };

        /// <summary>
        /// Nếu giá MA 5 hướng lên, hiện tại râu nến trên đã chạm MA 20 rồi, nên và MA 5 chưa vượt MA 20
        /// Nên nếu nến ngày mai, giá mở cửa trên MA 20, thì canh mua từ [giá mở cửa + 1-2 line] xuống [ma 20] - có thể canh mua ở cuối phiên cho chắc cú
        /// 
        /// Đây chỉ là tín hiệu gợi ý nếu ngày mai đủ DK mua, ko phải chắc chắn nên mua
        /// 
        /// Ví dụ: MSN ngày 10 tháng 2 - 2022
        ///     - MACD 3 đáy nhỏ xíu trước đó, macd hướng lên 2 phiên liên tục
        ///     - giá 9 tháng 2/22 là doji dưới MA 20, râu nến top vượt mA 20, MA 5 đang hướng xuống nhẹ
        ///     - ngày 10 giá bật lên khỏi MA 20, vụt tăng 10%
        ///     - là nến tăng giá
        /// </summary>
        public static LocCoPhieuFilterRequest CT1B2 = new LocCoPhieuFilterRequest("CT1B2")
        {
            //MACD tang lien tuc trong XX ngày sau khi có đáy 2
            //Giá dưới MA 20
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "C", Property2 = "O", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Day2 = true, Property1 = "MACD", Operation = OperationEnum.TrongVong, Result = 2  },
                new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 1 },
                new LocCoPhieuCompareModel { Property1 = "NenTop", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  }
            },
        };


        public static LocCoPhieuFilterRequest CT1B3 = new LocCoPhieuFilterRequest("CT1B3")    //nến đảo chiều tăng ở ngoài bands dưới
        {
            NenBaoPhuDaoChieuManh = true,
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "BandsBot", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "H", Property2 = "GiaMA05", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                new LocCoPhieuCompareModel { Phien = -1, Property1 = "NenBot", Property2 = "BandsBot", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
            }
        };


        /// <summary>
        /// CT Tìm những cổ phiếu MA 5 đang tăng, giá tăng lên chạm hoặc vượt wa MA 20, nhưng nến bắt đầu dưới MA 20, MA 5 vẫn đang nằm dưới MA 20
        /// 
        /// NenTangGia = true,
        /// MA5TangLienTucTrongNPhien = 1, (tối thiểu 1 phiên)
        /// NenTopSoVoiGiaMA20 = new LocCoPhieuFilter { Ope = LocCoPhieuFilterEnum.LonHonHoacBang },
        /// NenBotSoVoiGiaMA20 = new LocCoPhieuFilter { Ope = LocCoPhieuFilterEnum.NhoHon },
        /// MA5SoVoiMA20 = new LocCoPhieuFilter { Ope = LocCoPhieuFilterEnum.NhoHon }
        /// </summary>
        public static LocCoPhieuFilterRequest CT1C = new LocCoPhieuFilterRequest("CT1C")
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "C", Property2 = "O", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "GiaMA05", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 1  },
                new LocCoPhieuCompareModel { Property1 = "NenTop", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHonHoacBang, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 1  },
                new LocCoPhieuCompareModel { Property1 = "C", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.NhoHon, Result = 4  },
            },
        };



    }
}
