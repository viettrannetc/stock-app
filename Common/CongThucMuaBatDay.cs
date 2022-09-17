using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Common
{
    public static partial class CongThuc
    {
        /// <summary>
        /// CT1 bắt đáy cbi cho sóng hồi          (bán nhanh ở T3 nếu thị trường yếu)
        /// 
        /// NenTangGia = true,
        /// MACDTangLienTucTrongNPhien = 1,         (chỉ vừa bật hum wa, hum nay múc thôi, để mấy ngày sau khi tăng thì ko kịp hồi T3)
        /// MACDMomentumTangLienTucTrongNPhien = 1, (chỉ vừa bật hum wa, hum nay múc thôi, để mấy ngày sau khi tăng thì ko kịp hồi T3)
        /// NenTopSoVoiGiaMA5 = new LocCoPhieuFilter { Ope = SoSanhEnum.LonHonHoacBang },
        /// NenBotSoVoiGiaMA5 = new LocCoPhieuFilter { Ope = SoSanhEnum.NhoHon },
        /// ==> Ko hiệu quả 
        /// </summary>
        //public static LocCoPhieuFilterRequest CT2A = new LocCoPhieuFilterRequest("CT2A")
        //{
        //    GiaSoVoiDinhTrongVong40Ngay = new LocCoPhieuFilter { Value = 0.7M, Ope = SoSanhEnum.NhoHonHoacBang },
        //    CachDayThapNhatCua40NgayTrongVongXNgay = 10,
        //    PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
        //        new LocCoPhieuCompareModel { Property1 = "C", Property2 = "O", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
        //        new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.Bang, Result = 1  },
        //        new LocCoPhieuCompareModel { Property1 = "MACDMomentum", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.Bang, Result = 1  },
        //        new LocCoPhieuCompareModel { Property1 = "NenTop", Property2 = "GiaMA05", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHonHoacBang, Result = 0  },
        //        new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "GiaMA05", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
        //    },
        //};


        /// <summary>
        /// CT2 bắt đáy cbi cho sóng hồi kĩ thuật (bán nhanh ở T3 nếu thị trường yếu)
        /// 
        /// MA20TiLeVoiM5 = new LocCoPhieuFilter { Ope = SoSanhEnum.LonHonHoacBang, Value = 1.09M },
        /// RSITangLienTucTrongNPhien = 1,
        /// MACDMomentumTangDanSoVoiNPhien = 1
        /// ==> Ko hiệu quả 
        /// </summary>
        public static LocCoPhieuFilterRequest CT2B = new LocCoPhieuFilterRequest("CT2B")
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "RSI", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.Bang, Result = 1  },
                new LocCoPhieuCompareModel { Property1 = "MACDMomentum", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.Bang, Result = 1  },
                new LocCoPhieuCompareModel { Property1 = "BandsMid", Property2 = "GiaMA05", Operation = OperationEnum.Divide, Sign = SoSanhEnum.LonHonHoacBang, Result = 1.09M  }
            }
        };

        /// <summary>
        /// CT2 bắt đáy trong sóng hồi kĩ thuật ở thị trường đi ngang, ví dụ VHM, VNM, đa phần đi ngang, thành ra giá chạm bands dưới hay bật về bands trên
        /// Biên độ giao động thấp (đỉnh/đáy chỉ cách nhau 1 cây CE/FL) - bán nhanh ở T3 khi lời > 1%
        /// 
        /// TODO: kiểm tra tính khả thi, vì có khả năng chỉ mua trong cây hồi trong ngày dc thôi
        ///         Có thể cải tiến bẳng việc nhắc ở cây trước, canh trong phiên nếu giá chạm về bands dưới thì đặt mua sẵn
        /// 
        /// MA20TiLeVoiM5 = new LocCoPhieuFilter { Ope = SoSanhEnum.LonHonHoacBang, Value = 1.035M },
        /// RSITangLienTucTrongNPhien = 1,       chỉ vừa bật hum wa, hum nay múc thôi, để mấy ngày sau khi tăng thì ko kịp hồi T3
        /// MACDMomentumTangDanSoVoiNPhien = 1,  chỉ vừa bật hum wa, hum nay múc thôi, để mấy ngày sau khi tăng thì ko kịp hồi T3
        /// ĐuôiNenThapHonBandDuoi = true,       ra ngoài bands dưới bật lại bands giữa
        /// ChieuDaiThanNenSoVoiRau = new LocCoPhieuFilter { Ope = SoSanhEnum.LonHonHoacBang, Value = 3.5M },   Thân nên nhỏ chứng tỏ tt chưa vô nhiều, để mai họ mới để ý, mình sẽ vô sớm
        /// NenTangGia = true
        /// 
        /// Cẩn thận vẽ đường xu hướng để canh ăn góc bật lên
        /// ==> Ko hiệu quả 
        /// </summary>
        public static LocCoPhieuFilterRequest CT2C = new LocCoPhieuFilterRequest("CT2C")
        {
            //PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
            //    new LocCoPhieuCompareModel { Property1 = "BandsMid", Property2 = "GiaMA05", Operation = OperationEnum.Divide, Sign = SoSanhEnum.LonHonHoacBang, Result = 1.035M  },
            //    new LocCoPhieuCompareModel { Property1 = "RSI", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.Bang, Result = 1  },
            //    new LocCoPhieuCompareModel { Property1 = "MACDMomentum", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.Bang, Result = 1  },
            //    new LocCoPhieuCompareModel { Property1 = "L", Property2 = "BandsBot", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
            //    new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "BandsBot", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
            //    new LocCoPhieuCompareModel { Property1 = "C", Property2 = "O", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  }
            //},
            //ChieuDaiThanNenSoVoiRau = new LocCoPhieuFilter { Ope = SoSanhEnum.LonHonHoacBang, Value = 3.5M },
            //BienDoBands10PhanTram = true

            //PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
            //    new LocCoPhieuCompareModel { Property1 = "BandsMid", Property2 = "GiaMA05", Operation = OperationEnum.Divide, Sign = SoSanhEnum.LonHonHoacBang, Result = 1.035M  },
            //    new LocCoPhieuCompareModel { Property1 = "RSI", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.Bang, Result = 1  },
            //    new LocCoPhieuCompareModel { Property1 = "MACDMomentum", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.Bang, Result = 1  },
            //    new LocCoPhieuCompareModel { Property1 = "L", Property2 = "BandsBot", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
            //    new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "BandsBot", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
            //    new LocCoPhieuCompareModel { Property1 = "C", Property2 = "O", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  }
            //},
            //ChieuDaiThanNenSoVoiRau = new LocCoPhieuFilter { Ope = SoSanhEnum.LonHonHoacBang, Value = 3.5M },
            BienDoBandsHep = true
        };

        /// <summary>
        /// /*
        /// * CT 1 - CT Bắt đáy khi giảm mạnh - tính từ ngày giá giảm liên tục tới hiện tại, nếu giá đã giảm >= 20%, thì bắt đầu mua vô
        /// *      + Tính RSI hiện tại, đếm ngược lại những ngày trước đó mà RSI vẫn đang giảm và các nến đều là nến đỏ
        /// *      + Đi ngược lại tìm nến cao nhất
        /// * + Tính từ giá đóng của của cây xanh cao nhất, so với giá hiện tại, nếu hiện tại giá đã giảm > 20%
        /// * -> Giá gợi ý mua từ [giá đóng của hum nay - 1/2 thân nến hôm nay] tới [giá C hum nay + 1/5 thân nên hum nay], tuyệt đối ko mua nếu giá mở cửa có tạo GAP cao hơn giá mở cửa của phiên hum nay
        /// */
        public static LocCoPhieuFilterRequest CT2D = new LocCoPhieuFilterRequest("CT2D")
        {
            Confirmed = true,
            Note = "CT2D - Bắt đầu theo dõi thôi, chưa gợi ý mua ngay. Để ý né mây xấu và MACD cắt xuống Signal trong 3 phiên trước. Theo dõi giá hum sau chart h và lệnh mua/bán, nếu giá rớt liên tục hoặc ko có vol, thì bỏ ko mua, chỉ mua nếu xuất hiện vol bắt đáy lớn (> 50%) và giá ở sàn + maximum 1%, và chỉ mua tối đa 30%, để dành 70% còn lại để trung bình giá nếu giá tiếp tục sàn ở những phiên sau hoặc khi hàng T3 về. Đây là nhịp hồi kĩ thuật nên chỉ mong đợi 2-5% trong T+5-10.",
            BatDay1 = true,
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "C", Operation = OperationEnum.SoSanh, Sign = SoSanhEnum.NhoHonHoacBang, Result = 40000  },
            },
        };

        /// <summary>
        /// các phiên giảm liên lục, chưa có dấu hiệu tạo đáy, chỉ bắt dao rơi theo tỉ lệ những cây giảm liên tục
        /// gợi ý giá mua ở cây đáy tiếp theo tới nửa thân
        /// </summary>
        public static LocCoPhieuFilterRequest CT2E = new LocCoPhieuFilterRequest("CT2E")
        {
            Confirmed = true,
            Note = "CT2E - Bắt đầu theo dõi thôi, chưa gợi ý mua ngay. Để ý né mây xấu và MACD cắt xuống Signal trong 3 phiên trước. Theo dõi giá hum sau chart h và lệnh mua/bán, nếu giá rớt liên tục hoặc ko có vol, thì bỏ ko mua, chỉ mua nếu xuất hiện vol bắt đáy lớn (> 50%) và giá ở sàn + maximum 1%, và chỉ mua tối đa 30%, để dành 70% còn lại để trung bình giá nếu giá tiếp tục sàn ở những phiên sau hoặc khi hàng T3 về. Đây là nhịp hồi kĩ thuật nên chỉ mong đợi 2-5% trong T+5-10.",
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "RSI", Operation = OperationEnum.ThayDoiGiamNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 7  },
                new LocCoPhieuCompareModel { Property1 = "RSI", Operation = OperationEnum.SoSanh, Sign = SoSanhEnum.NhoHonHoacBang, Result = 30  },
                new LocCoPhieuCompareModel { Property1 = "C", Operation = OperationEnum.SoSanh, Sign = SoSanhEnum.NhoHonHoacBang, Result = 40000  },
            },
        };

        /// <summary>
        /// Nến giảm sát hoặc ngoài bands bot, mong đợi 1 cây bật ngược lại ở ngày mai nếu giá xanh và
        /// 1 - vol lớn hơn 100% MA 20 tại bất cứ giờ nào trong ngày tiếp theo thì đặt mua giá bands bot - 
        /// 2 - vol lớn hơn  80% MA 20 tại bất cứ giờ nào trong ngày tiếp theo thì đặt mua giá bands bot - thân nến xanh phải dài hơn 80% thân nến đỏ hôm nay
        /// 
        /// ==> Chung quy lại là nến đỏ ngoài bands bot, cần theo dõi phiên ngày mai nếu trong 30p đầu phiên mà xuất hiện nến xanh, vol > 1.5 MA 20 của chart h thì múc giá bands bot, càng thấp càng tốt
        /// </summary>
        public static LocCoPhieuFilterRequest CT2F = new LocCoPhieuFilterRequest("CT2F")
        {
            Confirmed = true,
            Note = "CT2F - Bắt đầu theo dõi thôi, chưa gợi ý mua ngay. Để ý né mây xấu và MACD cắt xuống Signal trong 3 phiên trước. chung quy lại là nến đỏ ngoài bands bot, cần theo dõi phiên ngày mai nếu trong 30p đầu phiên mà xuất hiện nến xanh, vol > 1.5 MA 20 của chart h thì múc giá bands bot, càng thấp càng tốt",
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "BandsBot", Property2 = "C", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "O", Property2 = "C", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
            },
        };


        //public static LocCoPhieuFilterRequest CT2G = new LocCoPhieuFilterRequest("CT2G")        //RSI Phân kì
        //{
        //    RSIPhanKyTang = true
        //};

        /*
         * Mẫu mới:
         *      Đã làm rồi mà chưa tích hợp
         *      + MACD phân kỳ dương (Đã làm chưa tích hợp)
         *      + RSI  phân kì dương (Đã làm chưa tích hợp)
         *  
         *      Chưa làm
         *      + Giá giảm mạnh -> xuất hiện giá thấp nhất trong vòng 30 phiên -> nếu thấy xuất hiện nến đảo chiều (*) -> kiểm tra MACD, so giá trong ngày có MACD thấp nhất, nếu MACD hiện tại cao hơn thì báo cbi vô tiền
         *              Ví dụ: APG ngày 21/6/22
         *                     -> Trước ngày 20/21 /6/22 -> thị trường giảm mạnh
         *                     -> Sau ngày 20 -> 1 số CP đã cbi quay đầu
         *                     -> Sau ngày 21 -> gần như bộ thị trường quay đầu (đây có thể là phiên hồi kĩ thuật nhẹ)
         *              (*) - Nến đảo chiều
         *                      - Spin bar, râu trên cao hơn đuôi, thân bé bằng <= 1/4 râu + đuôi, 
         *                          + màu đỏ (nến búa ngược ko tính - PVD 21/6, 22/6, spin bar dưới giữa thân nến cũng được - OIL - 22/6)
         *                          + màu xanh thì sao cũng dc (PAS 22/6)
         *                      - Nến Bao phủ tăng
         *                      
         *                      
         *      
         */
    }
}
