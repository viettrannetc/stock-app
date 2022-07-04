using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Common
{
    public static partial class CongThuc
    {
        







        /// <summary>
        /// * quyết định bởi số ngày nắm giữ và lợi nhuận mong muốn
        /// CT tìm những CP nên bán
        ///     Bắt buộc phải đủ T3
        ///     + OR Đủ kì vọng                                                                                                     CTB1A
        ///     + OR MA 5 cắt dưới MA 20 (chứa MA 5 giảm rồi)                                                                       CTB1B
        ///     + OR MA 5 giảm và thân nến chạm xuống MA 20                                                                         CTB1C1
        ///     + OR MA 5 giảm và đuôi nến chạm xuống MA 20                                                                         CTB1C2
        ///     + OR Xuất hiện nến đảo chiều giảm mạnh                                                                              CTB1D
        ///     +     - OR có nến bao phủ giảm thân dài hơn thân nến xanh trước là được                                             CTB1E
        ///     +         (Đang xem xét) - OR nến bao phủ giảm có thân nến dài hơn ít nhất 2 lần thân nến của cây nến tăng trước    CTB1F   (considering)
        ///     +         (Đang xem xét) - OR 2 cây giảm liên tiếp ở đỉnh                                                           CTB1G   (considering)
        /// </summary>
        public static LocCoPhieuFilterRequest CTB1A = new LocCoPhieuFilterRequest("CTB1A")
        {
            KiVong = new LocCoPhieuCompareModel { Property1 = "C", Result = 20.2M }
        };

        public static LocCoPhieuFilterRequest CTB1B = new LocCoPhieuFilterRequest("CTB1B")
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.CrossDown },
            }
        };

        public static LocCoPhieuFilterRequest CTB1C1 = new LocCoPhieuFilterRequest("CTB1C1")
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "GiaMA05", Operation = OperationEnum.ThayDoiGiamNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 1  },
                new LocCoPhieuCompareModel { Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "NenTop", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHonHoacBang, Result = 0  },
            }
        };

        public static LocCoPhieuFilterRequest CTB1C2 = new LocCoPhieuFilterRequest("CTB1C2")
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                //new LocCoPhieuCompareModel { Property1 = "GiaMA05", Operation = OperationEnum.ThayDoiGiamNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 1  },
                new LocCoPhieuCompareModel { Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "H", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "L", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHonHoacBang, Result = 0  },
            }
        };

        public static LocCoPhieuFilterRequest CTB1D = new LocCoPhieuFilterRequest("CTB1D")    //giảm thiểu rủi ro cho lợi nhuận đang có
        {
            NenBaoPhuDaoChieuManh = false
        };

        public static LocCoPhieuFilterRequest CTB1F = new LocCoPhieuFilterRequest("CTB1F")    //giảm thiểu rủi ro cho lợi nhuận đang có
        {
            NenBaoPhuDaoChieuTrungBinh = false
        };

        public static LocCoPhieuFilterRequest CTB1E = new LocCoPhieuFilterRequest("CTB1E")
        {
            KiVong = new LocCoPhieuCompareModel { Property1 = "C", Result = 0.95M }
        };


        public static List<LocCoPhieuFilterRequest> allCongThuc = new List<LocCoPhieuFilterRequest>()
        {
            CT1A, CT1B, CT1C, CT3, CT1B2, CT1B3,
            CT2B,CT2C, CT2D,CT2E, CT2F
        };
    }
}
