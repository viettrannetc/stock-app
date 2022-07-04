using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Common
{
    public static partial class CongThuc
    {

        /*
         * NT
         *  CT1:
         *      Cây tăng t2 bật lên từ MA 20
         */

        /// <summary>
        /// CT Tìm những cổ phiếu theo ichimoku
        /// 
        ///     - giá cắt xướng kijun -> bắt đầu xu hướng giảm
        ///     - giá cắt xướng tenkan -> bắt đầu xu hướng giảm mạnh
        ///     - giá dưới tenkan và trên kijun: xu hướng giảm nhẹ
        ///         20/05/22: VNM
        ///     - giá dưới tenkan và dưới kijun: xu hướng giảm mạnh
        ///         01/06/22: VNM
        ///         + Tenkan > kijun > giá: giá đang giảm, kijun là kháng cự nhẹ, tenkan sẽ là kháng cự mạnh
        ///         + Tenkan bé hơn kijun bé hơn  giá: giá đang TĂNG, Tenkan là hỗ trợ nhẹ, kijun sẽ là hỗ trợ mạnh
        ///         + Giá bé hơn Mây bot: Mây bot sẽ là kháng cự nhẹ, mây top sẽ là kháng cự mạnh, nếu giá vượt mây top, sẽ có test lại mây, sẽ có thể xuất hiện breakout giả
        ///
        ///         + Tenkan cắt lên kijun: mua     + chỉ thực hiện khi thị trường trong 1 xu hướng tăng/giảm rõ ràng
        ///                 + GEG: 27/05/22, 02/11/22 ĐÚNG
        ///                 + VNM: 06/06/22, 08/04/22, 17/11/21 SAI vì đi ngược xu hướng thị trường
        ///             + Xác xuất cao hơn nếu tại giao cắt
        ///                 + điểm giao cắt nằm trên mây
        ///                 OR Chikou-span nằm trên đường giá (và giá nằm trên mây)
        ///                 (AND thì tỉ lệ cao hơn)
        ///                 
        ///         + Tenkan cắt xuống kijun: bán   + chỉ thực hiện khi thị trường trong 1 xu hướng tăng/giảm rõ ràng
        ///                 + VNM: 20/04/22 ĐÚNG 
        ///             + Xác xuất cao hơn nếu tại giao cắt
        ///                 + điểm giao cắt nằm dưới mây
        ///                 OR Chikou-span nằm dưới đường giá (và giá nằm dưới mây)
        ///                 (AND thì tỉ lệ cao hơn)
        ///                 
        ///         + Xu Hướng Thị trường: 
        ///             +  Có >= 2 cây trước đó là nến giảm (H trước > H sau, L trước > L sau)
        ///             + Hoặc Có >= 60% LÀ NẾN GIẢM TRONG 10 cây trước đó (H trước bé hơn H sau, L trước bé hơn L sau)
        ///             + Xu hướng tăng dc củng cố khi chikou span nằm trên đường giá và giá nắm trên mây Kumo
        ///             + Xu hướng giảm dc củng cố khi chikou span nằm dưới đường giá và giá nằm dưới mây Kumo
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// </summary>        
        public static LocCoPhieuFilterRequest CT3 = new LocCoPhieuFilterRequest("CT3") //Kijun vs Tenkan
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
