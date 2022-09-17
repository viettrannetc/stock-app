using DotNetCoreSqlDb.Common.ArrayExtensions;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace DotNetCoreSqlDb.Common
{
    public static class BusinessHourModellExtension
    {
        public static bool VOLBienDong(this HistoryHour checkingDate, List<HistoryHour> histories, int numberOfPreviousPhien, decimal bienDong)
        {
            if (numberOfPreviousPhien == 0) return false;

            var checkingRange = new List<HistoryHour>();
            if (numberOfPreviousPhien < 0)
            {
                numberOfPreviousPhien = numberOfPreviousPhien * -1;
                checkingRange = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).Take(numberOfPreviousPhien).ToList();
            }
            else
                checkingRange = histories.Where(h => h.Date >= checkingDate.Date).OrderBy(h => h.Date).Take(numberOfPreviousPhien).ToList();

            if (checkingRange.Count != numberOfPreviousPhien) return false;

            var max = checkingRange.Max(h => h.V);
            var min = checkingRange.Min(h => h.V);
            var avg = checkingRange.Sum(h => h.V) / numberOfPreviousPhien;

            var dk1 = avg.IsDifferenceInRank(max, bienDong);
            var dk2 = avg.IsDifferenceInRank(min, bienDong);

            return dk1 && dk2;
        }

        public static decimal VOL(this HistoryHour checkingDate, List<HistoryHour> histories, int numberOfPreviousPhien)
        {
            if (numberOfPreviousPhien == 0) return 0;

            var checkingRange = new List<HistoryHour>();
            if (numberOfPreviousPhien < 0)
            {
                numberOfPreviousPhien = numberOfPreviousPhien * -1;
                checkingRange = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).Take(numberOfPreviousPhien).ToList();
            }
            else
                checkingRange = histories.Where(h => h.Date >= checkingDate.Date).OrderBy(h => h.Date).Take(numberOfPreviousPhien).ToList();

            if (checkingRange.Count != numberOfPreviousPhien) return 0;

            return checkingRange.Sum(h => h.V) / numberOfPreviousPhien;
        }

        public static decimal MA(this HistoryHour checkingDate, List<HistoryHour> histories, int numberOfPreviousPhien)
        {
            if (numberOfPreviousPhien == 0) return 0;

            var checkingRange = new List<HistoryHour>();
            if (numberOfPreviousPhien < 0)
            {
                numberOfPreviousPhien = numberOfPreviousPhien * -1;
                checkingRange = histories.Where(h => h.Date <= checkingDate.Date).OrderByDescending(h => h.Date).Take(numberOfPreviousPhien).ToList();
            }
            else
                checkingRange = histories.Where(h => h.Date >= checkingDate.Date).OrderBy(h => h.Date).Take(numberOfPreviousPhien).ToList();

            if (checkingRange.Count != numberOfPreviousPhien) return 0;

            return checkingRange.Sum(h => h.C) / numberOfPreviousPhien;
        }

        public static bool IsPriceUp(this HistoryHour today, List<HistoryHour> histories, int numberOfSideway)
        {
            var todayVol = today.C;
            var averageOfVolInSideway = today.MAPrice(histories, numberOfSideway);

            return todayVol > averageOfVolInSideway * 1.05M;
        }

        public static bool IsPriceDown(this HistoryHour today, List<HistoryHour> histories, int numberOfSideway)
        {
            var todayVol = today.C;
            var averageOfVolInSideway = today.MAPrice(histories, numberOfSideway);

            return todayVol * 1.05M < averageOfVolInSideway;
        }


        public static bool IsVolUp(this HistoryHour today, List<HistoryHour> histories, int numberOfSideway)
        {
            var todayVol = today.V;
            var averageOfVolInSideway = today.MASideway(histories, numberOfSideway);

            return todayVol > averageOfVolInSideway * 1.05M;
        }

        public static bool IsVolDown(this HistoryHour today, List<HistoryHour> histories, int numberOfSideway)
        {
            var todayVol = today.V;
            var averageOfVolInSideway = today.MASideway(histories, numberOfSideway);

            return todayVol * 1.05M < averageOfVolInSideway;
        }

        public static decimal MASideway(this HistoryHour today, List<HistoryHour> histories, int numberOfSideway)
        {
            return histories.Where(h => h.Date < today.Date).OrderByDescending(h => h.Date).Take(numberOfSideway).Sum(h => h.V) / numberOfSideway;
        }

        public static decimal MAPrice(this HistoryHour today, List<HistoryHour> histories, int numberOfSideway)
        {
            return histories.Where(h => h.Date < today.Date).OrderByDescending(h => h.Date).Take(numberOfSideway).Sum(h => h.C) / numberOfSideway;
        }

        public static bool DangBiCanhCaoGD1Tuan(this HistoryHour checkingDate, List<HistoryHour> histories)
        {
            var latestHistories = histories
                .Where(h => h.Date <= checkingDate.Date && h.V > 0)
                .OrderByDescending(h => h.Date)
                .Take(3)
                .ToList();

            return latestHistories.All(h => h.Date.DayOfWeek == DayOfWeek.Friday);
        }

        /// <summary>
        /// Lấy giá mở của của T3 để xem thời điểm bán có lời hay ko
        /// </summary>
        /// <param name="checkingDate"></param>
        /// <param name="histories"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public static decimal LayGiaCuaPhienSau(this HistoryHour checkingDate, List<HistoryHour> histories, int T)
        {
            if (T < 0) return 0;

            if (T == 0) return checkingDate.C;

            var nextTransactions = histories
                .Where(h => h.Date > checkingDate.Date && h.V > 0)
                .OrderBy(h => h.Date)
                .Take(T)
                .ToList();

            if (!nextTransactions.Any() || nextTransactions.Count() < T) return 0;

            var transaction = nextTransactions.OrderByDescending(t => t.Date).First();

            return transaction.O;
        }

        /// <summary>
        /// Lấy giá đóng cửa của các phiên sau để xem thời điểm bán có lời hay ko
        /// </summary>
        /// <param name="checkingDate"></param>
        /// <param name="histories"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public static decimal LayGiaCaoNhatCuaCacPhienSau(this HistoryHour checkingDate, List<HistoryHour> histories, int fromT, int toT)
        {
            if (toT < fromT) return 0;

            //if (T == 0) return checkingDate.C;

            var nextTransactions = histories
                .Where(h => h.Date > checkingDate.Date && h.V > 0)
                .OrderBy(h => h.Date)
                .Skip(fromT)
                .Take(toT)
                .ToList();

            if (!nextTransactions.Any() || nextTransactions.Count() < (toT - fromT)) return 0;

            var transaction = nextTransactions.OrderByDescending(t => t.C).First();

            return transaction.C;
        }

        public static HistoryHour LookingForLowestWithout2Percent(this HistoryHour currentDateHistory, List<HistoryHour> histories)//, HistoryHour currentDateHistory)
        {
            if (currentDateHistory == null) return null;

            var history = histories.FirstOrDefault(h => h.Date == currentDateHistory.Date);
            if (history == null) return null;

            var currentDateToCheck = history.Date;
            var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(30).ToList();

            var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();

            return lowest;
        }

        public static HistoryHour LookingForSecondLowestWithout2Percent(this HistoryHour lowest, List<HistoryHour> histories, HistoryHour currentDateHistory)
        {
            var theDaysAfterLowest = histories.Where(h => h.Date > lowest.Date && h.Date <= currentDateHistory.Date)
                .OrderBy(h => h.Date)
                .ToList();

            if (!theDaysAfterLowest.Any()) return null;

            for (int i = 0; i < theDaysAfterLowest.Count(); i++)
            {
                if (i <= 3) continue;
                var secondLowestAssumption = theDaysAfterLowest[i];
                var rangesFromLowestTo2ndLowest = theDaysAfterLowest.Where(d => d.Date > lowest.Date && d.Date < secondLowestAssumption.Date).ToList();

                var dkSub1 = rangesFromLowestTo2ndLowest.Any(r => r.C > secondLowestAssumption.C);//at least 1 day > 2nd lowest
                if (!dkSub1) continue;

                var dkSub2 = false;//at least 1 day > next Phien from 2nd lowest
                if (i < theDaysAfterLowest.Count() - 1)
                {
                    var nextPhien = theDaysAfterLowest[i + 1];
                    dkSub2 = rangesFromLowestTo2ndLowest.Any(r => r.C > nextPhien.C) && nextPhien.C > secondLowestAssumption.C; //(secondLowestAssumption.C * 1.02M);
                }

                if (dkSub1 && dkSub2)
                {
                    return secondLowestAssumption;
                }
            }

            return null;
        }

        public static HistoryHour LookingForSecondLowest(this HistoryHour lowest, List<HistoryHour> histories, HistoryHour currentDateHistory, bool included2PercentHigher = false)
        {
            var theDaysAfterLowest = histories.Where(h => h.Date > lowest.Date && h.Date <= currentDateHistory.Date)
                .OrderBy(h => h.Date)
                .ToList();

            if (!theDaysAfterLowest.Any()) return null;

            for (int i = 0; i < theDaysAfterLowest.Count(); i++)
            {
                if (i <= 3) continue;
                var secondLowestAssumption = theDaysAfterLowest[i];
                var rangesFromLowestTo2ndLowest = theDaysAfterLowest.Where(d => d.Date > lowest.Date && d.Date < secondLowestAssumption.Date).ToList();

                var dkSub1 = rangesFromLowestTo2ndLowest.Any(r => r.C > secondLowestAssumption.C);//at least 1 day > 2nd lowest
                if (!dkSub1) continue;

                var dkSub2 = false;//at least 1 day > next Phien from 2nd lowest
                if (i < theDaysAfterLowest.Count() - 1)
                {
                    var nextPhien = theDaysAfterLowest[i + 1];
                    dkSub2 = included2PercentHigher
                        ? rangesFromLowestTo2ndLowest.Any(r => r.C > nextPhien.C) && nextPhien.C > (secondLowestAssumption.C * 1.02M)
                        : rangesFromLowestTo2ndLowest.Any(r => r.C > nextPhien.C) && nextPhien.C > secondLowestAssumption.C;
                }

                if (dkSub1 && dkSub2)
                {
                    return secondLowestAssumption;
                }
            }

            return null;
        }

        public static HistoryHour LookingForSecondLowestWithCheckingDate(this HistoryHour lowest, List<HistoryHour> histories, HistoryHour currentDateHistory, bool included2PercentHigher = false)
        {
            var theDaysAfterLowest = histories.Where(h => h.Date > lowest.Date && h.Date < currentDateHistory.Date)
                .OrderBy(h => h.Date)
                .ToList();

            if (!theDaysAfterLowest.Any()) return null;

            var adjustmentDays = theDaysAfterLowest.Skip(4).ToList();
            var secondLowestAssumption = theDaysAfterLowest.OrderByDescending(h => h.Date).FirstOrDefault();
            if (secondLowestAssumption == null) return null;

            var rangesFromLowestTo2ndLowest = theDaysAfterLowest.Where(d => d.Date > lowest.Date && d.Date < secondLowestAssumption.Date).ToList();
            var dkSub1 = rangesFromLowestTo2ndLowest.Any(r => r.C > secondLowestAssumption.C);//at least 1 day > 2nd lowest

            var dkSub2 = included2PercentHigher
                        ? adjustmentDays.Any(r => r.C > currentDateHistory.C) && currentDateHistory.C > (secondLowestAssumption.C * 1.02M)
                        : adjustmentDays.Any(r => r.C > currentDateHistory.C) && currentDateHistory.C > secondLowestAssumption.C;

            if (dkSub1 && dkSub2) return secondLowestAssumption;

            return null;
        }

        public static HistoryHour LookingForSecondLowestWithoutLowest(List<HistoryHour> histories, HistoryHour currentDateHistory, bool included2PercentHigher = false)
        {
            var theDaysInThePast = histories.Where(h => h.Date < currentDateHistory.Date)
                .OrderByDescending(h => h.Date)
                .ToList();

            if (!theDaysInThePast.Any()) return null;

            foreach (var secondLowestAssumption in theDaysInThePast)
            {
                var rangesFromLowestTo2ndLowest = theDaysInThePast.Where(d => d.Date < secondLowestAssumption.Date).Take(20).ToList();

                var day1 = rangesFromLowestTo2ndLowest.OrderBy(h => h.C).FirstOrDefault();

                var dkSub1 = rangesFromLowestTo2ndLowest.Any(r => r.C > secondLowestAssumption.C);//at least 1 day > 2nd lowest

                var adjustmentDays = rangesFromLowestTo2ndLowest.Skip(4).ToList();
                var dkSub2 = included2PercentHigher
                        ? adjustmentDays.Any(r => r.C > currentDateHistory.C) && currentDateHistory.C > (secondLowestAssumption.C * 1.02M)
                        : adjustmentDays.Any(r => r.C > currentDateHistory.C) && currentDateHistory.C > secondLowestAssumption.C;

                if (dkSub1 && dkSub2) return secondLowestAssumption;
            }

            return null;
        }

        public static HistoryHour LookingForLowest(this List<HistoryHour> histories, HistoryHour currentDateHistory)
        {
            var h1 = histories.Where(h => h.Date < currentDateHistory.Date).OrderByDescending(h => h.Date).ToList();

            foreach (var day1Ao in h1)
            {
                if (day1Ao.C * 1.15M <= currentDateHistory.C) continue;

                var BaMuoiPhienTruoc = histories.Where(h => h.Date < day1Ao.Date).OrderByDescending(h => h.Date).Take(30).ToList();
                if (BaMuoiPhienTruoc == null || !BaMuoiPhienTruoc.Any()) continue;

                if (BaMuoiPhienTruoc.Any(b => b.C > day1Ao.C * 1.15M))
                    return day1Ao;
            }

            return null;
        }

        public static bool DidDay2ShowYesterday(this List<HistoryHour> histories, HistoryHour currentDateHistory, out HistoryHour dinh1, out HistoryHour day1, out HistoryHour day2)
        {
            var h1 = histories.Where(h => h.Date < currentDateHistory.Date).OrderByDescending(h => h.Date).ToList();
            day1 = new HistoryHour();
            day2 = new HistoryHour();
            dinh1 = new HistoryHour();

            foreach (var day1Ao in h1)
            {
                if (day1Ao.C > currentDateHistory.C
                    || day1Ao.C * 1.15M <= currentDateHistory.C) continue;

                var BaMuoiPhienTruoc = histories.Where(h => h.Date < day1Ao.Date).OrderByDescending(h => h.Date).Take(30).ToList();
                if (BaMuoiPhienTruoc == null || !BaMuoiPhienTruoc.Any()) continue;

                var dinh1Ao = BaMuoiPhienTruoc.FirstOrDefault(b => b.C > day1Ao.C * 1.15M);
                if (dinh1Ao == null) continue;
                if (day1Ao == null) continue;

                var day2Ao = day1Ao.LookingForSecondLowestWithCheckingDate(histories, currentDateHistory);
                if (day2Ao == null) continue;

                var dk1 = day1Ao.C * 1.15M >= day2Ao.C && day1Ao.C < day2Ao.C;

                if (dk1)
                {
                    dinh1 = dinh1Ao;
                    day1 = day1Ao;
                    day2 = day2Ao;
                    return true;
                }
            }
            return false;
        }


        public static bool DidDay2ShowYesterdayStartWithDay2(this List<HistoryHour> histories, HistoryHour currentDateHistory, out HistoryHour dinh1, out HistoryHour day1, out HistoryHour day2)
        {
            //var h1 = histories.Where(h => h.Date < currentDateHistory.Date).OrderByDescending(h => h.Date).ToList();

            day1 = new HistoryHour();
            day2 = new HistoryHour();
            dinh1 = new HistoryHour();

            var theDaysInThePast = histories.Where(h => h.Date < currentDateHistory.Date)
                .OrderByDescending(h => h.Date)
                .ToList();

            if (!theDaysInThePast.Any()) return false;

            var day2Ao = theDaysInThePast.First();
            if (day2Ao == null) return false;
            //foreach (var day2Ao in theDaysInThePast)
            //{
            var rangesFromLowestTo2ndLowest = theDaysInThePast.Where(d => d.Date < day2Ao.Date).Take(20).ToList();

            var dkSub1 = rangesFromLowestTo2ndLowest.Any(r => r.C > day2Ao.C);//at least 1 day > 2nd lowest

            var adjustmentDays = rangesFromLowestTo2ndLowest.Skip(4).ToList();
            var dkSub2 = adjustmentDays.Any(r => r.C > currentDateHistory.C) && currentDateHistory.C > day2Ao.C;

            var dkCoDay2 = dkSub1 && dkSub2;

            var day1Ao = rangesFromLowestTo2ndLowest.OrderBy(h => h.C).FirstOrDefault();
            if (day1Ao == null) return false;

            var dk1 = day1Ao.C * 1.15M >= day2Ao.C && day1Ao.C < day2Ao.C;

            if (day1Ao.C > currentDateHistory.C || day1Ao.C * 1.15M <= currentDateHistory.C) return false;

            var BaMuoiPhienTruoc = histories.Where(h => h.Date < day1Ao.Date).OrderByDescending(h => h.Date).Take(30).ToList();
            if (BaMuoiPhienTruoc == null || !BaMuoiPhienTruoc.Any()) return false;

            var dinh1Ao = BaMuoiPhienTruoc.FirstOrDefault(b => b.C > day1Ao.C * 1.15M);
            if (dinh1Ao == null) return false;


            if (dk1 && dkCoDay2)
            {
                dinh1 = dinh1Ao;
                day1 = day1Ao;
                day2 = day2Ao;
                return true;
            }
            //}

            return false;

            //foreach (var day1Ao in h1)
            //{
            //    if (day1Ao.C > currentDateHistory.C
            //        || day1Ao.C * 1.15M <= currentDateHistory.C) continue;

            //    var BaMuoiPhienTruoc = histories.Where(h => h.Date < day1Ao.Date).OrderByDescending(h => h.Date).Take(30).ToList();
            //    if (BaMuoiPhienTruoc == null || !BaMuoiPhienTruoc.Any()) continue;

            //    var dinh1Ao = BaMuoiPhienTruoc.FirstOrDefault(b => b.C > day1Ao.C * 1.15M);
            //    if (dinh1Ao == null) continue;
            //    if (day1Ao == null) continue;

            //    var day2Ao = day1Ao.LookingForSecondLowestWithCheckingDate(histories, currentDateHistory);
            //    if (day2Ao == null) continue;

            //    var dk1 = day1Ao.C * 1.15M >= day2Ao.C && day1Ao.C < day2Ao.C;

            //    if (dk1)
            //    {
            //        dinh1 = dinh1Ao;
            //        day1 = day1Ao;
            //        day2 = day2Ao;
            //        return true;
            //    }
            //}
            //return false;
        }


        public static decimal RSI(this HistoryHour history, List<HistoryHour> histories, int rsi)
        {
            var historiesFromToday = histories.Where(h => h.Date <= history.Date).OrderByDescending(h => h.Date).ToList();

            var rsiDays = historiesFromToday.Take(rsi + 1).ToList();

            if (rsiDays.Count < rsi) return 0;

            decimal gainValues = 0;
            decimal lossValues = 0;

            for (int i = 0; i < rsi; i++)
            {
                var differentValue = historiesFromToday[i].C - historiesFromToday[i + 1].C;

                if (differentValue > 0) gainValues += differentValue;
                if (differentValue < 0) lossValues += differentValue;
            }

            var rs = (gainValues / lossValues) * (-1);

            var rsiValue = 100 - 100 / (rs + 1);

            return rsiValue;
        }

        public static Tuple<decimal, decimal, decimal> RSIDetail(this HistoryHour history, List<HistoryHour> histories, int rsi)
        {
            var historiesFromToday = histories.Where(h => h.Date <= history.Date).OrderByDescending(h => h.Date).Take(rsi + 1).ToList();

            var rsiDays = historiesFromToday.OrderBy(h => h.Date).ToList();

            if (rsiDays.Count < rsi) return null;

            decimal gainValues = 0;
            decimal lossValues = 0;

            for (int i = 0; i < rsi; i++)
            {
                var differentValue = rsiDays[i + 1].C - rsiDays[i].C;

                if (differentValue > 0) gainValues += differentValue;
                if (differentValue < 0) lossValues += differentValue;
            }

            if (lossValues == 0) return new Tuple<decimal, decimal, decimal>((gainValues / 14), 0, 0);

            var rs = (gainValues / 14) / (lossValues / 14) * (-1);

            var rsiValue = 100 - 100 / (rs + 1);

            return new Tuple<decimal, decimal, decimal>((gainValues / 14), (lossValues / 14) * (-1), rsiValue);
        }

        public static List<decimal> MACD(this HistoryHour history, List<HistoryHour> histories, int rsi)
        {
            var rsiDays = histories.Where(h => h.Date <= history.Date).OrderByDescending(h => h.Date).Take(rsi + 1).ToList();

            decimal gainValues = 0;
            decimal lossValues = 0;

            for (int i = 0; i < rsi; i++)
            {
                var differentValue = histories[i].C - histories[i + 1].C;

                if (differentValue > 0) gainValues += differentValue;
                if (differentValue < 0) lossValues += differentValue;
            }

            var rs = (gainValues / lossValues) * (-1);

            var rsiValue = 100 - 100 / (rs + 1);

            return null;
        }

        public static decimal SMA(this HistoryHour history, List<HistoryHour> histories, int rsi)
        {
            var rsiDays = histories.Where(h => h.Date <= history.Date).OrderByDescending(h => h.Date).Take(rsi + 1).ToList();

            decimal gainValues = 0;
            decimal lossValues = 0;

            for (int i = 0; i < rsi; i++)
            {
                var differentValue = histories[i].C - histories[i + 1].C;

                if (differentValue > 0) gainValues += differentValue;
                if (differentValue < 0) lossValues += differentValue;
            }

            var rs = (gainValues / lossValues) * (-1);

            var rsiValue = 100 - 100 / (rs + 1);

            return rsiValue;
        }

        public static bool TangGia(this HistoryHour today)
        {
            return today.C > today.O;
        }

        /// <summary>
        /// Đỉnh sau thấp hơn đỉnh trước
        /// Đáy sau thấp hơn đáy trước
        /// </summary>
        /// <param name="today"></param>
        /// <returns></returns>
        public static bool NếnGiảm(this List<HistoryHour> histories, HistoryHour history, int trongNPhien)
        {
            var rsiDays = histories.Where(h => h.Date < history.Date).OrderByDescending(h => h.Date).ToList();

            int counter = 0;

            for (int i = 0; i < rsiDays.Count - 1; i++)
            {
                if (rsiDays[i].H < rsiDays[i + 1].H && rsiDays[i].L < rsiDays[i + 1].L)
                    counter++;
                else
                    break;
            }

            return counter >= trongNPhien;
        }

        public static bool Doji(this HistoryHour today)
        {
            return today.C.IsDifferenceInRank(today.O, 0.01M);
        }

        /// <summary>
        /// TODO: Kháng cự có thể là 1 biên rộng, nên 2 giá trị có thể là hợp lí
        /// </summary>
        /// <param name="today"></param>
        /// <param name="histories"></param>
        /// <returns></returns>
        public static decimal KhángCựĐỉnh(this HistoryHour today, List<HistoryHour> histories)
        {
            return 0;
        }

        /// <summary>
        /// TODO: Kháng cự có thể là 1 biên rộng, nên 2 giá trị có thể là hợp lí
        /// </summary>
        /// <param name="today"></param>
        /// <param name="histories"></param>
        /// <returns></returns>
        public static decimal KhángCựBands(this HistoryHour today, List<HistoryHour> histories)
        {
            return 0;
        }

        ////public static void AddBollingerBands(ref SortedList<DateTime, Dictionary<string, double>> data, int period, int factor)
        //public static void AddBollingerBands(this List<HistoryHour> histories, int period, int factor)
        //{
        //    double total_average = 0;
        //    double total_squares = 0;

        //    for (int i = 0; i < data.Count(); i++)
        //    {
        //        total_average += data.Values[i]["close"];
        //        total_squares += Math.Pow(data.Values[i]["close"], 2);

        //        if (i >= period - 1)
        //        {
        //            double total_bollinger = 0;
        //            double average = total_average / period;

        //            double stdev = Math.Sqrt((total_squares - Math.Pow(total_average, 2) / period) / period);
        //            data.Values[i]["bollinger_average"] = average;
        //            data.Values[i]["bollinger_top"] = average + factor * stdev;
        //            data.Values[i]["bollinger_bottom"] = average - factor * stdev;

        //            total_average -= data.Values[i - period + 1]["close"];
        //            total_squares -= Math.Pow(data.Values[i - period + 1]["close"], 2);
        //        }
        //    }
        //}

        /// <summary>
        /// TODO: Kháng cự có thể là 1 biên rộng, nên 2 giá trị có thể là hợp lí
        /// </summary>
        /// <param name="today"></param>
        /// <param name="histories"></param>
        /// <returns></returns>
        public static decimal KhángCựFibonacci(this HistoryHour today, List<HistoryHour> histories)
        {
            return 0;
        }

        /// <summary>
        /// TODO: Kháng cự có thể là 1 biên rộng, nên 2 giá trị có thể là hợp lí
        /// </summary>
        /// <param name="today"></param>
        /// <param name="histories"></param>
        /// <returns></returns>
        public static decimal KhángCựIchimoku(this HistoryHour today, List<HistoryHour> histories)
        {
            return 0;
        }

        public static bool NếnĐảoChiều(this HistoryHour phienKiemTra)
        {
            var nếnĐảoChiều = phienKiemTra.NenTop < phienKiemTra.BandsBot
                                    && (phienKiemTra.H - phienKiemTra.NenTop) / 2 > (phienKiemTra.NenTop - phienKiemTra.NenBot);
            //var bắtĐầuMuaDoNếnTăngA1 = nếnĐảoChiều && phienKiemTra.TangGia() ? true : false;

            return nếnĐảoChiều;
        }

        public static bool NếnĐảoChiềuTăngMạnhA1(this HistoryHour phienKiemTra)
        {
            return phienKiemTra.NếnĐảoChiều() && phienKiemTra.TangGia() ? true : false;
        }

        public static bool NếnĐảoChiềuTăngMạnhA2(this HistoryHour phienKiemTra, HistoryHour phiênTrướcPhiênKiemTra)
        {
            return phiênTrướcPhiênKiemTra.NếnĐảoChiều() && !phiênTrướcPhiênKiemTra.TangGia() && phienKiemTra.TangGia() ? true : false;
        }

        /// <summary>
        /// Đơn giản là so sánh về quá khứ lấy 1 cây cùng giá đóng cửa, thấy RSI cây hiện tại cao hơn thì dương, còn ko thì âm
        /// </summary>
        /// <param name="phienKiemTra"></param>
        /// <param name="histories"></param>
        /// <returns></returns>
        public static bool RSIDương(this HistoryHour phienKiemTra, List<HistoryHour> histories)
        {
            var checkingList = histories.Where(h => h.Date < phienKiemTra.Date).OrderByDescending(h => h.Date).Skip(3).Take(52).ToList();
            var comparingHistory = checkingList.Where(h => h.C == phienKiemTra.C).OrderByDescending(h => h.Date).FirstOrDefault();

            if (comparingHistory != null)
                return comparingHistory.RSI < phienKiemTra.RSI;

            comparingHistory = checkingList.Where(h => h.C >= phienKiemTra.C * 0.98M && h.C <= phienKiemTra.C * 1.02M).OrderByDescending(h => h.Date).FirstOrDefault();
            if (comparingHistory != null)
                return comparingHistory.RSI < phienKiemTra.RSI;

            //200 entities
            checkingList = histories.Where(h => h.Date < phienKiemTra.Date).OrderByDescending(h => h.Date).Skip(3).Take(200).ToList();

            comparingHistory = checkingList.Where(h => h.C == phienKiemTra.C).OrderByDescending(h => h.Date).FirstOrDefault();
            if (comparingHistory != null)
                return comparingHistory.RSI < phienKiemTra.RSI;

            comparingHistory = checkingList.Where(h => h.C >= phienKiemTra.C * 0.98M && h.C <= phienKiemTra.C * 1.02M).OrderByDescending(h => h.Date).FirstOrDefault();
            if (comparingHistory != null)
                return comparingHistory.RSI < phienKiemTra.RSI;

            return false;
        }

        public static decimal TỉLệNếnCựcYếu(this HistoryHour phienKiemTra, List<HistoryHour> histories)
        {
            var checkingList = histories.Where(h => h.Date < phienKiemTra.Date).OrderByDescending(h => h.Date).ToList();
            var totalNenYeu = 0; //nến bám bands
            var nenCựcYếu = 0;   //nến dưới bands
            var tongSoNen = 0;
            for (int i = 0; i < checkingList.Count; i++)
            {
                tongSoNen++;
                var ma05 = checkingList[i].MA(histories, -5);


                if (checkingList[i].NenBot < checkingList[i].BandsBot)
                    nenCựcYếu++;
                else if (checkingList[i].NenTop < ma05)
                    totalNenYeu++;

                if (checkingList[i].NenBot > ma05) break;
            }

            return tongSoNen == 0 ? 0 : (decimal)totalNenYeu / (decimal)tongSoNen;
        }

        /// <summary>
        /// Nến cán bands
        /// </summary>
        public static decimal TỉLệNếnYếu(this HistoryHour phienKiemTra, List<HistoryHour> histories)
        {
            var checkingList = histories.Where(h => h.Date < phienKiemTra.Date).OrderByDescending(h => h.Date).ToList();
            var totalNenYeu = 0; //nến cán bands
            var tongSoNen = 0;
            for (int i = 0; i < checkingList.Count; i++)
            {
                tongSoNen++;
                var ma05 = checkingList[i].MA(histories, -5);

                if (checkingList[i].NenBot < checkingList[i].BandsBot
                    || (checkingList[i].NenTop > checkingList[i].BandsBot && checkingList[i].NenBot < checkingList[i].BandsBot)) //nến cán bands
                    totalNenYeu++;

                if (checkingList[i].NenBot > ma05) break;
            }

            return tongSoNen == 0 ? 0 : (decimal)totalNenYeu / (decimal)tongSoNen;
        }

        public static bool MAChuyểnDần(this HistoryHour phienKiemTra, List<HistoryHour> histories, bool chieuKiemTra, int numberOfPreviousPhien, int soPhienKiemTra)
        {
            var checkingList = histories.Where(h => h.Date <= phienKiemTra.Date).OrderByDescending(h => h.Date).Take(soPhienKiemTra + 1).ToList();
            checkingList = checkingList.OrderBy(h => h.Date).ToList();

            var startDate = checkingList[0].Date;

            if (chieuKiemTra) //tăng
            {
                decimal compareValue = checkingList[checkingList.Count - 1].MA(histories, numberOfPreviousPhien);
                for (int i = 0; i < checkingList.Count - 1; i++)
                {
                    var tăng = checkingList[i].MA(histories, numberOfPreviousPhien) - checkingList[i + 1].MA(histories, numberOfPreviousPhien);
                    if (tăng > compareValue) return false;

                    compareValue = tăng;
                }
            }
            else              //giảm
            {
                decimal compareValue = checkingList[0].MA(histories, numberOfPreviousPhien);
                for (int i = 0; i < checkingList.Count - 1; i++)
                {
                    var giảm = checkingList[i].MA(histories, numberOfPreviousPhien) - checkingList[i + 1].MA(histories, numberOfPreviousPhien);
                    if (giảm > compareValue) return false;

                    compareValue = giảm;
                }
            }

            return true;
        }

        public static bool SoSánhGiá(this HistoryHour today, int giáChênhLệch)
        {
            return
                today.TangGia()
                    ? ((decimal)today.C / (decimal)today.O) * 100 > giáChênhLệch
                    : ((decimal)today.O / (decimal)today.C) * 100 > giáChênhLệch;

        }

        public static bool HadBands(this HistoryHour today)
        {
            return today.BandsBot != 0 || today.BandsTop != 0 || today.BandsMid != 0;
        }

        public static bool HadIchimoku(this HistoryHour today)
        {
            return today.IchimokuCloudBot != 0
                || today.IchimokuCloudTop != 0
                || today.IchimokuTenKan != 0
                || today.IchimokuKijun != 0;
        }

        public static bool HadMA5(this HistoryHour today)
        {
            return today.GiaMA05 != 0;
        }

        public static bool HadRsi(this HistoryHour today)
        {
            return today.RSI != 0;
        }

        public static bool HadMACD(this HistoryHour today)
        {
            return today.MACD != 0
                || today.MACDSignal != 0
                || today.MACDMomentum != 0;
        }

        public static bool HadAllIndicators(this HistoryHour today)
        {
            return today.HadMA5() && today.HadBands() && today.HadIchimoku() && today.HadMACD() && today.HadRsi();
        }

        /// <summary>
        /// Phiên hum nay (phiên 1) là nến bao phủ tang so với phiên hum wa (phiên 2) hay ko
        /// </summary>
        /// <param name="phienHumNay"></param>
        /// <param name="phienHumWa"></param>
        /// <returns></returns>
        public static bool IsNenDaoChieuTang(this HistoryHour phienHumNay, HistoryHour phienHumWa, decimal khoangCachChenhLech = 0.5M)
        {
            /*
             * Nến hôm nay đổi ngược màu w nến hôm qua (tăng -> giảm, giảm -> tăng)
             * Đảo chiều tăng: 
             *      + đáy nến hôm nay xuất hiện dưới hoặc bàng đáy nến hôm qua,
             *      + nhưng top nến hum nay thì tăng vượt qua 50% của thân nến hôm qua
             * Đảo chiều giảm: top nến hôm nay xuất hiện trên hoặc bằng top nến hôm qua, nhưng đáy nến hum nay thì giảm vượt qua 50% của thân nến hôm qua
             */

            var dk1 = phienHumNay.TangGia() && !phienHumWa.TangGia();
            var dk2 = phienHumNay.NenBot <= phienHumWa.NenBot;
            var dk3 = phienHumNay.NenTop >= phienHumWa.NenBot + (phienHumWa.NenTop - phienHumWa.NenBot) * khoangCachChenhLech;

            return dk1 && dk2 && dk3;
        }

        public static bool IsNenDaoChieuGiam(this HistoryHour phienHumNay, HistoryHour phienHumWa, decimal khoangCachChenhLech = 0.5M)
        {
            /*
             * Nến hôm nay đổi ngược màu w nến hôm qua (tăng -> giảm, giảm -> tăng)
             * Đảo chiều giảm: 
             *      + top nến hôm nay xuất hiện trên hoặc bằng top nến hôm qua, 
             *      + nhưng đáy nến hum nay thì giảm vượt qua 50% của thân nến hôm qua
             */

            var dk1 = !phienHumNay.TangGia() && phienHumWa.TangGia();
            var dk2 = phienHumNay.NenTop >= phienHumWa.NenTop;
            var dk3 = phienHumNay.NenBot <= phienHumWa.NenTop - (phienHumWa.NenTop - phienHumWa.NenBot) * khoangCachChenhLech;

            //Thân nến 1 dài hơn thân nến 2 > 2 lần và giảm mạnh
            var dk4 = !phienHumNay.TangGia() && (phienHumWa.NenTop - phienHumWa.NenBot) > 0 && (phienHumNay.NenTop - phienHumNay.NenBot) / (phienHumWa.NenTop - phienHumWa.NenBot) >= 2;

            return (dk1 && dk2 && dk3) || dk4;
        }

        public static bool LaCayVuotMA20(this List<HistoryHour> histories, HistoryHour humnay)
        {
            
            //var ngayVuotMA20 = new HistoryHour();

            //if (!humwa.TangGia()) return false;

            if ((humnay.NenTop + humnay.NenBot) / 2 < humnay.BandsMid) return false;

            //if (humwa.NenTop > humwa.BandsMid) return false;

            if (humnay.V < humnay.VOL(histories, -20)) return false;

            if (humnay.C > humnay.BandsMid && humnay.O < humnay.BandsMid) return true;

            var humwa = histories.OrderByDescending(h => h.Date).First(h => h.Date < humnay.Date);
            if (humnay.NenBot > humnay.BandsMid && humwa.NenBot < humwa.BandsMid && humwa.NenTop < humwa.BandsMid)
                return true;

            return false;
        }

        public static bool LaCayVuotMA201(this List<HistoryHour> histories, HistoryHour humnay)
        {
            var humwa = histories.OrderByDescending(h => h.Date).First(h => h.Date < humnay.Date);
            if (humnay.TangGia() && humnay.NenTop > humnay.BandsMid && humwa.NenBot < humwa.BandsMid)
                return true;

            return false;
        }

        public static void TimThoiGianBanTheoT(this List<HistoryHour> histories, List<string> result1, ref decimal dung, ref decimal sai,
            HistoryHour phienHumNay,
            decimal giáĐặtMua,
            int TBan)
        {
            var kiVongLoiToiThieuDeBan = 1.03M;
            var lstNgayCoTheBan = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).Skip(15).Take(TBan - 15).ToList();
            if (!lstNgayCoTheBan.Any())
            {
                //Dữ liệu chưa có đủ cho T3 - phải chờ
                result1.Add($"{phienHumNay.StockSymbol} - ChartH - Mua: {phienHumNay.Date.ToShortDateString()} tại giá {giáĐặtMua} - Chưa đủ dữ liệu T3");
            }
            else
            {
                var note = new StringBuilder();
                //if (phienHumNay.DangTrongMay(histories) || phienHumNay.DangNamDuoiMayFlat(histories))
                //{
                //    note.Append(" - Đang gặp mây xấu.");
                //}
                //if (phienHumNay.CoXuatHienMACDCatXuongSignalTrongXPhienGanNhat(histories, 3))
                //{
                //    note.Append(" - MACD đã cắt xuống Signal.");
                //}

                var cóThểBán = false;
                var sentence = new StringBuilder();
                for (int j = 0; j < lstNgayCoTheBan.Count; j++)
                {
                    var ngayBanGiaDinh = lstNgayCoTheBan[j];

                    /*
                     * Hôm nay là ngày có thể bán, phải cbi kịch bản để bán hoặc giữ tiếp
                     */

                    if (ngayBanGiaDinh.C > giáĐặtMua * kiVongLoiToiThieuDeBan)
                    {
                        var giaBan = ngayBanGiaDinh.C;
                        cóThểBán = true;
                        sentence.Append("Lời");
                        dung++;

                        result1.Add($"{phienHumNay.StockSymbol} - {sentence} - Nhắc mua: {phienHumNay.Date.ToShortDateString()} ở {giáĐặtMua} - Bán - {ngayBanGiaDinh.Date.ToShortDateString()} ở {giaBan} {note}");
                        break;
                    }
                }

                if (!cóThểBán)
                {
                    //result1.Add($"{code} - {stringCTMua} - Nhắc mua: {phienHumNay.Date.ToShortDateString()} tại giá {giáĐặtMua} - Chưa tìm được điểm bán");
                    sai++;
                    sentence.Append("Lỗ");
                    result1.Add($"{phienHumNay.StockSymbol} - {sentence} - Nhắc mua: {phienHumNay.Date.ToShortDateString()} ở {giáĐặtMua} {note}");
                }
            }

        }


        /// <summary>
        /// Kiểm tra phân kỳ của 1 MACD/RSI
        /// true: Phân kỳ dương
        /// false: phân kỳ âm
        /// null: không có phân kỳ
        /// </summary>
        /// <param name="histories"></param>
        /// <param name="đáy2">Ngày hiện tại</param>
        /// <param name="đáy1">Ngày đếm ngược về quá khứ</param>
        /// <param name="propertyPhanky">MACD, RSI</param>
        /// <param name="soPhienDeKiemTraPhanki">Số phiên đếm ngược để kiểm tra phân kì</param>
        /// <returns></returns>
        //public static bool? HasPhanKyDuong(this List<HistoryHour> histories, HistoryHour đáy2, HistoryHour đáy1, string propertyPhanky = "RSI",
        //    int soPhienDeKiemTraPhanki = 60,
        //    decimal CL2G = 0.95M,
        //    decimal CL2D = 0.1M)
        //{
        //    if (đáy2.V < 100000) return null;
        //    var chenhLechĐủ = đáy1.NenBot / đáy2.NenBot >= CL2G; //1.02M; 1/lọc giá trị vol < 100K
        //    if (chenhLechĐủ == false) return null;

        //    var đáy2Minus1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < đáy2.Date);
        //    var đáy1Minus1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < đáy1.Date);
        //    var đáy1Add1 = histories.OrderBy(h => h.Date).First(h => h.Date > đáy1.Date);
        //    var đáy2Add1 = histories.OrderBy(h => h.Date).First(h => h.Date > đáy2.Date);

        //    var propertyValueĐáy2 = (decimal)đáy2.GetPropValue(propertyPhanky);
        //    var propertyValueĐáy1 = (decimal)đáy1.GetPropValue(propertyPhanky);

        //    //if (propertyValueĐáy1 + Math.Abs(propertyValueĐáy1 * CL2D) >= propertyValueĐáy2) return null;
        //    //if (propertyValueĐáy2 < propertyValueĐáy1) return null;

        //    var đáy1Minus1Value = (decimal)đáy1Minus1.GetPropValue(propertyPhanky);
        //    var đáy1Add1Value = (decimal)đáy1Add1.GetPropValue(propertyPhanky);
        //    var đáy2Minus1Value = (decimal)đáy2Minus1.GetPropValue(propertyPhanky);
        //    var đáy2Add1Value = (decimal)đáy2Add1.GetPropValue(propertyPhanky);

        //    var đáy2CenterPoint = new Vector2(0, (float)propertyValueĐáy2);
        //    var điểmTăngNgàyHumnay = new Vector2(1, (float)đáy2Add1Value);

        //    //var deg = điểmTăngNgàyHumnay.GetAngle(đáy2CenterPoint);
        //    //if (deg <= 20) return null;

        //    //Giữa 2 điểm so sánh, ko có 1 điểm nào bé hơn điểm đang xét cả
        //    var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > đáy1.Date && h.Date < đáy2.Date).ToList();
        //    if (middlePoints.Count < 2) return null; //ở giữa ít nhất 2 điểm - TODO: luôn luôn true vì mình đã skip 3

        //    var propertyInfo = typeof(HistoryHour).GetProperty(propertyPhanky);
        //    var tcr = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).First();
        //    var tcr2 = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).Last();

        //    if (middlePoints.Any(x => (decimal)propertyInfo.GetValue(x, null) <= propertyValueĐáy1)) return null;

        //    //Tất cả middle points ko có point nào dc phép nằm dưới đường thằng nối từ đáy 1 tới đáy 2
        //    var indexDay1 = histories.IndexOf(đáy2);
        //    var indexDay2 = histories.IndexOf(đáy1);
        //    var rangeFromDay1ToiDay2 = Math.Abs(indexDay1 - indexDay2);
        //    var averageNumberEachDay = (propertyValueĐáy2 - propertyValueĐáy1) / rangeFromDay1ToiDay2;
        //    var coPointDeuBenDuoiLine = false;
        //    for (int i = 1; i < rangeFromDay1ToiDay2; i++)
        //    {
        //        var checkingDayPropertyValue = (decimal)middlePoints[i - 1].GetPropValue(propertyPhanky);
        //        var comparedValue = propertyValueĐáy1 + averageNumberEachDay * i;
        //        if (comparedValue > checkingDayPropertyValue)
        //        {
        //            coPointDeuBenDuoiLine = true;
        //            break;
        //        }
        //    }

        //    if (coPointDeuBenDuoiLine) return null;


        //    /*
        //     * Xác định đáy nói chung
        //     * từ đáy đi lên bên phải và trái, giá trị phải tăng liên tục, tăng tối thiếu 10% của giá trị đáy trước khi bị giảm hoặc đi ngang 
        //     *
        //     * Xác định đáy 2
        //     * đường thẳng nối từ đáy 1 tới đáy 2 kéo dài ra, nếu property (RSI/MACD) của ngày sau ngày tạo đáy nằm trên đường thẳng này, chứng tỏ đáy 2 đã được tạo thành công; ngược lại đây chưa phải đáy 2
        //     */
        //    var đãTạoĐáy2 = histories.PropertyTangDanToiKhiDatTargetTrai(đáy2, propertyPhanky, 0.9M);
        //    var đãTạoĐáy1 = histories.PropertyTangDanToiKhiDatTargetTrai(đáy1, propertyPhanky, 0.9M);
        //    if (!đãTạoĐáy1 || đáy1Add1Value <= propertyValueĐáy1) return null;
        //    //if (!đãTạoĐáy2 || đáy2Add1Value <= propertyValueĐáy2) return null;//TODO: điều kiện cần thêm
        //    if (!đãTạoĐáy2 || đáy2Add1Value <= propertyValueĐáy2 + averageNumberEachDay) return null;


        //    var trendLineRsi = new Line();
        //    trendLineRsi.x1 = 0;  //x là trục tung - trục đối xứng
        //    trendLineRsi.y1 = propertyValueĐáy1;   //
        //    trendLineRsi.x2 = middlePoints.Count() + 2; //+ 2 vì tính từ ngày kiểm tra và ngày so sánh
        //    trendLineRsi.y2 = propertyValueĐáy2;

        //    var crossLineRsi = new Line();
        //    var cr1 = 1;
        //    while (cr1 < middlePoints.Count())
        //    {
        //        if (đáy1.Date.AddDays(cr1) == tcr.Date)
        //            break;
        //        cr1++;
        //    }
        //    crossLineRsi.x1 = cr1;
        //    crossLineRsi.y1 = (decimal)tcr.GetPropValue(propertyPhanky); //tcr2.RSI;//tcr.RSI;

        //    var cr2 = 1;
        //    while (cr2 < middlePoints.Count())
        //    {
        //        if (đáy1.Date.AddDays(cr2) == tcr2.Date)
        //            break;
        //        cr2++;
        //    }
        //    crossLineRsi.x2 = cr2;//(decimal)((middlePoints.OrderByDescending(h => h.RSI).Last().Date - ngaySoSanhVoihistory1.Date).TotalDays);
        //    crossLineRsi.y2 = (decimal)tcr2.GetPropValue(propertyPhanky); //tcr2.RSI;

        //    //var trendLineGia = new Line();
        //    //trendLineGia.x1 = 0;  //x là trục tung - trục đối xứng
        //    //trendLineGia.y1 = dayInThePast.NenBot;   //
        //    //trendLineGia.x2 = middlePoints.Count() + 2;//(decimal)((history1.Date - ngaySoSanhVoihistory1.Date).TotalDays);
        //    //trendLineGia.y2 = today.NenBot;
        //    //var crossLineGia = new Line();
        //    //var point1 = middlePoints.OrderByDescending(h => h.NenBot).First();
        //    //var point2 = middlePoints.OrderByDescending(h => h.NenBot).Last();

        //    //var cg1 = 1;
        //    //while (cg1 < middlePoints.Count())
        //    //{
        //    //    if (dayInThePast.Date.AddDays(cg1) == point1.Date)
        //    //        break;
        //    //    cg1++;
        //    //}

        //    //var cg2 = 1;
        //    //while (cg2 < middlePoints.Count())
        //    //{
        //    //    if (dayInThePast.Date.AddDays(cg2) == point2.Date)
        //    //        break;
        //    //    cg2++;
        //    //}

        //    //crossLineGia.x1 = cg1;
        //    //crossLineGia.y1 = point1.NenBot;
        //    //crossLineGia.x2 = cg2;
        //    //crossLineGia.y2 = point2.NenBot;

        //    var pointRsi = trendLineRsi.FindIntersection(crossLineRsi);
        //    //var pointGia = trendLineGia.FindIntersection(crossLineGia);

        //    if (pointRsi == null)
        //    {
        //        return true;
        //    }

        //    return null;
        //}


        /// <summary>
        /// Trong vòng XX ngày, có bất kì ngày nào là đáy 2 chưa
        /// </summary>
        /// <param name="histories"></param>
        /// <param name="đáy1"></param>
        /// <param name="property"></param>
        /// <param name="soPhienDeKiemTra2Day"></param>
        /// <returns></returns>
        //public static bool CoTao2DayChua(this List<HistoryHour> histories, HistoryHour đáy2, string property = "RSI", int soPhienDeKiemTra2Day = 30)
        //{
        //    //[Ngày hiện tại] phải là ngày nhỏ hơn ngày thực tế ít nhất 1
        //    /* Nếu tìm thấy đáy 1 nữa là xong */
        //    //if (đáy2.V < 100000) return null;
        //    //var chenhLechĐủ = đáy1.NenBot / đáy2.NenBot >= CL2G; //1.02M; 1/lọc giá trị vol < 100K
        //    //if (chenhLechĐủ == false) return null;

        //    var ngayThucTeXacNhanDay2 = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date > đáy2.Date);
        //    if (ngayThucTeXacNhanDay2 == null) return false;

        //    var propertyNgayXacNhanĐáy2 = (decimal)ngayThucTeXacNhanDay2.GetPropValue(property);
        //    var propertyĐáy2 = (decimal)đáy2.GetPropValue(property);
        //    if (propertyNgayXacNhanĐáy2 <= propertyĐáy2 || propertyĐáy2 == 0) return false;


        //    var nhungNgaySoSanhVoiDayGiaDinh = histories.OrderByDescending(h => h.Date).Where(h => h.Date < đáy2.Date).Take(soPhienDeKiemTra2Day).ToList();
        //    if (nhungNgaySoSanhVoiDayGiaDinh.Count < soPhienDeKiemTra2Day) return false;

        //    for (int j = 1; j < nhungNgaySoSanhVoiDayGiaDinh.Count - 1; j++)
        //    {
        //        var đáy1 = nhungNgaySoSanhVoiDayGiaDinh[j];

        //        var đáy2Minus1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < đáy2.Date);
        //        var đáy1Minus1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < đáy1.Date);
        //        var đáy1Add1 = histories.OrderBy(h => h.Date).First(h => h.Date > đáy1.Date);
        //        var đáy2Add1 = histories.OrderBy(h => h.Date).First(h => h.Date > đáy2.Date);


        //        var propertyValueĐáy1 = (decimal)đáy1.GetPropValue(property);

        //        var đáy1Minus1Value = (decimal)đáy1Minus1.GetPropValue(property);
        //        var đáy1Add1Value = (decimal)đáy1Add1.GetPropValue(property);
        //        var đáy2Minus1Value = (decimal)đáy2Minus1.GetPropValue(property);
        //        var đáy2Add1Value = (decimal)đáy2Add1.GetPropValue(property);

        //        //Giữa 2 điểm so sánh, ko có 1 điểm nào bé hơn điểm đang xét cả
        //        var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > đáy1.Date && h.Date < đáy2.Date).ToList();
        //        if (middlePoints.Count < 2) continue;

        //        var propertyInfo = typeof(HistoryHour).GetProperty(property);
        //        var tcr = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).First();
        //        var tcr2 = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).Last();

        //        if (middlePoints.Any(x => (decimal)propertyInfo.GetValue(x, null) <= propertyValueĐáy1)) continue;

        //        //Tất cả middle points ko có point nào dc phép nằm dưới đường thằng nối từ đáy 1 tới đáy 2
        //        var indexDay1 = histories.IndexOf(đáy2);
        //        var indexDay2 = histories.IndexOf(đáy1);
        //        var rangeFromDay1ToiDay2 = Math.Abs(indexDay1 - indexDay2);
        //        var averageNumberEachDay = (propertyĐáy2 - propertyValueĐáy1) / rangeFromDay1ToiDay2;
        //        var coPointDeuBenDuoiLine = false;
        //        for (int i = 1; i < rangeFromDay1ToiDay2; i++)
        //        {
        //            var checkingDayPropertyValue = (decimal)middlePoints[i - 1].GetPropValue(property);
        //            var comparedValue = propertyValueĐáy1 + averageNumberEachDay * i;
        //            if (comparedValue > checkingDayPropertyValue)
        //            {
        //                coPointDeuBenDuoiLine = true;
        //                break;
        //            }
        //        }

        //        if (coPointDeuBenDuoiLine) continue;


        //        /*
        //         * Xác định đáy nói chung
        //         * từ đáy đi lên bên phải và trái, giá trị phải tăng liên tục, tăng tối thiếu 10% của giá trị đáy trước khi bị giảm hoặc đi ngang 
        //         *
        //         * Xác định đáy 2
        //         * đường thẳng nối từ đáy 1 tới đáy 2 kéo dài ra, nếu property (RSI/MACD) của ngày sau ngày tạo đáy nằm trên đường thẳng này, chứng tỏ đáy 2 đã được tạo thành công; ngược lại đây chưa phải đáy 2
        //         */
        //        var đãTạoĐáy2 = histories.PropertyTangDanToiKhiDatTargetTrai(đáy2, property, 0.9999M);
        //        var đãTạoĐáy1 = histories.PropertyTangDanToiKhiDatTargetTrai(đáy1, property, 0.9999M);
        //        if (!đãTạoĐáy1 || đáy1Add1Value <= propertyValueĐáy1) continue;
        //        //if (!đãTạoĐáy2 || đáy2Add1Value <= propertyValueĐáy2) return null;//TODO: điều kiện cần thêm
        //        if (!đãTạoĐáy2 || đáy2Add1Value <= propertyĐáy2 + averageNumberEachDay) continue;


        //        var trendLineRsi = new Line();
        //        trendLineRsi.x1 = 0;  //x là trục tung - trục đối xứng
        //        trendLineRsi.y1 = propertyValueĐáy1;   //
        //        trendLineRsi.x2 = middlePoints.Count() + 2; //+ 2 vì tính từ ngày kiểm tra và ngày so sánh
        //        trendLineRsi.y2 = propertyĐáy2;

        //        var crossLineRsi = new Line();
        //        var cr1 = 1;
        //        while (cr1 < middlePoints.Count())
        //        {
        //            if (đáy1.Date.AddDays(cr1) == tcr.Date)
        //                break;
        //            cr1++;
        //        }
        //        crossLineRsi.x1 = cr1;
        //        crossLineRsi.y1 = (decimal)tcr.GetPropValue(property); //tcr2.RSI;//tcr.RSI;

        //        var cr2 = 1;
        //        while (cr2 < middlePoints.Count())
        //        {
        //            if (đáy1.Date.AddDays(cr2) == tcr2.Date)
        //                break;
        //            cr2++;
        //        }
        //        crossLineRsi.x2 = cr2;
        //        crossLineRsi.y2 = (decimal)tcr2.GetPropValue(property); //tcr2.RSI;

        //        var pointRsi = trendLineRsi.FindIntersection(crossLineRsi);

        //        if (pointRsi == null)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }

}
