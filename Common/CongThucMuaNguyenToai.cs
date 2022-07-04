using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Common
{
    public static partial class CongThuc
    {

        /// <summary>
        /// Nguyễn Toại
        /// - 2 đáy Giá (sau 1 chu kì giảm, nó có tăng lại 1 chút, nếu sau đó nó vòng lại test đáy thì mua vô
        /// - 2 đáy MACD 
        /// - MACD phan ki duong
        /// - RSI phan ki duong
        /// - Giá tăng trên MA 20, sau đó vòng lại tích lũy trên MA 20
        /// - Xem mây để đón kháng cự/hỗ trợ 
        /// - Xem Fibonance để đón kháng cự 
        /// - Xem mẫu hình cờ, mẫu hình tam giác, sóng eliot, mẫu hình nến đảo chiều để cbi kịch bản
        /// - Nếu mã nào thủng đáy thì sẽ có nhịp quay lại test kháng cự cũ -> canh nhịp hồi thoát hàng nếu có
        /// - Nếu mã nào thủng đáy cũ, thì sau khi tạo đáy mới, sẽ có 1 nhịp hồi, sau nhịp hồi này sẽ rơi về test lại đáy mới(nếu ko có tin xầu thì giá chỉ rơi gần về thôi), và sẽ bật lên -> canh thoát hàng nếu có hoặc canh mua đáy 
        /// - Giá gặp cản cũ, mà MACD tăng cao hơn lên thì cứ múc, cản sẽ bị phá (05/21)
        /// - mẫu hình phá vỡ tăng
        ///     + nến hum nay được theo sau bởi 1 nến tăng ngày hum wa, bất chấp giá O tạo gap, giá O bật lên khỏi
        /// - Vượt đỉnh cũ, nổ vol lớn, thì đu giá, call margin các kiểu cũng dc (SSI - 20/05/21, NLG thì ko như thế => ko mua đuổi)
        /// Chart H: chỉ dùng trong sóng hồi, trong up trend dùng dễ bị mất hàng. chỉ xem chart giờ khi chart ngày có tín hiệu (ví dụ: 2 đỉnh MACD - DIG - 9/1/21)
        ///     - tạo đáy MACD, phân kì
        /// Cuối tuần:
        ///     - Xem chart Tuần
        /// Cuối tháng:
        ///     - Xem chart Tháng
        /// MACD đi ngang mà giá đi xuống, thì phải để ý rất kĩ, khi nào nổ vol tăng giá thì chơi (LDG - 15/05/19 - 28/08/19)
        /// MACD tăng qua phân kì âm (giảm giá) thì sẽ tăng mạnh, độ dốc của MACD càng cao thì càng mạnh
        /// VNINDEX 09/20 - 11/20 - Phan ki am -> tang mạnh 15/11/20 vì MACd vượt đỉnh MACD Cũ cuối tháng 11/20
        /// VNINDEX 01/21 - 04/10 - Phan ki am -> tang mạnh 25/06/21 vì MACd vượt đỉnh MACD Cũ giữa tháng 04/21
        /// 
        /// Ngoại lệ: 01/2020 - Covid
        /// 
        /// Đơi macd có  3 đáy pk dương 
        /// MA20 đi ngang, giá tích luỹ trên ma20 thành công
        /// Mây mỏng và có eo mây thì mới full dc
        /// </summary>

        public static LocCoPhieuFilterRequest CTNT1 = new LocCoPhieuFilterRequest("CTNT1")
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                //new LocCoPhieuCompareModel { Phien = -1, Property1 = "NenBot", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                //new LocCoPhieuCompareModel { Phien = -1, Property1 = "MACD", Operation = OperationEnum.SoSanh, Sign = SoSanhEnum.NhoHon, Result = 0 },
                new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 2  },
                new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.TrongVong, Result = 10  },
                new LocCoPhieuCompareModel { Property1 = "BandsMid", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 1 },
                new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHonHoacBang, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "MACD", Property2 = "MACDSignal", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.SoSanh, Sign = SoSanhEnum.LonHon, Result = 0  }

                //new LocCoPhieuCompareModel { Property1 = "C", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.NhoHon, Result = 4  },
                //new LocCoPhieuCompareModel { Property1 = "MACD", Property2 = "O", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                //new LocCoPhieuCompareModel { Property1 = "GiaMA05", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 1  },
                //new LocCoPhieuCompareModel { Property1 = "NenTop", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHonHoacBang, Result = 0  },
                //new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                //new LocCoPhieuCompareModel { Property1 = "GiaMA05", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  },
                //new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 2  },
                //new LocCoPhieuCompareModel { Property1 = "C", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.NhoHon, Result = 4  },
            },
        };

        /// <summary>
        /// MACD cross up signal
        /// </summary>
        public static LocCoPhieuFilterRequest CTNT2 = new LocCoPhieuFilterRequest("CTNT2")
        {
            //MACDPhanKiTang = true,
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "C", Property2 = "O", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Day2 = true, Property1 = "MACD", Operation = OperationEnum.TrongVong, Result = 2  },
                new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 1 },
                new LocCoPhieuCompareModel { Property1 = "NenTop", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.NhoHon, Result = 0  }
            },
        };

        /// <summary>
        /// Trong vong 7 phien, co 2 đáy MACD 
        /// TODO: chưa xai dc 
        /// </summary>
        public static LocCoPhieuFilterRequest CTNT3 = new LocCoPhieuFilterRequest("CTNT3")
        {
            FullMargin = true
        };
    }
}
