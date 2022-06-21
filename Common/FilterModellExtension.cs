using DotNetCoreSqlDb.Common.ArrayExtensions;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business;
using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetCoreSqlDb.Common
{
    public static class FilterModellExtension
    {
        //public static bool NenTopVsGiaMa20(this List<History> histories, History checkingDate, LocCoPhieuFilterEnum filter)
        //{
        //    switch (filter)
        //    {
        //        case LocCoPhieuFilterEnum.Bang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenTop == checkingDate.MA(histories, -20)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.NhoHon:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenTop < checkingDate.MA(histories, -20)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.LonHon:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenTop > checkingDate.MA(histories, -20)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.NhoHonHoacBang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenTop <= checkingDate.MA(histories, -20)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.LonHonHoacBang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenTop >= checkingDate.MA(histories, -20)).ToList();
        //            break;
        //        default:
        //            break;
        //    }

        //    return histories;
        //}

        //public static bool NenBotVsGiaMa20(this List<History> histories, History checkingDate, LocCoPhieuFilterEnum filter)
        //{
        //    switch (filter)
        //    {
        //        case LocCoPhieuFilterEnum.Bang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenBot == checkingDate.MA(histories, -20)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.NhoHon:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenBot < checkingDate.MA(histories, -20)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.LonHon:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenBot > checkingDate.MA(histories, -20)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.NhoHonHoacBang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenBot <= checkingDate.MA(histories, -20)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.LonHonHoacBang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenBot >= checkingDate.MA(histories, -20)).ToList();
        //            break;
        //        default:
        //            break;
        //    }

        //    return histories;
        //}

        //public static bool NenTopVsGiaMa5(this List<History> histories, History checkingDate, LocCoPhieuFilterEnum filter)
        //{
        //    switch (filter)
        //    {
        //        case LocCoPhieuFilterEnum.Bang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenTop == checkingDate.MA(histories, -5)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.NhoHon:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenTop < checkingDate.MA(histories, -5)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.LonHon:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenTop > checkingDate.MA(histories, -5)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.NhoHonHoacBang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenTop <= checkingDate.MA(histories, -5)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.LonHonHoacBang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenTop >= checkingDate.MA(histories, -5)).ToList();
        //            break;
        //        default:
        //            break;
        //    }

        //    return histories;
        //}

        //public static bool NenBotVsGiaMa5(this List<History> histories, History checkingDate, LocCoPhieuFilterEnum filter)
        //{
        //    switch (filter)
        //    {
        //        case LocCoPhieuFilterEnum.Bang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenBot == checkingDate.MA(histories, -5)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.NhoHon:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenBot < checkingDate.MA(histories, -5)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.LonHon:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenBot > checkingDate.MA(histories, -5)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.NhoHonHoacBang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenBot <= checkingDate.MA(histories, -5)).ToList();
        //            break;
        //        case LocCoPhieuFilterEnum.LonHonHoacBang:
        //            histories = histories.Where(h => h.Date == checkingDate.Date && h.NenBot >= checkingDate.MA(histories, -5)).ToList();
        //            break;
        //        default:
        //            break;
        //    }

        //    return histories;
        //}

        public static bool VolTrenMA20LienTucTrongNPhien(this List<History> histories, History checkingDate, int expected)
        {
            var counter = 0;
            var past = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).ToList();
            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].V > past[i].VOL(histories, -20))
                    counter++;
                else
                    break;
            }

            if (counter < expected)
                return false;

            return true;
        }

        public static bool VolDuoiMA20LienTucTrongNPhien(this List<History> histories, History checkingDate, int expected)
        {
            var counter = 0;
            var past = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).ToList();

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].V < past[i].VOL(histories, -20))
                    counter++;
                else
                    break;
            }

            if (counter < expected)
                return false;

            return true;
        }

        public static bool MA20TangLienTucTrongNPhien(this List<History> histories, History checkingDate, int expected)
        {
            var counter = 0;
            var past = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).ToList();
            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MA(histories, -20) > past[i + 1].MA(histories, -20) * 1.01M)
                    counter++;
                else
                    break;
            }

            if (counter < expected)
                return false;

            return true;
        }

        public static bool MA20GiamLienTucTrongNPhien(this List<History> histories, History checkingDate, int expected)
        {
            var counter = 0;
            var past = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).ToList();

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MA(histories, -20) * 1.01M < past[i + 1].MA(histories, -20))
                    counter++;
                else
                    break;
            }

            if (counter < expected)
                return false;

            return true;
        }

        public static bool MA20DiNgangLienTucTrongNPhien(this List<History> histories, History checkingDate, int expected)
        {
            var counter = 0;
            var past = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).ToList();

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i - i].MA(histories, -20).IsDifferenceInRank(past[i + 1].MA(histories, -20), 1.01M - 1))
                    counter++;
                else
                    break;
            }

            if (counter < expected)
                return false;

            return true;
        }

        public static bool MA5TangLienTucTrongNPhien(this List<History> histories, History checkingDate, int expected)
        {
            var counter = 0;
            var past = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).ToList();
            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MA(histories, -5) > past[i + 1].MA(histories, -5) * 1.01M)
                    counter++;
                else
                    break;
            }

            if (counter < expected)
                return false;

            return true;
        }

        public static bool MA5GiamLienTucTrongNPhien(this List<History> histories, History checkingDate, int expected)
        {
            var counter = 0;
            var past = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).ToList();

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i].MA(histories, -5) * 1.01M < past[i + 1].MA(histories, -5))
                    counter++;
                else
                    break;
            }

            if (counter < expected)
                return false;

            return true;
        }

        public static bool MA5DiNgangLienTucTrongNPhien(this List<History> histories, History checkingDate, int expected)
        {
            var counter = 0;
            var past = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).ToList();

            for (int i = 0; i < past.Count() - 1; i++)
            {
                if (past[i - i].MA(histories, -5).IsDifferenceInRank(past[i + 1].MA(histories, -5), 1.01M - 1))
                    counter++;
                else
                    break;
            }

            if (counter < expected)
                return false;

            return true;
        }

        public static bool PropertyTangLienTucTrongNPhien(this List<History> histories, History checkingDate, string propertyName, int expected, decimal diffValue = 1.01M)
        {
            var counter = 0;
            var past = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).ToList();
            if (past[0].Date != checkingDate.Date) return false;

            for (int i = 0; i < past.Count() - 1; i++)
            {
                var propertyByCurrentIndex = (decimal)past[i].GetPropValue(propertyName);
                var propertyByPastIndex = (decimal)past[i + 1].GetPropValue(propertyName);

                if (propertyByCurrentIndex > propertyByPastIndex * diffValue)
                    counter++;
                else
                    break;
            }

            if (counter < expected)
                return false;

            return true;
        }

        public static bool PropertyGiamLienTucTrongNPhien(this List<History> histories, History checkingDate, string propertyName, int expected, decimal diffValue = 1.01M)
        {
            var counter = 0;
            var past = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).ToList();
            if (past[0].Date != checkingDate.Date) return false;

            for (int i = 0; i < past.Count() - 1; i++)
            {
                var propertyByCurrentIndex = (decimal)past[i].GetPropValue(propertyName);
                var propertyByPastIndex = (decimal)past[i + 1].GetPropValue(propertyName);

                if (propertyByCurrentIndex * diffValue < propertyByPastIndex)
                    counter++;
                else
                    break;
            }

            if (counter < expected)
                return false;

            return true;
        }

        public static bool PropertyDiNgangLienTucTrongNPhien(this List<History> histories, History checkingDate, string propertyName, int expected, decimal diffValue = 1.01M)
        {
            var counter = 0;
            var past = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).ToList();
            if (past[0].Date != checkingDate.Date) return false;

            for (int i = 0; i < past.Count() - 1; i++)
            {
                var propertyByCurrentIndex = (decimal)past[i].GetPropValue(propertyName);
                var propertyByPastIndex = (decimal)past[i + 1].GetPropValue(propertyName);

                var firstNumber = propertyByCurrentIndex;
                var secondNumber = propertyByPastIndex;
                var percentage = diffValue - 1;

                var isDifferenceInRank = (firstNumber - firstNumber * percentage) <= secondNumber && secondNumber <= (firstNumber + (firstNumber * percentage));

                if (isDifferenceInRank)
                    counter++;
                else
                    break;
            }

            if (counter < expected)
                return false;

            return true;
        }

        public static bool PropertyMongMuon(this List<History> histories, History checkingDate, string propertyName, LocCoPhieuFilter filter)
        {
            var dulieuThucTe = (decimal)checkingDate.GetPropValue(propertyName);
            switch (filter.Ope)
            {
                case LocCoPhieuFilterEnum.Bang:
                    if (dulieuThucTe == filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.NhoHon:
                    if (dulieuThucTe < filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.LonHon:
                    if (dulieuThucTe > filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.NhoHonHoacBang:
                    if (dulieuThucTe <= filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.LonHonHoacBang:
                    if (dulieuThucTe >= filter.Value) return true;
                    break;
            }

            //histories.Remove(checkingDate);
            return false;
        }

        public static bool PropertySoSanh(this List<History> histories, History checkingDate, string propertyName1, string propertyName2, LocCoPhieuFilterEnum filter)
        {
            var dulieuThucTe1 = (decimal)checkingDate.GetPropValue(propertyName1);
            var dulieuThucTe2 = (decimal)checkingDate.GetPropValue(propertyName2);
            switch (filter)
            {
                case LocCoPhieuFilterEnum.Bang:
                    if (dulieuThucTe1 == dulieuThucTe2) return true;
                    break;
                case LocCoPhieuFilterEnum.NhoHon:
                    if (dulieuThucTe1 < dulieuThucTe2) return true;
                    break;
                case LocCoPhieuFilterEnum.LonHon:
                    if (dulieuThucTe1 > dulieuThucTe2) return true;
                    break;
                case LocCoPhieuFilterEnum.NhoHonHoacBang:
                    if (dulieuThucTe1 <= dulieuThucTe2) return true;
                    break;
                case LocCoPhieuFilterEnum.LonHonHoacBang:
                    if (dulieuThucTe1 >= dulieuThucTe2) return true;
                    break;
            }

            //histories.Remove(checkingDate);
            return false;
        }

        public static bool PropertySoSanhDuLieu(this List<History> histories, decimal dulieuThucTe1, decimal dulieuThucTe2, LocCoPhieuFilterEnum filter)
        {
            switch (filter)
            {
                case LocCoPhieuFilterEnum.Bang:
                    if (dulieuThucTe1 == dulieuThucTe2) return true;
                    break;
                case LocCoPhieuFilterEnum.NhoHon:
                    if (dulieuThucTe1 < dulieuThucTe2) return true;
                    break;
                case LocCoPhieuFilterEnum.LonHon:
                    if (dulieuThucTe1 > dulieuThucTe2) return true;
                    break;
                case LocCoPhieuFilterEnum.NhoHonHoacBang:
                    if (dulieuThucTe1 <= dulieuThucTe2) return true;
                    break;
                case LocCoPhieuFilterEnum.LonHonHoacBang:
                    if (dulieuThucTe1 >= dulieuThucTe2) return true;
                    break;
            }

            return false;
        }

        /// <summary>
        /// filter.Value = 1.035
        /// </summary>
        public static bool PropertySoSanhDuLieu(this List<History> histories, decimal dulieuThucTe1, decimal dulieuThucTe2, LocCoPhieuFilter filter)
        {
            var tileChenhLech = Math.Abs(dulieuThucTe1) / dulieuThucTe2;
            switch (filter.Ope)
            {
                case LocCoPhieuFilterEnum.Bang:
                    if (tileChenhLech == filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.NhoHon:
                    if (tileChenhLech < filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.LonHon:
                    if (tileChenhLech > filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.NhoHonHoacBang:
                    if (tileChenhLech <= filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.LonHonHoacBang:
                    if (tileChenhLech >= filter.Value) return true;
                    break;
            }

            return false;
        }


        public static bool PropertySoSanhTiLe(this List<History> histories, History checkingDate,
            decimal donViDo,
            string propertyDiemBatDauDo,
            string propertyMucTieuDo, LocCoPhieuFilter filter)
        {
            var dulieuThucTe1 = (decimal)checkingDate.GetPropValue(propertyDiemBatDauDo);
            var dulieuThucTe2 = (decimal)checkingDate.GetPropValue(propertyMucTieuDo);

            var tileChenhLech = Math.Round(Math.Abs(dulieuThucTe2 - dulieuThucTe1) / donViDo, 2);

            switch (filter.Ope)
            {
                case LocCoPhieuFilterEnum.Bang:
                    if (tileChenhLech == filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.NhoHon:
                    if (tileChenhLech < filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.LonHon:
                    if (tileChenhLech > filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.NhoHonHoacBang:
                    if (tileChenhLech <= filter.Value) return true;
                    break;
                case LocCoPhieuFilterEnum.LonHonHoacBang:
                    if (tileChenhLech >= filter.Value) return true;
                    break;
            }

            return false;
        }

        /// <summary>
        /// //TODO: consideration
        /// </summary>
        /// <param name="histories"></param>
        /// <param name="dulieu1Ngay1"></param>
        /// <param name="dulieu2Ngay1"></param>
        /// <param name="dulieu1Ngay2"></param>
        /// <param name="dulieu2Ngay2"></param>
        /// <returns></returns>
        public static bool MA5CatLenMA20(this List<History> histories, decimal phienHumWaMa05, decimal phienHumWaMa20, decimal phienHumNayMa20, decimal phienHumNayMa05)
        {
            return phienHumWaMa05 < phienHumWaMa20 && phienHumNayMa05 > phienHumNayMa20;
        }

        /// <summary>
        /// //TODO: consideration
        /// </summary>
        /// <param name="histories"></param>
        /// <param name="dulieu1Ngay1"></param>
        /// <param name="dulieu2Ngay1"></param>
        /// <param name="dulieu1Ngay2"></param>
        /// <param name="dulieu2Ngay2"></param>
        /// <returns></returns>
        public static bool MA5CatXuongMA20(this List<History> histories, decimal phienHumWaMa05, decimal phienHumWaMa20, decimal phienHumNayMa20, decimal phienHumNayMa05)
        {
            return phienHumWaMa05 > phienHumWaMa20 && phienHumNayMa05 < phienHumNayMa20;
        }

        public static bool PropertyTangDanTrongNPhien(this List<History> histories, History checkingDate, string propertyChecking, int soPhienKiemTra)
        {
            var checkingDateOrdered = histories.OrderByDescending(h => h.Date <= checkingDate.Date).ToList();

            for (int i = 1; i < checkingDateOrdered.Count() - 2; i++)
            {
                var dulieuT0 = (decimal)checkingDateOrdered[i].GetPropValue(propertyChecking);
                var dulieuT1Am = (decimal)checkingDateOrdered[i + 1].GetPropValue(propertyChecking);
                var dulieuT2Am = (decimal)checkingDateOrdered[i + 1].GetPropValue(propertyChecking);
                if (dulieuT0 - dulieuT1Am > dulieuT1Am - dulieuT2Am && i >= soPhienKiemTra)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool PropertyGiamDanTrongNPhien(this List<History> histories, History checkingDate, string propertyChecking, int soPhienKiemTra)
        {
            var checkingDateOrdered = histories.OrderByDescending(h => h.Date <= checkingDate.Date).ToList();

            for (int i = 1; i < checkingDateOrdered.Count() - 2; i++)
            {
                var dulieuT0 = (decimal)checkingDateOrdered[i].GetPropValue(propertyChecking);
                var dulieuT1Am = (decimal)checkingDateOrdered[i + 1].GetPropValue(propertyChecking);
                var dulieuT2Am = (decimal)checkingDateOrdered[i + 1].GetPropValue(propertyChecking);
                if (dulieuT0 - dulieuT1Am < dulieuT1Am - dulieuT2Am && i >= soPhienKiemTra)
                {
                    return true;
                }
            }

            return false;
        }
    }

}
