using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Common
{
    public static partial class CongThuc
    {
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
        /// CT Tìm những cổ phiếu có nến đang tăng, cây nến đóng lên chạm MA 20, ma 5 dưới MA 20
        /// 
        /// NenTangGia = true,
        /// NenTopSoVoiGiaMA20 = new LocCoPhieuFilter { Ope = LocCoPhieuFilterEnum.LonHonHoacBang },
        /// NenBotSoVoiGiaMA20 = new LocCoPhieuFilter { Ope = LocCoPhieuFilterEnum.NhoHon },
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
                new LocCoPhieuCompareModel { Ngay = -1, Property1 = "NenBot", Property2 = "BandsBot", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
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

        public static LocCoPhieuFilterRequest CT1D = new LocCoPhieuFilterRequest("CT1D") //Kijun vs Tenkan
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                //phienHumNay.IchimokuTenKan.IchimokuKijun phienHumNay.IchimokuCloudTop
                //new LocCoPhieuCompareModel { Property1 = "C", Property2 = "O", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "IchimokuTenKan", Property2 = "IchimokuKijun", Operation = OperationEnum.CrossUp },
                new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "IchimokuCloudTop", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHonHoacBang, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "BandsTop", Property2 = "L", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  }

            },
        };

    }
}
