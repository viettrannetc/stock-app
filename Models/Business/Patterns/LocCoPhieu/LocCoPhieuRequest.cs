using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu
{
    public enum SoSanhEnum
    {
        Unknow = 0,
        Bang,
        NhoHon,
        LonHon,
        NhoHonHoacBang,
        LonHonHoacBang
    }

    public enum OperationEnum
    {
        Add,
        Minus,
        Multiply,
        Divide,
        CrossUp,
        CrossDown,
        ThayDoiTangNPhien,
        ThayDoiGiamNPhien,
        ThayDoiNgangNPhien,
        SoSanh,
        TrongVong,
        /// <summary>
        /// Toán từ này được dùng trong việc xác định giá mua
        /// Tương Quan giữa 2 property có tỉ lệ là bao nhiêu
        /// VD: property 1 tương quan property 2
        /// Nếu property 1: điểm lời để chốt
        /// Nếu là số < 1: điểm lỗ để cắt
        /// </summary>
        TuongQuan
    }

    public class LocCoPhieuFilter
    {
        public SoSanhEnum Ope { get; set; }
        public decimal Value { get; set; }
    }

    public class LocCoPhieuCompareModel
    {
        /// <summary>
        /// Default = null - không sử dụng
        /// = 0 ngày hiện tại
        /// Số Âm là ngày quá khứ
        /// </summary>
        public int? Phien { get; set; }
        public bool Day2 { get; set; }

        public string Property1 { get; set; }
        public string Property2 { get; set; }
        public OperationEnum Operation { get; set; }
        public SoSanhEnum Sign { get; set; }
        public decimal Result { get; set; }
    }

    public class LocCoPhieuQuaKhuRequest
    {
        public DateTime? TuNgay { get; set; }
        public DateTime? ToiNgay { get; set; }
        public LocCoPhieuFilterRequest Filter { get; set; }
    }

    public class LocCoPhieuFilterRequest
    {
        public LocCoPhieuFilterRequest(string name)
        {
            Name = name;
            PropertiesSoSanh = new List<LocCoPhieuCompareModel>();
            PriceMongDoi = new List<LocCoPhieuCompareModel>();
        }

        public LocCoPhieuFilterRequest(string name, List<LocCoPhieuCompareModel> keThua)
        {
            Name = name;
            PropertiesSoSanh = new List<LocCoPhieuCompareModel>();
            PropertiesSoSanhKeThua = new List<LocCoPhieuCompareModel>();
            PropertiesSoSanhKeThua.AddRange(keThua);
            PriceMongDoi = new List<LocCoPhieuCompareModel>();
        }

        private bool DaKeThua { get; set; }

        public string Name { get; set; }

        public bool? NenBaoPhuDaoChieuTrungBinh { get; set; }
        /// <summary>
        /// False: đảo chieu giảm
        /// True: đảo chiều tăng
        /// </summary>
        public bool? NenBaoPhuDaoChieuManh { get; set; }
        /// <summary>
        /// Toán từ này được dùng trong việc xác định giá bán
        /// Toán tử này kì vọng giá hum nay sẽ đem đi so sánh với giá quá khứ ở 1 mức (>=) bao nhiêu đó %
        /// Nếu là số > 1: điểm lời để chốt
        /// Nếu là số < 1: điểm lỗ để cắt
        /// </summary>
        public LocCoPhieuCompareModel KiVong { get; set; }
        public LocCoPhieuFilter ChieuDaiThanNenSoVoiRau { get; set; }
        public LocCoPhieuFilter RSIHumWa { get; set; }
        /// <summary>
        /// TODO
        /// </summary>
        public bool? RSIPhanKyGiam { get; set; }
        /// <summary>
        /// TODO
        /// </summary>
        public bool? RSIPhanKyTang { get; set; }
        public int? PhanKyXayRaTrongNPhien { get; set; }
        public bool? TrongThoiGianPhanKyDeuLaNenTang { get; set; }
        public bool? TrongThoiGianPhanKyDeuLaNenGiam { get; set; }
        public bool? MACDPhanKiGiam { get; set; }
        public bool? MACDPhanKiTang { get; set; }
        public LocCoPhieuFilter VolSoVoiVolMA20 { get; set; }
        public int? VolLonHonMA20LienTucTrongNPhien { get; set; }
        public int? VolNhoHonMA20LienTucTrongNPhien { get; set; }
        public LocCoPhieuFilter GiaSoVoiDinhTrongVong40Ngay { get; set; }
        public int? CachDayThapNhatCua40NgayTrongVongXNgay { get; set; }
        public List<LocCoPhieuCompareModel> PropertiesSoSanh { get; set; }
        public List<LocCoPhieuCompareModel> Properties
        {
            get
            {
                var a = new List<LocCoPhieuCompareModel>();
                a.AddRange(PropertiesSoSanh);
                if (PropertiesSoSanhKeThua != null && PropertiesSoSanhKeThua.Any())
                    a.AddRange(PropertiesSoSanhKeThua);
                return a;
            }
        }
        private List<LocCoPhieuCompareModel> PropertiesSoSanhKeThua { get; set; }

        /// <summary>
        /// CT Bắt đáy khi giảm mạnh
        /// <para> + Tính RSI hiện tại, đếm ngược lại những ngày trước đó mà RSI vẫn đang giảm và các nến đều là nến đỏ</para>
        /// <para> + Đi ngược lại tìm nến cao nhất</para>
        /// <para> + Tính từ giá đóng của của cây nến đỏ cao nhất, so với giá hiện tại, nếu hiện tại giá đã giảm > 20%</para>
        /// <para> -> thì cây sau mua bắt giá thấp nhất, giá cao nhất để mua cũng chỉ <= giá đóng của hum nay + 1/5 thân nên hum nay, tuyệt đối ko mua nếu giá mở cửa có tạo GAP cao hơn giá mở cửa của phiên hum nay</para>
        /// </summary>
        public bool? BatDay1 { get; set; }


        public List<LocCoPhieuCompareModel> PriceMongDoi { get; set; }

        /// <summary>
        /// Giá của ngày hiện tại so với giá đỉnh của ngày quá khứ trong vòng 60 ngày phải có sự tương quan
        /// Ví dụ:
        ///         Giá ngày hiện tại so với đỉnh quá khứ   > : 
        ///  *                                              = : 
        ///  *                                        nho hon : MUA: nếu Giá hiện tại <= Giá quá khứ* (1 - ((RSI quá khứ / RSI hiện tại) / 100)        - TODO: chưa làm
        ///  *                                                  MUA: nếu Giá hiện tại <= Giá quá khứ* (1 - ((MACD quá khứ / MACD hiện tại) / 100)      - làm trong CT này
        ///  *                                              
        ///  */
        /// </summary>
        public bool? KiemTraGiaVsMACD { get; set; }


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
        public bool? KiemTraTangManhTuDay { get; set; }
    }

    public class LocCoPhieuKiVongRequest
    {
        public decimal NgayMuaKichBanVolSoVoiVolMA20PhienTruoc { get; set; }
        public decimal NgayMuaKichBanVolSoVoiVolPhienTruoc { get; set; }
        public decimal NgayMuaKichBanGiaMoCuaSoVoiPhienTruoc { get; set; }
        public decimal NgayMuaKichBanGiaMoCuaSoVoiMA20PhienTruoc { get; set; }
        public decimal LãiMin { get; set; }
    }

    public class LocCoPhieuRequest
    {
        public LocCoPhieuRequest(string code, DateTime ngay)
        {
            Code = code;
            Ngay = ngay;
            Suggestion = new LocCoPhieuKiVongRequest
            {
                LãiMin = 1.01M
            };
            Filters = new List<LocCoPhieuFilterRequest>();
            VolToiThieu = new LocCoPhieuFilter { Ope = SoSanhEnum.LonHonHoacBang, Value = 1000000 };
            GiaToiThieu = new LocCoPhieuFilter { Ope = SoSanhEnum.LonHonHoacBang, Value = 6000 };
            ShowHistory = true;
        }
        public string Code { get; set; }
        public DateTime Ngay { get; set; }
        public LocCoPhieuFilter VolToiThieu { get; set; }
        public LocCoPhieuFilter GiaToiThieu { get; set; }
        public List<LocCoPhieuFilterRequest> Filters { get; set; }
        public LocCoPhieuKiVongRequest Suggestion { get; set; }
        public bool ShowHistory { get; set; }
    }
}
