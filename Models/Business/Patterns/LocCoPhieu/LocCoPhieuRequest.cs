using System;
using System.Collections.Generic;

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
        TrongVong
        /// <summary>
        /// Toán từ này được dùng trong việc xác định giá bán
        /// Toán tử này kì vọng giá hum nay sẽ đem đi so sánh với giá quá khứ ở 1 mức (>=) bao nhiêu đó %
        /// Nếu là số > 1: điểm lời để chốt
        /// Nếu là số < 1: điểm lỗ để cắt
        /// </summary>
        //KiVong
    }

    public class LocCoPhieuFilter
    {
        public SoSanhEnum Ope { get; set; }
        public decimal Value { get; set; }
    }

    public class LocCoPhieuCompareModel
    {
        /// <summary>
        /// Default = 0; 
        /// Số Âm là ngày quá khứ
        /// </summary>
        public int Ngay { get; set; }
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

        /// <summary>
        /// CT Bắt đáy khi giảm mạnh
        /// <para> + Tính RSI hiện tại, đếm ngược lại những ngày trước đó mà RSI vẫn đang giảm và các nến đều là nến đỏ</para>
        /// <para> + Đi ngược lại tìm nến cao nhất</para>
        /// <para> + Tính từ giá đóng của của cây nến đỏ cao nhất, so với giá hiện tại, nếu hiện tại giá đã giảm > 20%</para>
        /// <para> -> thì cây sau mua bắt giá thấp nhất, giá cao nhất để mua cũng chỉ <= giá đóng của hum nay + 1/5 thân nên hum nay, tuyệt đối ko mua nếu giá mở cửa có tạo GAP cao hơn giá mở cửa của phiên hum nay</para>
        /// </summary>
        public bool? BatDay1 { get; set; }


        public List<LocCoPhieuCompareModel> PriceMongDoi { get; set; }
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
