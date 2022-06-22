using DotNetCoreSqlDb.Common.ArrayExtensions;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetCoreSqlDb.Common
{
    public static class BusinessModellExtension
    {
        public static bool IsBigger(this PatternWeekResearchModel checkingWeek, PatternWeekResearchModel compareWeek)
        {


            return false;
        }

        public static bool VOLBienDong(this History checkingDate, List<History> histories, int numberOfPreviousPhien, decimal bienDong)
        {
            if (numberOfPreviousPhien == 0) return false;

            var checkingRange = new List<History>();
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

        public static decimal VOL(this History checkingDate, List<History> histories, int numberOfPreviousPhien)
        {
            if (numberOfPreviousPhien == 0) return 0;

            var checkingRange = new List<History>();
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

        public static decimal MA(this History checkingDate, List<History> histories, int numberOfPreviousPhien)
        {
            if (numberOfPreviousPhien == 0) return 0;

            var checkingRange = new List<History>();
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

        public static decimal MA(this PatternWeekResearchModel checkingDate, List<PatternWeekResearchModel> histories, int numberOfPreviousPhien)
        {
            if (numberOfPreviousPhien == 0) return 0;

            var checkingRange = new List<PatternWeekResearchModel>();
            if (numberOfPreviousPhien < 0)
            {
                numberOfPreviousPhien = numberOfPreviousPhien * -1;
                checkingRange = histories.Where(h => h.Date < checkingDate.Date).OrderByDescending(h => h.Date).Take(numberOfPreviousPhien).ToList();
            }
            else
                checkingRange = histories.Where(h => h.Date > checkingDate.Date).OrderBy(h => h.Date).Take(numberOfPreviousPhien).ToList();

            if (checkingRange.Count != numberOfPreviousPhien) return 0;

            return checkingRange.Sum(h => h.C) / numberOfPreviousPhien;
        }

        public static bool IsPriceUp(this History today, List<History> histories, int numberOfSideway)
        {
            var todayVol = today.C;
            var averageOfVolInSideway = today.MAPrice(histories, numberOfSideway);

            return todayVol > averageOfVolInSideway * 1.05M;
        }

        public static bool IsPriceDown(this History today, List<History> histories, int numberOfSideway)
        {
            var todayVol = today.C;
            var averageOfVolInSideway = today.MAPrice(histories, numberOfSideway);

            return todayVol * 1.05M < averageOfVolInSideway;
        }


        public static bool IsVolUp(this History today, List<History> histories, int numberOfSideway)
        {
            var todayVol = today.V;
            var averageOfVolInSideway = today.MASideway(histories, numberOfSideway);

            return todayVol > averageOfVolInSideway * 1.05M;
        }

        public static bool IsVolDown(this History today, List<History> histories, int numberOfSideway)
        {
            var todayVol = today.V;
            var averageOfVolInSideway = today.MASideway(histories, numberOfSideway);

            return todayVol * 1.05M < averageOfVolInSideway;
        }

        public static decimal MASideway(this History today, List<History> histories, int numberOfSideway)
        {
            return histories.Where(h => h.Date < today.Date).OrderByDescending(h => h.Date).Take(numberOfSideway).Sum(h => h.V) / numberOfSideway;
        }

        public static decimal MAPrice(this History today, List<History> histories, int numberOfSideway)
        {
            return histories.Where(h => h.Date < today.Date).OrderByDescending(h => h.Date).Take(numberOfSideway).Sum(h => h.C) / numberOfSideway;
        }

        public static bool DangBiCanhCaoGD1Tuan(this History checkingDate, List<History> histories)
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
        public static decimal LayGiaCuaPhienSau(this History checkingDate, List<History> histories, int T)
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
        public static decimal LayGiaCaoNhatCuaCacPhienSau(this History checkingDate, List<History> histories, int fromT, int toT)
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

        public static History LookingForLowestWithout2Percent(this History currentDateHistory, List<History> histories)//, History currentDateHistory)
        {
            if (currentDateHistory == null) return null;

            var history = histories.FirstOrDefault(h => h.Date == currentDateHistory.Date);
            if (history == null) return null;

            var currentDateToCheck = history.Date;
            var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(30).ToList();

            var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();

            return lowest;
        }

        public static History LookingForSecondLowestWithout2Percent(this History lowest, List<History> histories, History currentDateHistory)
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

        public static History LookingForSecondLowest(this History lowest, List<History> histories, History currentDateHistory, bool included2PercentHigher = false)
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

        public static History LookingForSecondLowestWithCheckingDate(this History lowest, List<History> histories, History currentDateHistory, bool included2PercentHigher = false)
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

        public static History LookingForSecondLowestWithoutLowest(List<History> histories, History currentDateHistory, bool included2PercentHigher = false)
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

        public static History LookingForLowest(this List<History> histories, History currentDateHistory)
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

        public static bool DidDay2ShowYesterday(this List<History> histories, History currentDateHistory, out History dinh1, out History day1, out History day2)
        {
            var h1 = histories.Where(h => h.Date < currentDateHistory.Date).OrderByDescending(h => h.Date).ToList();
            day1 = new History();
            day2 = new History();
            dinh1 = new History();

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


        public static bool DidDay2ShowYesterdayStartWithDay2(this List<History> histories, History currentDateHistory, out History dinh1, out History day1, out History day2)
        {
            //var h1 = histories.Where(h => h.Date < currentDateHistory.Date).OrderByDescending(h => h.Date).ToList();

            day1 = new History();
            day2 = new History();
            dinh1 = new History();

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


        public static decimal RSI(this History history, List<History> histories, int rsi)
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

        public static Tuple<decimal, decimal, decimal> RSIDetail(this History history, List<History> histories, int rsi)
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

        public static List<StockSymbolFinanceHistory> Filter(this List<StockSymbolFinanceHistory> stockSymbolFinanceHistories, int year, int quarter)
        {
            var currentIndex = ConstantData.TimeQuarter.LstOfQuarters.FirstOrDefault(i => i.Item2 == quarter && i.Item3 == year)?.Item1 ?? 0;
            List<StockSymbolFinanceHistory> result = new List<StockSymbolFinanceHistory>();
            stockSymbolFinanceHistories = stockSymbolFinanceHistories.Where(r => r.YearPeriod >= year).ToList();

            foreach (var item in stockSymbolFinanceHistories)
            {
                var index = ConstantData.TimeQuarter.LstOfQuarters.FirstOrDefault(i => i.Item2 == item.Quarter && i.Item3 == item.YearPeriod)?.Item1 ?? 0;
                if (index == currentIndex)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public static List<decimal> MACD(this History history, List<History> histories, int rsi)
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

        public static decimal SMA(this History history, List<History> histories, int rsi)
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

        public static bool TangGia(this History today)
        {
            return today.C > today.O;
        }

        public static bool Doji(this History today)
        {
            return today.C.IsDifferenceInRank(today.O, 0.01M);
        }

        /// <summary>
        /// TODO: Kháng cự có thể là 1 biên rộng, nên 2 giá trị có thể là hợp lí
        /// </summary>
        /// <param name="today"></param>
        /// <param name="histories"></param>
        /// <returns></returns>
        public static decimal KhángCựĐỉnh(this History today, List<History> histories)
        {
            return 0;
        }

        /// <summary>
        /// TODO: Kháng cự có thể là 1 biên rộng, nên 2 giá trị có thể là hợp lí
        /// </summary>
        /// <param name="today"></param>
        /// <param name="histories"></param>
        /// <returns></returns>
        public static decimal KhángCựBands(this History today, List<History> histories)
        {
            return 0;
        }

        ////public static void AddBollingerBands(ref SortedList<DateTime, Dictionary<string, double>> data, int period, int factor)
        //public static void AddBollingerBands(this List<History> histories, int period, int factor)
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
        public static decimal KhángCựFibonacci(this History today, List<History> histories)
        {
            return 0;
        }

        /// <summary>
        /// TODO: Kháng cự có thể là 1 biên rộng, nên 2 giá trị có thể là hợp lí
        /// </summary>
        /// <param name="today"></param>
        /// <param name="histories"></param>
        /// <returns></returns>
        public static decimal KhángCựIchimoku(this History today, List<History> histories)
        {
            return 0;
        }

        public static bool NếnĐảoChiều(this History phienKiemTra)
        {
            var nếnĐảoChiều = phienKiemTra.NenTop < phienKiemTra.BandsBot
                                    && (phienKiemTra.H - phienKiemTra.NenTop) / 2 > (phienKiemTra.NenTop - phienKiemTra.NenBot);
            //var bắtĐầuMuaDoNếnTăngA1 = nếnĐảoChiều && phienKiemTra.TangGia() ? true : false;

            return nếnĐảoChiều;
        }

        public static bool NếnĐảoChiềuTăngMạnhA1(this History phienKiemTra)
        {
            return phienKiemTra.NếnĐảoChiều() && phienKiemTra.TangGia() ? true : false;
        }

        public static bool NếnĐảoChiềuTăngMạnhA2(this History phienKiemTra, History phiênTrướcPhiênKiemTra)
        {
            return phiênTrướcPhiênKiemTra.NếnĐảoChiều() && !phiênTrướcPhiênKiemTra.TangGia() && phienKiemTra.TangGia() ? true : false;
        }


        /// <summary>
        /// Đơn giản là so sánh về quá khứ lấy 1 cây cùng giá đóng cửa, thấy RSI cây hiện tại cao hơn thì dương, còn ko thì âm
        /// </summary>
        /// <param name="phienKiemTra"></param>
        /// <param name="histories"></param>
        /// <returns></returns>
        public static bool RSIDương(this History phienKiemTra, List<History> histories)
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


        public static decimal TỉLệNếnCựcYếu(this History phienKiemTra, List<History> histories)
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
        public static decimal TỉLệNếnYếu(this History phienKiemTra, List<History> histories)
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


        public static bool MAChuyểnDần(this History phienKiemTra, List<History> histories, bool chieuKiemTra, int numberOfPreviousPhien, int soPhienKiemTra)
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

        public static bool SoSánhGiá(this History today, int giáChênhLệch)
        {
            return
                today.TangGia()
                    ? ((decimal)today.C / (decimal)today.O) * 100 > giáChênhLệch
                    : ((decimal)today.O / (decimal)today.C) * 100 > giáChênhLệch;

        }


        public static bool HadBands(this History today)
        {
            return today.BandsBot != 0 || today.BandsTop != 0 || today.BandsMid != 0;
        }

        public static bool HadIchimoku(this History today)
        {
            return today.IchimokuCloudBot != 0
                || today.IchimokuCloudTop != 0
                || today.IchimokuTenKan != 0
                || today.IchimokuKijun != 0;
        }

        public static bool HadMA5(this History today)
        {
            return today.GiaMA05 != 0;
        }

        public static bool HadRsi(this History today)
        {
            return today.RSI != 0;
        }

        public static bool HadMACD(this History today)
        {
            return today.MACD != 0
                || today.MACDSignal != 0
                || today.MACDMomentum != 0;
        }

        public static bool HadAllIndicators(this History today)
        {
            return today.HadMA5() && today.HadBands() && today.HadIchimoku() && today.HadMACD() && today.HadRsi();
        }

        /// <summary>
        /// Phiên hum nay (phiên 1) là nến bao phủ so với phiên hum wa (phiên 2) hay ko
        /// </summary>
        /// <param name="phien1"></param>
        /// <param name="phien2"></param>
        /// <returns></returns>
        public static bool IsNenBaoPhu(this History phien1, History phien2)
        {
            return phien1.NenTop - phien1.NenBot > phien2.NenTop - phien2.NenBot &&
                ((phien1.NenTop >= phien2.NenTop && phien1.NenBot <= phien2.NenBot + (phien2.NenTop - phien2.NenBot) * 0.75M)
                || (phien1.NenBot <= phien2.NenBot && phien1.NenTop >= phien2.NenBot + (phien2.NenTop - phien2.NenBot) * 0.75M));
        }
    }

}
