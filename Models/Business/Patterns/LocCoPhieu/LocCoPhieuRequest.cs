using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu
{
    public enum LocCoPhieuFilterEnum
    {
        Unknow = 0,
        Bang,
        NhoHon,
        LonHon,
        NhoHonHoacBang,
        LonHonHoacBang
    }

    public class LocCoPhieuFilter
    {
        public LocCoPhieuFilterEnum Ope { get; set; }
        public decimal Value { get; set; }
    }


    public class LocCoPhieuQuaKhuRequest
    {
        public DateTime? TuNgay { get; set; }
        public DateTime? ToiNgay { get; set; }
        public LocCoPhieuFilterRequest Filter { get; set; }
    }

    //public class LocCoPhieuHumNayRequest
    //{
    //    public DateTime? Ngay { get; set; }
    //    public LocCoPhieuFilterRequest Filter { get; set; }
    //}

    public class LocCoPhieuFilterRequest
    {
        public LocCoPhieuFilter DoDaiThanNenToiBandsTop { get; set; }
        public LocCoPhieuFilter DoDaiThanNenToiBandsBot { get; set; }
        public LocCoPhieuFilter NenTopSoVoiBandsTop { get; set; }
        public LocCoPhieuFilter NenBotSoVoiBandsBot { get; set; }
        public LocCoPhieuFilter NenTopSoVoiGiaMA20 { get; set; }
        public LocCoPhieuFilter NenBotSoVoiGiaMA20 { get; set; }
        public LocCoPhieuFilter NenTopSoVoiGiaMA5 { get; set; }
        public LocCoPhieuFilter NenBotSoVoiGiaMA5 { get; set; }
        public bool? NenTangGia { get; set; }
        public bool? NenBaoPhu { get; set; }
        public LocCoPhieuFilter ChieuDaiThanNenSoVoiRau { get; set; }
        public bool? ĐuôiNenThapHonBandDuoi { get; set; }
        public int? BandTopTangLienTucTrongNPhien { get; set; }
        public int? BandTopGiamLienTucTrongNPhien { get; set; }
        public int? BandTopDiNgangLienTucTrongNPhien { get; set; }
        public int? BandBotTangLienTucTrongNPhien { get; set; }
        public int? BandBotGiamLienTucTrongNPhien { get; set; }
        public int? BandBotDiNgangLienTucTrongNPhien { get; set; }
        public int? MA5TangLienTucTrongNPhien { get; set; }
        public int? MA5GiamLienTucTrongNPhien { get; set; }
        public int? MA5DiNgangLienTucTrongNPhien { get; set; }
        public bool? MA5CatLenMA20 { get; set; }
        public bool? MA5CatXuongMA20 { get; set; }
        public LocCoPhieuFilter MA5SoVoiMA20 { get; set; }
        public LocCoPhieuFilter MA20TiLeVoiM5 { get; set; }
        public int? MA20TangLienTucTrongNPhien { get; set; }
        public int? MA20GiamLienTucTrongNPhien { get; set; }
        public int? MA20DiNgangLienTucTrongNPhien { get; set; }
        public LocCoPhieuFilter RSIHumWa { get; set; }
        public LocCoPhieuFilter RSI { get; set; }
        /// <summary>
        /// % - tăng hoặc giảm so với phiên trước
        /// </summary>        
        //public LocCoPhieuFilter RSINaySoVoiWa { get; set; }
        public int? RSITangLienTucTrongNPhien { get; set; }
        public int? RSIGiamLienTucTrongNPhien { get; set; }
        public int? RSIDiNgangLienTucTrongNPhien { get; set; }
        //TODO
        public bool? RSIPhanKyAm { get; set; }

        public LocCoPhieuFilter Macd { get; set; }
        public LocCoPhieuFilter MacdSoVoiSignal { get; set; }
        public LocCoPhieuFilter MacdSignal { get; set; }
        public LocCoPhieuFilter MacdMomentum { get; set; }
        public int? MACDTangLienTucTrongNPhien { get; set; }
        public int? MACDGiamLienTucTrongNPhien { get; set; }
        public int? MACDDiNgangLienTucTrongNPhien { get; set; }
        public int? MACDSignalTangLienTucTrongNPhien { get; set; }
        public int? MACDSignalGiamLienTucTrongNPhien { get; set; }
        public int? MACDSignalDiNgangLienTucTrongNPhien { get; set; }
        public int? MACDMomentumTangLienTucTrongNPhien { get; set; }
        public int? MACDMomentumGiamLienTucTrongNPhien { get; set; }
        public int? MACDMomentumTangDanSoVoiNPhien { get; set; }
        public int? MACDMomentumDiNgangLienTucTrongNPhien { get; set; }
        public LocCoPhieuFilter VolSoVoiVolMA20 { get; set; }
        public int? VolLonHonMA20LienTucTrongNPhien { get; set; }
        public int? VolNhoHonMA20LienTucTrongNPhien { get; set; }
        public LocCoPhieuFilter IchiGiaSoVoiTenkan { get; set; }
        public LocCoPhieuFilter IchiGiaSoVoiKijun { get; set; }
        public LocCoPhieuFilter IchiGiaSoVoiSpanA { get; set; }
        public LocCoPhieuFilter IchiGiaSoVoiSpanB { get; set; }
        public LocCoPhieuFilter GiaSoVoiDinhTrongVong40Ngay { get; set; }

        public int? CachDayThapNhatCua40NgayTrongVongXNgay { get; set; }
    }

    public class LocCoPhieuKiVongRequest
    {
        public decimal NgayMuaKichBanVolSoVoiVolMA20PhienTruoc { get; set; }
        public decimal NgayMuaKichBanVolSoVoiVolPhienTruoc { get; set; }
        public decimal NgayMuaKichBanGiaMoCuaSoVoiPhienTruoc { get; set; }
        public decimal NgayMuaKichBanGiaMoCuaSoVoiMA20PhienTruoc { get; set; }

        //public bool Mua { get; set; }
        //public decimal Gia { get; set; }
        //public decimal Loi { get; set; }
        //public decimal Von { get; set; }


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
            VolToiThieu = new LocCoPhieuFilter { Ope = LocCoPhieuFilterEnum.LonHonHoacBang, Value = 100000 };
        }
        public string Code { get; set; }
        public DateTime Ngay { get; set; }
        public LocCoPhieuFilter VolToiThieu { get; set; }
        public List<LocCoPhieuFilterRequest> Filters { get; set; }
        public LocCoPhieuKiVongRequest Suggestion { get; set; }
    }
}
