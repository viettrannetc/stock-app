using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Common
{
    public static partial class CongThuc
    {
        /*
        /// <summary>
        /// Nguyễn Toại
        /// - 2 đáy Giá (sau 1 chu kì giảm, nó có tăng lại 1 chút, nếu sau đó nó vòng lại test đáy thì mua vô)
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
        /// - Khi bắt đáy, chọn cổ giảm mạnh hơn VNINDEX trước, những mã này sẽ bật lên trước VN-INDEX
        /// 
        /// - Những mã xây nền tốt: giá đóng trên MA 20, đi ngang vài phiên, kijun đi ngang hoặc hướng lên, MACD sắp lên 0, MACD hướng lên
        /// 
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
        /// - 1 nhận xét từ 1 người trong room
        /// Đơi macd có  3 đáy pk dương 
        /// MA20 đi ngang, giá tích luỹ trên ma20 thành công
        /// Mây mỏng và có eo mây thì mới full dc
        /// </summary>
        */

        /// <summary>
        /// Có đáy 2 MACD trong 11 ngày trước
        /// Từ đáy 2 MACD tới ngày hiện tại, MACD liên tục tăng
        /// Từ những ngày sau đáy 2 tới hiện tại, tìm ra cây vượt MA 20
        /// Giá vượt MA 20 thì bắt đầu canh mua ở giá MA 20 hoặc chờ giá đang giảm dần về test lại MA 20
        /// </summary>
        public static LocCoPhieuFilterRequest CTNT1 = new LocCoPhieuFilterRequest("CTNT1")      //Confirmed
        {
            //PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
            //    //new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.ThayDoiTangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 2  },
            //    //new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.Day2XuatHienTrongVongNPhien, Result = 10  },
            //    ////new LocCoPhieuCompareModel { Property1 = "BandsMid", Operation = OperationEnum.DangBeNgang },
            //    //new LocCoPhieuCompareModel { Property1 = "MACD", Property2 = "MACDSignal", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
            //    //new LocCoPhieuCompareModel { Property1 = "NenBot", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHonHoacBang, Result = 0  },//đang ở trên MA 20

            //    //new LocCoPhieuCompareModel { Phien = -1, Property1 = "NenBot", Property2 = "BandsMid", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHonHoacBang, Result = 0  },//đang ở trên MA 20
            //    //new LocCoPhieuCompareModel { Phien = -1, Property1 = "C", Property2 = "O", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHonHoacBang, Result = 0  },//đang ở trên MA 20


            //    new LocCoPhieuCompareModel { Property1 = "MACD", Operation = OperationEnum.TaoDay2, Result = 10  }, //Có xuất hiện đáy 2 MACD trong vòng 10 phiên trước
            //},
            Confirmed = true,
            Note = "CTNT1 - Có đáy 2 MACD trong 11 ngày trước, Từ đáy 2 MACD tới ngày hiện tại, MACD liên tục tăng, Từ những ngày sau đáy 2 tới hiện tại, tìm ra cây vượt MA 20, Giá vượt MA 20 thì bắt đầu canh mua ở giá MA 20 hoặc chờ giá đang giảm dần về test lại MA 20. Tránh mây flat, trong mây, MACD cắt xuống Signal trong 3 phiên trước, giá mua quanh giữa thân nến LÊN MA 20+ 2%, để ý giá hum sau chart h, nếu giá rớt liên tục thì bỏ ko mua",
            CTNT1 = true,
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "C", Operation = OperationEnum.SoSanh, Sign = SoSanhEnum.NhoHonHoacBang, Result = 40000  },
            },
        };

        /// <summary>
        /// Giá tích lũy trong > 2 phien
        ///  - Nến Top của 2 phiên trước chênh 2% so với nến top hiện tại       (CTNT2A)
        ///  - OR Nến Bot của 2 phiên trước chênh 2% so với nến Bot hiện tại    (CTNT2B)
        ///          /// Có 2 đáy MACD, MACD đang hướng lên 0,
        /// Giá vượt MA 20 rồi
        /// MA đang bẻ ngang
        /// </summary>
        public static LocCoPhieuFilterRequest CTNT2 = new LocCoPhieuFilterRequest("CTNT2")
        {
            Confirmed = true,
            Note = "CTNT2 - Giá vượt MA 20 thì bắt đầu canh mua ở giá MA 20 hoặc chờ giá đang giảm dần về test lại MA 20, Từ giao điểm giá cắt lên MA 20, tính tới hiện tại, nếu giá trên MA 20 và đường trung bình cộng của giá (theo  (H - L) /2) tạo thành hình cong xuống hoặc đi ngang, và giá hiện tại chưa chạm MA 20 hoặc điệm trung tâm nến vẫn > MA 20, Trong 3 ngày trước đó ko có MACD cắt xuống Signal => thì báo tín hiệu MACD trên Signal, MACD trong 2 phiên trước không cắt xuống Singal. Tránh mây flat, trong mây, MACD cắt xuống Signal trong 3 phiên trước, giá mua quanh giữa thân nến LÊN MA 20+ 2%, để ý giá hum sau chart h, nếu giá rớt liên tục thì bỏ ko mua, nếu nến ko vượt dc nửa thân wa ma 20 thì mua giá ở nửa thân nến thôi",
            CTNT2 = true,
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "MACD", Property2 = "MACDSignal", Operation = OperationEnum.Minus, Sign = SoSanhEnum.LonHon, Result = 0  },
                new LocCoPhieuCompareModel { Property1 = "MACD", Property2 = "MACDSignal", Operation = OperationEnum.CrossDown, Result = -1 },

                new LocCoPhieuCompareModel { Phien = -1, Property1 = "MACD", Property2 = "MACDSignal", Operation = OperationEnum.CrossDown, Result = -1 },
                new LocCoPhieuCompareModel { Phien = -2, Property1 = "MACD", Property2 = "MACDSignal", Operation = OperationEnum.CrossDown, Result = -1 },
                //new LocCoPhieuCompareModel { Property1 = "C", Operation = OperationEnum.SoSanh, Sign = SoSanhEnum.NhoHonHoacBang, Result = 40000  }
            },
        };

        /// <summary>
        /// Giá tích lũy trong > 2 phien
        ///  - Nến Top của 2 phiên trước chênh 2% so với nến top hiện tại       (CTNT2A)
        ///  - OR Nến Bot của 2 phiên trước chênh 2% so với nến Bot hiện tại    (CTNT2B)
        /// </summary>
        public static LocCoPhieuFilterRequest CTNT2B = new LocCoPhieuFilterRequest("CTNT2B")    //Not Good
        {
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "NenBot", Operation = OperationEnum.ThayDoiNgangNPhien, Sign = SoSanhEnum.LonHonHoacBang, Result = 2  }
            },
        };

        /// <summary>
        /// Co Xuat Hien Phien Bung No Trong 5 Phien Truoc -> canh mua giá ở giữa thân cây nến bùng nổ trở lên
        /// 1 - C > O 4%
        /// 2 - V > V MA 20 * 1.5
        /// 3 - Trong 5 phiên trước, không có giá nào cao hơn nửa thân nến hum nay
        /// </summary>
        public static LocCoPhieuFilterRequest CTNT4 = new LocCoPhieuFilterRequest("CTNT4")      //Confirmed
        {
            Confirmed = true,
            Note = "CTNT4 - Có Xuất hiện phiên bùng nổ trong 5 phiên trước, để ý xem nó có phải cây break ko. Để ý né mây flat, trong mây, MACD cắt xuống Signal trong 3 phiên trước, giá mua quanh giữa thân nến của phiên bùng nổ, nếu hôm sau vol thấp và trần thì đợi chỉnh.",
            PhienBungNo = true,
            PropertiesSoSanh = new List<LocCoPhieuCompareModel> {
                new LocCoPhieuCompareModel { Property1 = "C", Operation = OperationEnum.SoSanh, Sign = SoSanhEnum.NhoHonHoacBang, Result = 40000  },
            },
        };
    }
}
