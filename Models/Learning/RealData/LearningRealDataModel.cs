using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Learning.RealData
{
    public class LearningRealDataModel
    {

        public LearningRealDataModel(List<History> histories, History humnay, History humwa, History humkia, bool mua, decimal loi, decimal von)
        {
            var chechLechSoVoiPhienTruoc = 1.02M;
            Code = humnay.StockSymbol;
            Date = humnay.Date.ToShortDateString();
            Gia = humnay.C;
            Mua = mua;
            Loi = loi;
            Von = von;
            NenTopSoVoiBandsTop = Math.Round(humwa.NenTop / humwa.BandsTop, 2);
            NenBotSoVoiBandsBot = Math.Round(humwa.NenBot / humwa.BandsBot, 2);
            NenTopSoVoiGiaMA20 = Math.Round(humwa.NenTop / humwa.MA(histories, -20), 2);
            NenTopSoVoiGiaMA5 = Math.Round(humwa.NenTop / humwa.MA(histories, -5), 2);
            NenBotSoVoiGiaMA20 = Math.Round(humwa.NenBot / humwa.MA(histories, -20), 2);
            NenBotSoVoiGiaMA5 = Math.Round(humwa.NenBot / humwa.MA(histories, -5), 2);
            NenTangGia = humwa.TangGia();
            NenBaoPhu = humwa.NenTop - humwa.NenBot > humkia.NenTop - humkia.NenBot &&
                ((humwa.NenTop >= humkia.NenTop && humwa.NenBot <= humkia.NenBot + (humkia.NenTop - humkia.NenBot) * 0.75M)
                || (humwa.NenBot <= humkia.NenBot && humwa.NenTop >= humkia.NenBot + (humkia.NenTop - humkia.NenBot) * 0.75M));

            var past = histories.Where(h => h.Date < humwa.Date).OrderByDescending(h => h.Date).ToList();
            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].BandsTop > past[i + 1].BandsTop * chechLechSoVoiPhienTruoc)
                    BandTopTangTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].BandsTop * chechLechSoVoiPhienTruoc < past[i + 1].BandsTop)
                    BandTopGiamTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i - i].BandsTop.IsDifferenceInRank(past[i + 1].BandsTop, chechLechSoVoiPhienTruoc - 1))
                    BandTopDiNgangTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].BandsBot > past[i + 1].BandsBot * chechLechSoVoiPhienTruoc)
                    BandBotTangTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].BandsBot * chechLechSoVoiPhienTruoc < past[i + 1].BandsBot)
                    BandBotGiamTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i - i].BandsBot.IsDifferenceInRank(past[i + 1].BandsBot, chechLechSoVoiPhienTruoc - 1))
                    BandBotDiNgangTrongNPhien++;
                else
                    break;
            }


            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MA(histories, -5) > past[i + 1].MA(histories, -5) * chechLechSoVoiPhienTruoc)
                    MA5TangTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MA(histories, -5) * chechLechSoVoiPhienTruoc < past[i + 1].MA(histories, -5))
                    MA5GiamTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i - i].MA(histories, -5).IsDifferenceInRank(past[i + 1].MA(histories, -5), chechLechSoVoiPhienTruoc - 1))
                    MA5DiNgangTrongNPhien++;
                else
                    break;
            }


            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MA(histories, -20) > past[i + 1].MA(histories, -20) * chechLechSoVoiPhienTruoc)
                    MA20TangTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MA(histories, -20) * chechLechSoVoiPhienTruoc < past[i + 1].MA(histories, -20))
                    MA20GiamTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i - i].MA(histories, -20).IsDifferenceInRank(past[i + 1].MA(histories, -20), chechLechSoVoiPhienTruoc - 1))
                    MA20DiNgangTrongNPhien++;
                else
                    break;
            }


            RSINay = humwa.RSI;
            RSIWa = humkia.RSI;
            RSITang = Math.Round(humwa.RSI / humkia.RSI, 2);
            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].RSI > past[i + 1].RSI * 1.05M)
                    RSITangTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].RSI * 1.05M < past[i + 1].RSI)
                    RSIGiamTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i - i].RSI.IsDifferenceInRank(past[i + 1].RSI, 0.05M))
                    RSIDiNgangTrongNPhien++;
                else
                    break;
            }


            Macd = humwa.MACD;
            MacdSoVoi0 = humwa.MACD > 0;
            MacdSignal = humwa.MACDSignal;
            MacdSignalSoVoi0 = humwa.MACDSignal > 0;
            MacdMomentum = humwa.MACDMomentum;
            MacdMomentumSoVoi0 = humwa.MACDMomentum > 0;

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MACD > past[i + 1].MACD * chechLechSoVoiPhienTruoc)
                    MACDTangTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MACD * chechLechSoVoiPhienTruoc < past[i + 1].MACD)
                    MACDGiamTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i - i].MACD.IsDifferenceInRank(past[i + 1].MACD, chechLechSoVoiPhienTruoc - 1))
                    MACDDiNgangTrongNPhien++;
                else
                    break;
            }


            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MACDSignal > past[i + 1].MACDSignal * chechLechSoVoiPhienTruoc)
                    MACDSignalTangTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MACDSignal * chechLechSoVoiPhienTruoc < past[i + 1].MACDSignal)
                    MACDSignalGiamTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i - i].MACDSignal.IsDifferenceInRank(past[i + 1].MACDSignal, chechLechSoVoiPhienTruoc - 1))
                    MACDSignalDiNgangTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MACDMomentum > past[i + 1].MACDMomentum * chechLechSoVoiPhienTruoc)
                    MACDMomentumTangTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MACDMomentum * chechLechSoVoiPhienTruoc < past[i + 1].MACDMomentum)
                    MACDMomentumGiamTrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i - i].MACDMomentum.IsDifferenceInRank(past[i + 1].MACDMomentum, chechLechSoVoiPhienTruoc - 1))
                    MACDMomentumDiNgangTrongNPhien++;
                else
                    break;
            }

            VolSoVoiVolMA20 = humwa.VOL(histories, -20) <= 0 ? 0 : Math.Round(humwa.V / humwa.VOL(histories, -20), 2);

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].V > past[i + 1].VOL(histories, -20))
                    VolLonHonMA20TrongNPhien++;
                else
                    break;
            }

            for (int i = 0; i < past.Count(); i++)
            {
                if (past[i].V < past[i + 1].VOL(histories, -20))
                    VolNhoHonMA20TrongNPhien++;
                else
                    break;
            }

            IchiGiaDuoiKijun = humwa.C < humwa.IchimokuKijun;
            IchiGiaDuoiSpanA = humwa.C < humwa.IchimokuCloudTop;
            IchiGiaDuoiSpanB = humwa.C < humwa.IchimokuCloudBot;
            IchiGiaDuoiTenkan = humwa.C < humwa.IchimokuTenKan;
            IchiGiaTrenKijun = humwa.C > humwa.IchimokuKijun;
            IchiGiaTrenSpanA = humwa.C > humwa.IchimokuCloudTop;
            IchiGiaTrenSpanB = humwa.C > humwa.IchimokuCloudBot;
            IchiGiaTrenTenkan = humwa.C > humwa.IchimokuTenKan;

            NgayMuaKichBanVolSoVoiVolMA20PhienTruoc = humwa.VOL(histories, -20) <= 0 ? 0 : Math.Round(humnay.V / humwa.VOL(histories, -20), 2);
            NgayMuaKichBanVolSoVoiVolPhienTruoc = humwa.V <= 0 ? 0 : Math.Round(humnay.V / humwa.V, 2);
            NgayMuaKichBanGiaMoCuaSoVoiPhienTruoc = Math.Round(humnay.O / humwa.C, 2);
            NgayMuaKichBanGiaMoCuaSoVoiMA20PhienTruoc = Math.Round(humnay.O / humwa.MA(histories, -20), 2);
        }

        public string Code { get; set; }
        /// <summary>
        /// ToShortDateString
        /// </summary>
        public string Date { get; set; }
        public bool Mua { get; set; }
        public decimal Gia { get; set; }
        public decimal Loi { get; set; }
        public decimal Von { get; set; }
        //public string GiaMA5 { get; set; }
        //public string GiaMA20 { get; set; }
        public decimal NenTopSoVoiBandsTop { get; set; }
        public decimal NenBotSoVoiBandsBot { get; set; }
        public decimal NenTopSoVoiGiaMA20 { get; set; }
        public decimal NenBotSoVoiGiaMA20 { get; set; }
        public decimal NenTopSoVoiGiaMA5 { get; set; }
        public decimal NenBotSoVoiGiaMA5 { get; set; }
        public bool NenTangGia { get; set; }
        public bool NenBaoPhu { get; set; }
        public int BandTopTangTrongNPhien { get; set; }
        public int BandTopGiamTrongNPhien { get; set; }
        public int BandTopDiNgangTrongNPhien { get; set; }
        public int BandBotTangTrongNPhien { get; set; }
        public int BandBotGiamTrongNPhien { get; set; }
        public int BandBotDiNgangTrongNPhien { get; set; }
        public int MA5TangTrongNPhien { get; set; }
        public int MA5GiamTrongNPhien { get; set; }
        public int MA5DiNgangTrongNPhien { get; set; }
        public int MA20TangTrongNPhien { get; set; }
        public int MA20GiamTrongNPhien { get; set; }
        public int MA20DiNgangTrongNPhien { get; set; }
        public decimal RSIWa { get; set; }
        public decimal RSINay { get; set; }
        /// <summary>
        /// % - tăng hoặc giảm so với phiên trước
        /// </summary>        
        public decimal RSITang { get; set; }
        public int RSITangTrongNPhien { get; set; }
        public int RSIGiamTrongNPhien { get; set; }
        public int RSIDiNgangTrongNPhien { get; set; }
        /// <summary>
        /// % - tăng hoặc giảm so với phiên trước
        /// </summary>
        public decimal Macd { get; set; }
        public bool MacdSoVoi0 { get; set; }
        public decimal MacdSignal { get; set; }
        public bool MacdSignalSoVoi0 { get; set; }
        public decimal MacdMomentum { get; set; }
        public bool MacdMomentumSoVoi0 { get; set; }
        public int MACDTangTrongNPhien { get; set; }
        public int MACDGiamTrongNPhien { get; set; }
        public int MACDDiNgangTrongNPhien { get; set; }
        public int MACDSignalTangTrongNPhien { get; set; }
        public int MACDSignalGiamTrongNPhien { get; set; }
        public int MACDSignalDiNgangTrongNPhien { get; set; }
        public int MACDMomentumTangTrongNPhien { get; set; }
        public int MACDMomentumGiamTrongNPhien { get; set; }
        public int MACDMomentumDiNgangTrongNPhien { get; set; }
        public decimal VolSoVoiVolMA20 { get; set; }
        //public string VolMA20 { get; set; }
        public int VolLonHonMA20TrongNPhien { get; set; }
        public int VolNhoHonMA20TrongNPhien { get; set; }
        public bool IchiGiaTrenTenkan { get; set; }
        public bool IchiGiaDuoiTenkan { get; set; }
        public bool IchiGiaTrenKijun { get; set; }
        public bool IchiGiaDuoiKijun { get; set; }
        public bool IchiGiaTrenSpanA { get; set; }
        public bool IchiGiaDuoiSpanA { get; set; }
        public bool IchiGiaTrenSpanB { get; set; }
        public bool IchiGiaDuoiSpanB { get; set; }



        public decimal NgayMuaKichBanVolSoVoiVolMA20PhienTruoc { get; set; }
        public decimal NgayMuaKichBanVolSoVoiVolPhienTruoc { get; set; }
        public decimal NgayMuaKichBanGiaMoCuaSoVoiPhienTruoc { get; set; }
        public decimal NgayMuaKichBanGiaMoCuaSoVoiMA20PhienTruoc { get; set; }
        //public string NgayMuaKichBanGiaSoVoiBandTop { get; set; }
        //public string NgayMuaKichBanGiaSoVoiBandBot { get; set; }
        //public string NgayMuaKichBanGiaSoVoiGiaMA20 { get; set; }
        //public string NgayMuaKichBanGiaSoVoiGiaMA5 { get; set; }
        //public string NgayMuaKichBanVolSoVoiVolMA20 { get; set; }
        //public string NgayMuaKichBanVolSoVoiVolMA20 { get; set; }
        //public string NgayMuaKichBanVolSoVoiVolMA20 { get; set; }

    }
}
