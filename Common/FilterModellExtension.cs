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

        public static bool PropertyTangLienTucTrongNPhien(this List<History> histories, History checkingDate, string propertyName, int expected, SoSanhEnum soSanh, decimal diffValue = 1.01M)
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

            switch (soSanh)
            {
                case SoSanhEnum.Bang:
                    return counter == expected;
                case SoSanhEnum.NhoHon:
                    return counter < expected;
                case SoSanhEnum.LonHon:
                    return counter > expected;
                case SoSanhEnum.LonHonHoacBang:
                    return counter >= expected;
                case SoSanhEnum.NhoHonHoacBang:
                    return counter <= expected;
                default:
                    return false;
            }
        }

        public static bool PropertyGiamLienTucTrongNPhien(this List<History> histories, History checkingDate, string propertyName, int expected, SoSanhEnum soSanh, decimal diffValue = 1.01M)
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

            switch (soSanh)
            {
                case SoSanhEnum.Bang:
                    return counter == expected;
                case SoSanhEnum.NhoHon:
                    return counter < expected;
                case SoSanhEnum.LonHon:
                    return counter > expected;
                case SoSanhEnum.LonHonHoacBang:
                    return counter >= expected;
                case SoSanhEnum.NhoHonHoacBang:
                    return counter <= expected;
                default:
                    return false;
            }
        }

        public static bool PropertyDiNgangLienTucTrongNPhien(this List<History> histories, History checkingDate, string propertyName, int expected, SoSanhEnum soSanh, decimal diffValue = 1.01M)
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

            switch (soSanh)
            {
                case SoSanhEnum.Bang:
                    return counter == expected;
                case SoSanhEnum.NhoHon:
                    return counter < expected;
                case SoSanhEnum.LonHon:
                    return counter > expected;
                case SoSanhEnum.LonHonHoacBang:
                    return counter >= expected;
                case SoSanhEnum.NhoHonHoacBang:
                    return counter <= expected;
                default:
                    return false;
            }
        }

        public static bool PropertyMongMuon(this List<History> histories, History checkingDate, string propertyName, LocCoPhieuFilter filter)
        {
            var dulieuThucTe = (decimal)checkingDate.GetPropValue(propertyName);
            switch (filter.Ope)
            {
                case SoSanhEnum.Bang:
                    if (dulieuThucTe == filter.Value) return true;
                    break;
                case SoSanhEnum.NhoHon:
                    if (dulieuThucTe < filter.Value) return true;
                    break;
                case SoSanhEnum.LonHon:
                    if (dulieuThucTe > filter.Value) return true;
                    break;
                case SoSanhEnum.NhoHonHoacBang:
                    if (dulieuThucTe <= filter.Value) return true;
                    break;
                case SoSanhEnum.LonHonHoacBang:
                    if (dulieuThucTe >= filter.Value) return true;
                    break;
            }

            //histories.Remove(checkingDate);
            return false;
        }

        public static bool PropertySoSanh(this List<History> histories, History checkingDate, string propertyName1, string propertyName2, SoSanhEnum filter)
        {
            var dulieuThucTe1 = (decimal)checkingDate.GetPropValue(propertyName1);
            var dulieuThucTe2 = (decimal)checkingDate.GetPropValue(propertyName2);
            switch (filter)
            {
                case SoSanhEnum.Bang:
                    if (dulieuThucTe1 == dulieuThucTe2) return true;
                    break;
                case SoSanhEnum.NhoHon:
                    if (dulieuThucTe1 < dulieuThucTe2) return true;
                    break;
                case SoSanhEnum.LonHon:
                    if (dulieuThucTe1 > dulieuThucTe2) return true;
                    break;
                case SoSanhEnum.NhoHonHoacBang:
                    if (dulieuThucTe1 <= dulieuThucTe2) return true;
                    break;
                case SoSanhEnum.LonHonHoacBang:
                    if (dulieuThucTe1 >= dulieuThucTe2) return true;
                    break;
            }

            //histories.Remove(checkingDate);
            return false;
        }

        public static bool PropertySoSanhDuLieu(this List<History> histories, decimal dulieuThucTe1, decimal dulieuThucTe2, SoSanhEnum filter)
        {
            switch (filter)
            {
                case SoSanhEnum.Bang:
                    if (dulieuThucTe1 == dulieuThucTe2) return true;
                    break;
                case SoSanhEnum.NhoHon:
                    if (dulieuThucTe1 < dulieuThucTe2) return true;
                    break;
                case SoSanhEnum.LonHon:
                    if (dulieuThucTe1 > dulieuThucTe2) return true;
                    break;
                case SoSanhEnum.NhoHonHoacBang:
                    if (dulieuThucTe1 <= dulieuThucTe2) return true;
                    break;
                case SoSanhEnum.LonHonHoacBang:
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
                case SoSanhEnum.Bang:
                    if (tileChenhLech == filter.Value) return true;
                    break;
                case SoSanhEnum.NhoHon:
                    if (tileChenhLech < filter.Value) return true;
                    break;
                case SoSanhEnum.LonHon:
                    if (tileChenhLech > filter.Value) return true;
                    break;
                case SoSanhEnum.NhoHonHoacBang:
                    if (tileChenhLech <= filter.Value) return true;
                    break;
                case SoSanhEnum.LonHonHoacBang:
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
                case SoSanhEnum.Bang:
                    if (tileChenhLech == filter.Value) return true;
                    break;
                case SoSanhEnum.NhoHon:
                    if (tileChenhLech < filter.Value) return true;
                    break;
                case SoSanhEnum.LonHon:
                    if (tileChenhLech > filter.Value) return true;
                    break;
                case SoSanhEnum.NhoHonHoacBang:
                    if (tileChenhLech <= filter.Value) return true;
                    break;
                case SoSanhEnum.LonHonHoacBang:
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

        /// <summary>
        /// Chưa ready to use - vì target phải là tính tới tương lai, mà tương lai thì ko có dữ liệu 
        /// </summary>
        /// <param name="histories"></param>
        /// <param name="checkingDate"></param>
        /// <param name="propertyChecking"></param>
        /// <param name="target">0.9M - tức là 2 cạnh phải cao hơn đáy 10% (0.1)</param>
        /// <returns></returns>
        public static bool PropertyTangDanToiKhiDatTargetPhai(this List<History> histories, History checkingDate, string propertyChecking, decimal targetPhai)
        {
            var checkingDateOrdered = histories.OrderByDescending(h => h.Date <= checkingDate.Date).ToList();
            var dulieugoc = (decimal)histories[0].GetPropValue(propertyChecking);
            decimal expectedValuePhai = dulieugoc * targetPhai; //-100 * 0.9 = -90

            for (int i = 1; i < checkingDateOrdered.Count() - 1; i++)
            {
                var dulieuT0 = (decimal)checkingDateOrdered[i].GetPropValue(propertyChecking);
                var dulieuT1Am = (decimal)checkingDateOrdered[i + 1].GetPropValue(propertyChecking);

                if (dulieuT0 > dulieuT1Am)
                {
                    return false;
                }

                dulieugoc += dulieuT1Am - dulieuT0;
                if (dulieugoc > expectedValuePhai)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="histories"></param>
        /// <param name="checkingDate"></param>
        /// <param name="propertyChecking"></param>
        /// <param name="target">0.9M - tức là cạnh trái phải cao hơn đáy 10% (0.1)</param>
        /// <returns></returns>
        public static bool PropertyTangDanToiKhiDatTargetTrai(this List<History> histories, History checkingDate, string propertyChecking, decimal targetTrai)
        {
            var checkingDateOrdered = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= checkingDate.Date).ToList();
            var dulieugoc = (decimal)checkingDateOrdered[0].GetPropValue(propertyChecking);
            decimal expectedValueTrai = dulieugoc < 0
                ? dulieugoc * targetTrai //-100 * 0.9 = -90
                : dulieugoc * (1 + (1 - targetTrai));

            for (int i = 0; i < checkingDateOrdered.Count() - 1; i++)
            {
                var dulieuT0 = (decimal)checkingDateOrdered[i].GetPropValue(propertyChecking);
                var dulieuT1Am = (decimal)checkingDateOrdered[i + 1].GetPropValue(propertyChecking);

                if (dulieuT0 > dulieuT1Am)
                {
                    return false;
                }

                dulieugoc += dulieuT1Am - dulieuT0;
                if (dulieugoc > expectedValueTrai)
                    return true;
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

        public static bool PropertyChecking(this LocCoPhieuCompareModel soSanhDuLieu, List<History> histories, History checkingDate)
        {
            var ngayKiemTra = !soSanhDuLieu.Phien.HasValue || soSanhDuLieu.Phien >= 0
                ? checkingDate
                : histories.OrderByDescending(h => h.Date).Where(h => h.Date < checkingDate.Date).Take(Math.Abs(soSanhDuLieu.Phien.Value)).First();

            var property1 = (decimal)ngayKiemTra.GetPropValue(soSanhDuLieu.Property1);
            var property2 = string.IsNullOrEmpty(soSanhDuLieu.Property2) ? 0 : (decimal)ngayKiemTra.GetPropValue(soSanhDuLieu.Property2);
            var yesterdayData = histories.OrderByDescending(h => h.Date).First(h => h.Date < ngayKiemTra.Date);

            var total = 0M;


            switch (soSanhDuLieu.Operation)
            {
                case OperationEnum.Add:
                    total = property1 + property2;
                    return total.SoSanh(soSanhDuLieu.Sign, soSanhDuLieu.Result);
                case OperationEnum.Minus:
                    total = property1 - property2;
                    return total.SoSanh(soSanhDuLieu.Sign, soSanhDuLieu.Result);
                case OperationEnum.Multiply:
                    total = property1 * property2;
                    return total.SoSanh(soSanhDuLieu.Sign, soSanhDuLieu.Result);
                case OperationEnum.Divide:
                    total = property1 / property2;
                    return total.SoSanh(soSanhDuLieu.Sign, soSanhDuLieu.Result);
                case OperationEnum.CrossUp:
                    var lineProperty1 = new Line();
                    lineProperty1.x1 = 0;  //x là trục tung - trục đối xứng - trục thời gian                    
                    lineProperty1.y1 = (decimal)yesterdayData.GetPropValue(soSanhDuLieu.Property1);
                    lineProperty1.x2 = 1;
                    lineProperty1.y2 = property1;

                    var lineProperty2 = new Line();
                    lineProperty2.x1 = 0;  //x là trục tung - trục đối xứng - trục thời gian                    
                    lineProperty2.y1 = (decimal)yesterdayData.GetPropValue(soSanhDuLieu.Property2);
                    lineProperty2.x2 = 1;
                    lineProperty2.y2 = property2;

                    var crossPoint = lineProperty1.FindIntersection(lineProperty2);

                    var cut = crossPoint != null && property2 < property1;
                    return soSanhDuLieu.Result >= 0 ? cut : !cut;


                case OperationEnum.CrossDown:
                    lineProperty1 = new Line();
                    lineProperty1.x1 = 0;  //x là trục tung - trục đối xứng - trục thời gian                    
                    lineProperty1.y1 = (decimal)yesterdayData.GetPropValue(soSanhDuLieu.Property1);
                    lineProperty1.x2 = 1;
                    lineProperty1.y2 = property1;

                    lineProperty2 = new Line();
                    lineProperty2.x1 = 0;  //x là trục tung - trục đối xứng - trục thời gian                    
                    lineProperty2.y1 = (decimal)yesterdayData.GetPropValue(soSanhDuLieu.Property2);
                    lineProperty2.x2 = 1;
                    lineProperty2.y2 = property2;

                    crossPoint = lineProperty1.FindIntersection(lineProperty2);

                    cut = crossPoint != null && property2 > property1;

                    return soSanhDuLieu.Result >= 0 ? cut : !cut;


                case OperationEnum.ThayDoiTangNPhien:
                    return histories.PropertyTangLienTucTrongNPhien(ngayKiemTra, soSanhDuLieu.Property1, (int)soSanhDuLieu.Result, soSanhDuLieu.Sign, 1);
                case OperationEnum.ThayDoiGiamNPhien:
                    return histories.PropertyGiamLienTucTrongNPhien(ngayKiemTra, soSanhDuLieu.Property1, (int)soSanhDuLieu.Result, soSanhDuLieu.Sign, 1);
                case OperationEnum.ThayDoiNgangNPhien:
                    return histories.PropertyDiNgangLienTucTrongNPhien(ngayKiemTra, soSanhDuLieu.Property1, (int)soSanhDuLieu.Result, soSanhDuLieu.Sign);
                case OperationEnum.SoSanh:
                    switch (soSanhDuLieu.Sign)
                    {
                        case SoSanhEnum.Bang:
                            return property1 == soSanhDuLieu.Result;
                        case SoSanhEnum.NhoHon:
                            return property1 < soSanhDuLieu.Result;
                        case SoSanhEnum.LonHon:
                            return property1 > soSanhDuLieu.Result;
                        case SoSanhEnum.NhoHonHoacBang:
                            return property1 <= soSanhDuLieu.Result;
                        case SoSanhEnum.LonHonHoacBang:
                            return property1 >= soSanhDuLieu.Result;
                    }
                    break;
                case OperationEnum.TrongVong:
                    var soNgayKiemTra = histories.OrderByDescending(h => h.Date).Where(h => h.Date < ngayKiemTra.Date).Take((int)soSanhDuLieu.Result).ToList();
                    for (int i = 0; i < soNgayKiemTra.Count; i++)
                    {
                        var có2Đáy = histories.CoTao2DayChua(soNgayKiemTra[i], soSanhDuLieu.Property1);
                        if (có2Đáy)
                            return có2Đáy;
                    }

                    break;
            }

            return false;
        }

        private static bool SoSanh(this decimal dulieu1, SoSanhEnum operation, decimal dulieu2)
        {
            switch (operation)
            {
                case SoSanhEnum.Bang:
                    return dulieu1 == dulieu2;
                case SoSanhEnum.NhoHon:
                    return dulieu1 < dulieu2;
                case SoSanhEnum.LonHon:
                    return dulieu1 > dulieu2;
                case SoSanhEnum.NhoHonHoacBang:
                    return dulieu1 <= dulieu2;
                case SoSanhEnum.LonHonHoacBang:
                    return dulieu1 >= dulieu2;
            }

            return false;
        }

        /// <summary>
        /// CT Bắt đáy khi giảm mạnh
        /// <para> + Tính RSI hiện tại, đếm ngược lại những ngày trước đó mà RSI vẫn đang giảm và các nến đều là nến đỏ</para>
        /// <para> + Đi ngược lại tìm nến cao nhất</para>
        /// <para> + Tính từ giá đóng của của cây xanh cao nhất, so với giá hiện tại, nếu hiện tại giá đã giảm > 20%</para>
        /// <para> -> Giá gợi ý mua từ [giá đóng của hum nay - 1/2 thân nến hôm nay] tới [giá C hum nay + 1/5 thân nên hum nay], tuyệt đối ko mua nếu giá mở cửa có tạo GAP cao hơn giá mở cửa của phiên hum nay</para>
        /// </summary>
        /// <param name="histories"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static bool BatDayDcChua(this List<History> histories, History history)
        {
            //return false;
            //Version 1
            var rsiGiamLienTucTrongXPhien = 0;
            var checkingHistoies = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= history.Date).ToList();

            for (int i = 0; i < checkingHistoies.Count(); i++)
            {
                if (checkingHistoies[i].RSI < checkingHistoies[i + 1].RSI)
                {
                    rsiGiamLienTucTrongXPhien++;
                }
                else
                {
                    break;
                }

                if (checkingHistoies[i + 1].TangGia()) break;
            }

            if (rsiGiamLienTucTrongXPhien <= 0) return false;
            var priceOfNgayBatDauGiam = checkingHistoies[rsiGiamLienTucTrongXPhien].C;
            var priceOfNgayHienTai = history.C;
            var canBatDay = priceOfNgayHienTai <= priceOfNgayBatDauGiam * 0.8M;
            return canBatDay;



            //Version 2
            //var rsiGiamLienTucTrongXPhien = 0;
            //var checkingHistoies = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= history.Date).ToList();

            //var priceOfNgayHienTai = 0M;
            //var lstLosingDates = new List<History>();

            //for (int i = 0; i < checkingHistoies.Count(); i++)
            //{
            //    if (checkingHistoies[i].RSI < checkingHistoies[i + 1].RSI)
            //    {
            //        //rsiGiamLienTucTrongXPhien++;
            //        lstLosingDates.Add(histories[i]);
            //    }
            //    else
            //    {
            //        break;
            //    }

            //    if (checkingHistoies[i + 1].TangGia())
            //    {
            //        lstLosingDates.Add(histories[i]);
            //        break;
            //    }
            //}

            //if (!lstLosingDates.Any()) return false;

            //lstLosingDates = lstLosingDates.OrderBy(h => h.Date).ToList();

            //for (int i = 1; i < lstLosingDates.Count; i++)
            //{
            //    var coTaoGapGiaGiam = lstLosingDates[i].O < lstLosingDates[i - 1].C;
            //    priceOfNgayHienTai = coTaoGapGiaGiam
            //        ? lstLosingDates[i].C - lstLosingDates[i].O + lstLosingDates[i - 1].C
            //        : lstLosingDates[i].C;
            //}

            //var priceOfNgayBatDauGiam = checkingHistoies[rsiGiamLienTucTrongXPhien].C;

            //var canBatDay = priceOfNgayHienTai <= priceOfNgayBatDauGiam * 0.8M;
            //return canBatDay;
        }


        /// <summary>
        /// /*  Giá của phiên hiện tại so với giá đỉnh của phiên quá khứ trong vòng 60 phiên phải có sự tương quan
        ///  *  Ví dụ:
        ///  *      Giá phiên hiện tại so với đỉnh quá khứ  > : 
        ///  *                                              = : 
        ///  *                                         be hon : MUA: nếu Giá hiện tại <= Giá quá khứ * (1 - ((RSI quá khứ / RSI hiện tại) / 100)        - TODO: chưa làm
        ///  *                                                  MUA: nếu Giá hiện tại <= Giá quá khứ * (1 - ((MACD quá khứ / MACD hiện tại) / 100)      - làm trong CT này
        ///  *                                              
        ///  */
        /// </summary>
        public static bool TuongQuanGiuaGiaVaMACD(this List<History> histories, History history)
        {
            var checkingHistories = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= history.Date).Take(60).ToList();
            if (!checkingHistories.Any()) return false;
            var dinhCaoNhat = checkingHistories.OrderByDescending(h => h.NenTop).First();


            if (history.C <= dinhCaoNhat.C)
            {
                return history.C <= dinhCaoNhat.C * (1 - (dinhCaoNhat.MACD / history.MACD) / 100);
            }

            return false;
        }

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
        public static bool KiemTraTangManhTuDay(this List<History> histories, History history)
        {
            var checkingHistories = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= history.Date).Take(20).ToList();
            if (!checkingHistories.Any()) return false;

            var dayThapNhat = checkingHistories.OrderBy(h => h.NenBot).First();

            var indexNgayKiemTra = histories.IndexOf(history);
            var indexDayThapNhat = histories.IndexOf(dayThapNhat);

            if (indexNgayKiemTra - indexDayThapNhat > 7) return false;
            if (history.C <= dayThapNhat.NenTop) return false;

            var khoangCachMongMuon = history.C / dayThapNhat.NenTop;
            if (khoangCachMongMuon <= 1.1M) return false;

            return true;
        }

        public static bool FullMargin(this List<History> histories, History history)
        {
            var soNgayMuonKiemTraTrongQuaKhu = 11;
            var soNgayKiemTra = histories.OrderByDescending(h => h.Date).Where(h => h.Date < history.Date).Take(soNgayMuonKiemTraTrongQuaKhu).ToList();
            var day2 = new History();
            for (int i = 0; i < soNgayKiemTra.Count; i++)
            {
                var có2Đáy = histories.CoTao2DayChua(soNgayKiemTra[i], "MACD");
                if (có2Đáy)
                {
                    day2 = soNgayKiemTra[i];
                    break;
                }
            }

            if (day2.ID <= 0) return false;

            var lstNgayTuDay2ToiHienTai = histories.OrderByDescending(h => h.Date).Where(h => h.Date < history.Date && h.Date >= day2.Date).ToList();

            var propertyTangLienTuc = histories.PropertyTangDanTrongNPhien(history, "MACD", lstNgayTuDay2ToiHienTai.Count());
            if (!propertyTangLienTuc) return false;

            var lstSoSanhMA20NgayTuDay2ToiHienTai = lstNgayTuDay2ToiHienTai.Where(h => h.NenBot > h.BandsMid).ToList();
            if (!lstSoSanhMA20NgayTuDay2ToiHienTai.Any()) return false;

            if (lstSoSanhMA20NgayTuDay2ToiHienTai.Count() == 1) return true;

            var kcXuongMA20XaNhat = lstSoSanhMA20NgayTuDay2ToiHienTai.OrderByDescending(h => h.NenBot).First();
            var kcXuongMA20NganNhat = lstSoSanhMA20NgayTuDay2ToiHienTai.OrderByDescending(h => h.NenBot).Last();

            if (kcXuongMA20XaNhat.Date > kcXuongMA20NganNhat.Date) return false;

            return true;
        }


        public static bool RSIAmTheoNgay(this List<History> histories, History history)
        {
            var humnay = history;
            var humqua = histories.OrderByDescending(h => h.Date).First(h => h.Date < humnay.Date);

            return humnay.RSI > humqua.RSI && (humnay.C < humqua.C || humnay.NenBot < humqua.NenBot);
            //|| humnay.NenTop < humqua.NenTop);
        }
    }

}
