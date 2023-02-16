using DotNetCoreSqlDb.Common.ArrayExtensions;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        public static decimal MA(this History checkingDate, List<History> histories, int numberOfPreviousPhien, string propertyName)
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

            var propertyInfo = typeof(History).GetProperty(propertyName);
            var sumValue = checkingRange.Sum(x => (decimal)propertyInfo.GetValue(x, null));
            return sumValue / numberOfPreviousPhien;
            //return checkingRange.Sum(h => h.V) / numberOfPreviousPhien;
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

        public static bool GiamGia(this History today)
        {
            return today.C < today.O;
        }

        public static bool GiamGia(this HistoryHour today)
        {
            return today.C < today.O;
        }

        public static bool DojiCoRauHoacTangGia(this History today)
        {
            return today.C >= today.O && today.H > today.C;
        }

        /// <summary>
        /// Đỉnh sau thấp hơn đỉnh trước
        /// Đáy sau thấp hơn đáy trước
        /// </summary>
        /// <param name="today"></param>
        /// <returns></returns>
        public static bool NếnGiảm(this List<History> histories, History history, int trongNPhien)
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
        /// Đơn giản là so sánh về quá khứ trong vòng 52 phiên, lấy 1 cây cùng giá đóng cửa, 
        /// Xét thấy RSI cây hiện tại cao hơn thì dương, còn ko thì âm
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
        /// Phiên hum nay (phiên 1) là nến bao phủ tang so với phiên hum wa (phiên 2) hay ko
        /// </summary>
        /// <param name="phienHumNay"></param>
        /// <param name="phienHumWa"></param>
        /// <returns></returns>
        public static bool IsNenDaoChieuTang(this History phienHumNay, History phienHumWa, decimal khoangCachChenhLech = 0.5M)
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

        public static bool IsNenDaoChieuGiam(this History phienHumNay, History phienHumWa, decimal khoangCachChenhLech = 0.5M)
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

        public static float GetAngle(this Vector2 point, Vector2 center)
        {
            Vector2 relPoint = point - center;
            return (ToDegrees(MathF.Atan2(relPoint.Y, relPoint.X)) + 450f) % 360f;
        }

        public static float ToDegrees(float radians) => radians * 180f / MathF.PI;


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
        public static bool? HasPhanKyDuong(this List<History> histories, History đáy2, History đáy1, string propertyPhanky = "RSI",
            int soPhienDeKiemTraPhanki = 60,
            decimal CL2G = 0.95M,
            decimal CL2D = 0.1M)
        {
            if (đáy2.VOL(histories, -20) < ConstantData.minMA20VolDaily) return null;
            var chenhLechĐủ = đáy1.NenBot / đáy2.NenBot >= CL2G; //1.02M; 1/lọc giá trị vol < 100K
            if (chenhLechĐủ == false) return null;

            var đáy2Minus1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < đáy2.Date);
            var đáy1Minus1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < đáy1.Date);
            var đáy1Add1 = histories.OrderBy(h => h.Date).First(h => h.Date > đáy1.Date);
            var đáy2Add1 = histories.OrderBy(h => h.Date).First(h => h.Date > đáy2.Date);

            var propertyValueĐáy2 = (decimal)đáy2.GetPropValue(propertyPhanky);
            var propertyValueĐáy1 = (decimal)đáy1.GetPropValue(propertyPhanky);

            //if (propertyValueĐáy1 + Math.Abs(propertyValueĐáy1 * CL2D) >= propertyValueĐáy2) return null;
            if (propertyValueĐáy2 < propertyValueĐáy1) return null;                                       //PVS 13/4/21 - 22/4/21

            var đáy1Minus1Value = (decimal)đáy1Minus1.GetPropValue(propertyPhanky);
            var đáy1Add1Value = (decimal)đáy1Add1.GetPropValue(propertyPhanky);
            var đáy2Minus1Value = (decimal)đáy2Minus1.GetPropValue(propertyPhanky);
            var đáy2Add1Value = (decimal)đáy2Add1.GetPropValue(propertyPhanky);

            var đáy2CenterPoint = new Vector2(0, (float)propertyValueĐáy2);
            var điểmTăngNgàyHumnay = new Vector2(1, (float)đáy2Add1Value);

            //var deg = điểmTăngNgàyHumnay.GetAngle(đáy2CenterPoint);
            //if (deg <= 20) return null;

            //Giữa 2 điểm so sánh, ko có 1 điểm nào bé hơn điểm đang xét cả
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > đáy1.Date && h.Date < đáy2.Date).ToList();
            if (middlePoints.Count < 2) return null; //ở giữa ít nhất 2 điểm - TODO: luôn luôn true vì mình đã skip 3

            var propertyInfo = typeof(History).GetProperty(propertyPhanky);
            var tcr = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).First();
            var tcr2 = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).Last();

            if (middlePoints.Any(x => (decimal)propertyInfo.GetValue(x, null) <= propertyValueĐáy1)) return null;

            //Tất cả middle points ko có point nào dc phép nằm dưới đường thằng nối từ đáy 1 tới đáy 2
            var indexDay1 = histories.IndexOf(đáy2);
            var indexDay2 = histories.IndexOf(đáy1);
            var rangeFromDay1ToiDay2 = Math.Abs(indexDay1 - indexDay2);
            var averageNumberEachDay = (propertyValueĐáy2 - propertyValueĐáy1) / rangeFromDay1ToiDay2;
            var coPointDeuBenDuoiLine = false;
            for (int i = 1; i < rangeFromDay1ToiDay2; i++)
            {
                var checkingDayPropertyValue = (decimal)middlePoints[i - 1].GetPropValue(propertyPhanky);
                var comparedValue = propertyValueĐáy1 + averageNumberEachDay * i;
                if (comparedValue > checkingDayPropertyValue)
                {
                    coPointDeuBenDuoiLine = true;
                    break;
                }
            }

            //if (coPointDeuBenDuoiLine) return null;   //TODO: tạm thời bỏ qua dk này


            /*
             * Xác định đáy nói chung
             * từ đáy đi lên bên phải và trái, giá trị phải tăng liên tục, tăng tối thiếu 10% của giá trị đáy trước khi bị giảm hoặc đi ngang 
             *
             * Xác định đáy 2
             * đường thẳng nối từ đáy 1 tới đáy 2 kéo dài ra, nếu property (RSI/MACD) của ngày sau ngày tạo đáy nằm trên đường thẳng này, chứng tỏ đáy 2 đã được tạo thành công; ngược lại đây chưa phải đáy 2
             */
            var đãTạoĐáy2 = histories.PropertyTangDanToiKhiDatTargetTrai(đáy2, propertyPhanky, 0.9M);
            var đãTạoĐáy1 = histories.PropertyTangDanToiKhiDatTargetTrai(đáy1, propertyPhanky, 0.9M);
            if (!đãTạoĐáy1 || đáy1Add1Value <= propertyValueĐáy1) return null;
            //if (!đãTạoĐáy2 || đáy2Add1Value <= propertyValueĐáy2) return null;//TODO: điều kiện cần thêm
            if (!đãTạoĐáy2 || đáy2Add1Value <= propertyValueĐáy2 + averageNumberEachDay) return null;


            var trendLineRsi = new Line();
            trendLineRsi.x1 = 0;  //x là trục tung - trục đối xứng
            trendLineRsi.y1 = propertyValueĐáy1;   //
            trendLineRsi.x2 = middlePoints.Count() + 2; //+ 2 vì tính từ ngày kiểm tra và ngày so sánh
            trendLineRsi.y2 = propertyValueĐáy2;

            var crossLineRsi = new Line();
            var cr1 = 1;
            while (cr1 < middlePoints.Count())
            {
                if (đáy1.Date.AddDays(cr1) == tcr.Date)
                    break;
                cr1++;
            }
            crossLineRsi.x1 = cr1;
            crossLineRsi.y1 = (decimal)tcr.GetPropValue(propertyPhanky); //tcr2.RSI;//tcr.RSI;

            var cr2 = 1;
            while (cr2 < middlePoints.Count())
            {
                if (đáy1.Date.AddDays(cr2) == tcr2.Date)
                    break;
                cr2++;
            }
            crossLineRsi.x2 = cr2;//(decimal)((middlePoints.OrderByDescending(h => h.RSI).Last().Date - ngaySoSanhVoihistory1.Date).TotalDays);
            crossLineRsi.y2 = (decimal)tcr2.GetPropValue(propertyPhanky); //tcr2.RSI;

            //var trendLineGia = new Line();
            //trendLineGia.x1 = 0;  //x là trục tung - trục đối xứng
            //trendLineGia.y1 = dayInThePast.NenBot;   //
            //trendLineGia.x2 = middlePoints.Count() + 2;//(decimal)((history1.Date - ngaySoSanhVoihistory1.Date).TotalDays);
            //trendLineGia.y2 = today.NenBot;
            //var crossLineGia = new Line();
            //var point1 = middlePoints.OrderByDescending(h => h.NenBot).First();
            //var point2 = middlePoints.OrderByDescending(h => h.NenBot).Last();

            //var cg1 = 1;
            //while (cg1 < middlePoints.Count())
            //{
            //    if (dayInThePast.Date.AddDays(cg1) == point1.Date)
            //        break;
            //    cg1++;
            //}

            //var cg2 = 1;
            //while (cg2 < middlePoints.Count())
            //{
            //    if (dayInThePast.Date.AddDays(cg2) == point2.Date)
            //        break;
            //    cg2++;
            //}

            //crossLineGia.x1 = cg1;
            //crossLineGia.y1 = point1.NenBot;
            //crossLineGia.x2 = cg2;
            //crossLineGia.y2 = point2.NenBot;

            var pointRsi = trendLineRsi.FindIntersection(crossLineRsi);
            //var pointGia = trendLineGia.FindIntersection(crossLineGia);

            if (pointRsi == null)
            {
                return true;
            }

            return null;
        }


        /// <summary>
        /// Trong vòng XX ngày, có bất kì ngày nào là đáy 2 chưa
        /// </summary>
        /// <param name="histories"></param>
        /// <param name="đáy1"></param>
        /// <param name="property"></param>
        /// <param name="soPhienDeKiemTra2Day"></param>
        /// <returns></returns>
        public static bool CoTao2DayChua(this List<History> histories, History đáy2, string property = "RSI", int soPhienDeKiemTra2Day = 60)
        {
            //[Ngày hiện tại] phải là ngày nhỏ hơn ngày thực tế ít nhất 1
            /* Nếu tìm thấy đáy 1 nữa là xong */
            //if (đáy2.V < 100000) return null;
            //var chenhLechĐủ = đáy1.NenBot / đáy2.NenBot >= CL2G; //1.02M; 1/lọc giá trị vol < 100K
            //if (chenhLechĐủ == false) return null;

            var ngayThucTeXacNhanDay2 = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date > đáy2.Date);
            if (ngayThucTeXacNhanDay2 == null) return false;

            var propertyNgayXacNhanĐáy2 = (decimal)ngayThucTeXacNhanDay2.GetPropValue(property);
            var propertyĐáy2 = (decimal)đáy2.GetPropValue(property);
            if (propertyNgayXacNhanĐáy2 <= propertyĐáy2 || propertyĐáy2 == 0) return false;


            var nhungNgaySoSanhVoiDayGiaDinh = histories.OrderByDescending(h => h.Date).Where(h => h.Date < đáy2.Date).Take(soPhienDeKiemTra2Day).ToList();
            if (nhungNgaySoSanhVoiDayGiaDinh.Count < soPhienDeKiemTra2Day) return false;

            for (int j = 1; j < nhungNgaySoSanhVoiDayGiaDinh.Count - 1; j++)
            {
                var đáy1 = nhungNgaySoSanhVoiDayGiaDinh[j];

                var đáy2Minus1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < đáy2.Date);
                var đáy1Minus1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < đáy1.Date);
                var đáy1Add1 = histories.OrderBy(h => h.Date).First(h => h.Date > đáy1.Date);
                var đáy2Add1 = histories.OrderBy(h => h.Date).First(h => h.Date > đáy2.Date);


                var propertyValueĐáy1 = (decimal)đáy1.GetPropValue(property);

                var đáy1Minus1Value = (decimal)đáy1Minus1.GetPropValue(property);
                var đáy1Add1Value = (decimal)đáy1Add1.GetPropValue(property);
                var đáy2Minus1Value = (decimal)đáy2Minus1.GetPropValue(property);
                var đáy2Add1Value = (decimal)đáy2Add1.GetPropValue(property);

                //Giữa 2 điểm so sánh, ko có 1 điểm nào bé hơn điểm đang xét cả
                var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > đáy1.Date && h.Date < đáy2.Date).ToList();
                if (middlePoints.Count < 2) continue;

                var propertyInfo = typeof(History).GetProperty(property);
                var tcr = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).First();
                var tcr2 = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).Last();

                if (middlePoints.Any(x => (decimal)propertyInfo.GetValue(x, null) <= propertyValueĐáy1)) continue;

                //Tất cả middle points ko có point nào dc phép nằm dưới đường thằng nối từ đáy 1 tới đáy 2
                var indexDay1 = histories.IndexOf(đáy2);
                var indexDay2 = histories.IndexOf(đáy1);
                var rangeFromDay1ToiDay2 = Math.Abs(indexDay1 - indexDay2);
                var averageNumberEachDay = (propertyĐáy2 - propertyValueĐáy1) / rangeFromDay1ToiDay2;
                var coPointDeuBenDuoiLine = false;
                for (int i = 1; i < rangeFromDay1ToiDay2 - 1; i++)
                {
                    var checkingDayPropertyValue = (decimal)middlePoints[i - 1].GetPropValue(property);
                    var comparedValue = propertyValueĐáy1 + averageNumberEachDay * i;
                    if (comparedValue > checkingDayPropertyValue)
                    {
                        coPointDeuBenDuoiLine = true;
                        break;
                    }
                }

                if (coPointDeuBenDuoiLine) continue;


                /*
                 * Xác định đáy nói chung
                 * từ đáy đi lên bên phải và trái, giá trị phải tăng liên tục, tăng tối thiếu 10% của giá trị đáy trước khi bị giảm hoặc đi ngang 
                 *
                 * Xác định đáy 2
                 * đường thẳng nối từ đáy 1 tới đáy 2 kéo dài ra, nếu property (RSI/MACD) của ngày sau ngày tạo đáy nằm trên đường thẳng này, chứng tỏ đáy 2 đã được tạo thành công; ngược lại đây chưa phải đáy 2
                 */
                var đãTạoĐáy2 = histories.PropertyTangDanToiKhiDatTargetTrai(đáy2, property, 0.9999M);
                var đãTạoĐáy1 = histories.PropertyTangDanToiKhiDatTargetTrai(đáy1, property, 0.9999M);
                if (!đãTạoĐáy1 || đáy1Add1Value <= propertyValueĐáy1) continue;
                if (!đãTạoĐáy2 || đáy2Add1Value <= propertyĐáy2) continue;
                //if (!đãTạoĐáy2 || đáy2Add1Value <= propertyValueĐáy2) return null;//TODO: điều kiện cần thêm
                //if (!đãTạoĐáy2 || đáy2Add1Value <= propertyĐáy2 + averageNumberEachDay) continue;


                var trendLineRsi = new Line();
                trendLineRsi.x1 = 0;  //x là trục tung - trục đối xứng
                trendLineRsi.y1 = propertyValueĐáy1;   //
                trendLineRsi.x2 = middlePoints.Count() + 2; //+ 2 vì tính từ ngày kiểm tra và ngày so sánh
                trendLineRsi.y2 = propertyĐáy2;

                var crossLineRsi = new Line();
                var cr1 = 0;
                for (cr1 = 0; cr1 < middlePoints.Count; cr1++)
                {
                    //if (đáy1.Date.AddDays(cr1) == tcr.Date)
                    //    break;
                    if (middlePoints[cr1].Date == tcr.Date) break;
                }
                //while (cr1 < middlePoints.Count())
                //{
                //    if (đáy1.Date.AddDays(cr1) == tcr.Date)
                //        break;
                //    cr1++;
                //}
                crossLineRsi.x1 = cr1 + 1;
                crossLineRsi.y1 = (decimal)tcr.GetPropValue(property); //tcr2.RSI;//tcr.RSI;

                var cr2 = 0;
                //while (cr2 < middlePoints.Count())
                //{
                //    if (đáy1.Date.AddDays(cr2) == tcr2.Date)
                //        break;
                //    cr2++;
                //}
                for (cr2 = 0; cr2 < middlePoints.Count; cr2++)
                {
                    if (middlePoints[cr2].Date == tcr2.Date) break;
                }
                crossLineRsi.x2 = cr2 + 1;
                crossLineRsi.y2 = (decimal)tcr2.GetPropValue(property); //tcr2.RSI;

                var pointRsi = trendLineRsi.FindIntersection(crossLineRsi);

                if (pointRsi == null)
                {
                    return true;
                }
            }
            return false;
        }


        public static bool LaCayVuotMA20(this List<History> histories, History humnay)
        {
            var humwa = histories.OrderByDescending(h => h.Date).First(h => h.Date < humnay.Date);
            var ngayVuotMA20 = new History();

            if (!humwa.TangGia()) return false;

            if (humwa.NenTop > humwa.BandsMid) return false;

            if (humnay.C > humnay.BandsMid && humnay.O < humnay.BandsMid) return true;

            if (humnay.NenBot > humnay.BandsMid && humwa.NenBot < humwa.BandsMid)
                return true;

            return false;
        }

        public static bool DangNamDuoiMayFlat(this History humnay, List<History> histories)
        {
            var humwa = histories.OrderByDescending(h => h.Date).First(h => h.Date < humnay.Date);
            return humnay.NenTop < humnay.IchimokuTop && humwa.IchimokuTop == humnay.IchimokuTop;
        }

        public static bool DangTrongMay(this History humnay, List<History> histories)
        {
            return humnay.C < humnay.IchimokuTop && humnay.C > humnay.IchimokuBot;
        }

        public static bool CoXuatHienMACDCatXuongSignalTrongXPhienGanNhat(this History humnay, List<History> histories, int soPhien)
        {
            var property1S = "MACD";
            var property2S = "MACDSignal";

            var checkingData = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= humnay.Date).ToList();
            for (int i = 0; i < soPhien; i++)
            {
                var humnayC = checkingData[i];
                var humwaC = checkingData[i + 1];

                //var humwa = histories.OrderByDescending(h => h.Date).First(h => h.Date < humnayC.Date);

                var property1 = (decimal)humnayC.GetPropValue(property1S);
                var property2 = string.IsNullOrEmpty(property2S) ? 0 : (decimal)humnayC.GetPropValue(property2S);

                var lineProperty1 = new Line();
                lineProperty1.x1 = 0;  //x là trục tung - trục đối xứng - trục thời gian                    
                lineProperty1.y1 = (decimal)humwaC.GetPropValue(property1S);
                lineProperty1.x2 = 1;
                lineProperty1.y2 = property1;

                var lineProperty2 = new Line();
                lineProperty2.x1 = 0;  //x là trục tung - trục đối xứng - trục thời gian                    
                lineProperty2.y1 = (decimal)humwaC.GetPropValue(property2S);
                lineProperty2.x2 = 1;
                lineProperty2.y2 = property2;

                var crossPoint = lineProperty1.FindIntersection(lineProperty2);

                var cut = crossPoint != null && property2 > property1;
                if (cut) return true;
            }

            return false;
        }

        /// <summary>
        /// Co Xuat Hien Phien Bung No Trong 5 Phien Truoc
        /// 1 - C > O 4%
        /// 2 - V > V MA 20 * 1.5
        /// 3 - Trong 5 phiên trước, không có giá nào cao hơn nửa thân nến hum nay
        /// </summary>
        /// <param name="histories"></param>
        /// <param name="history"></param>
        /// <param name="trongNPhienQuaKhu"></param>
        /// <returns></returns>
        public static bool CoXuatHienPhienBungNoTrongNPhienTruoc(this List<History> histories, History history, int trongNPhienQuaKhu = 5)
        {
            var checkingDays = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= history.Date).ToList();

            for (int i = 0; i < trongNPhienQuaKhu; i++)
            {
                var humnay = checkingDays[i];
                var humqua = histories.OrderByDescending(h => h.Date).First(h => h.Date < humnay.Date);

                var bungNoVeGia = humnay.C / humnay.O > 1.04M;
                var bungNoVeVol = humnay.V / humnay.VOL(histories, -20) > 1.5M;

                var nhungcayNenTruoc = histories.OrderByDescending(h => h.Date).Where(h => h.Date < humnay.Date).Take(trongNPhienQuaKhu).ToList();
                var trong5PhienTruocKhongCoGiaTopCaoHonNuaCayNenBungNo = nhungcayNenTruoc.All(n => n.NenTop <= (humnay.NenTop + humnay.NenBot) / 2);

                var giaTrenTatCaIndicatorLines = humnay.NenTop > humnay.IchimokuKijun
                    && humnay.NenTop > humnay.IchimokuTenKan
                    && humnay.NenTop > humnay.BandsMid
                    && humnay.RSI > 57
                    && humqua.RSI < 55
                    && trong5PhienTruocKhongCoGiaTopCaoHonNuaCayNenBungNo;

                if (bungNoVeGia && bungNoVeVol && giaTrenTatCaIndicatorLines) return true;
            }

            return false;
        }

        public static History TimPhienBungNoTrongNPhienTruoc(this List<History> histories, History history, int trongNPhienQuaKhu = 5)
        {
            var checkingDays = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= history.Date).ToList();

            for (int i = 0; i < trongNPhienQuaKhu; i++)
            {
                var humnay = checkingDays[i];
                var humqua = histories.OrderByDescending(h => h.Date).First(h => h.Date < humnay.Date);

                var bungNoVeGia = humnay.C / humnay.O > 1.04M;
                var bungNoVeVol = humnay.V / humnay.VOL(histories, -20) > 1.5M;


                var nhungcayNenTruoc = histories.OrderByDescending(h => h.Date).Where(h => h.Date < humnay.Date).Take(trongNPhienQuaKhu).ToList();
                var trong5PhienTruocKhongCoGiaTopCaoHonNuaCayNenBungNo = nhungcayNenTruoc.All(n => n.NenTop <= (humnay.NenTop + humnay.NenBot) / 2);

                var giaTrenTatCaIndicatorLines = humnay.NenTop > humnay.IchimokuKijun
                    && humnay.NenTop > humnay.IchimokuTenKan
                    && humnay.NenTop > humnay.BandsMid
                    && humnay.RSI > 57
                    && humqua.RSI < 55
                    && trong5PhienTruocKhongCoGiaTopCaoHonNuaCayNenBungNo;

                if (bungNoVeGia && bungNoVeVol && giaTrenTatCaIndicatorLines) return humnay;
            }

            return null;
        }

        public static bool BienDoBands10PhanTram(this List<History> histories, History history, int trongNPhienQuaKhu = 5)
        {
            var checkingDays = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= history.Date).ToList();
            var soPhienCoBienDoBand10PhanTram = 0;
            for (int i = 0; i < trongNPhienQuaKhu; i++)
            {
                var humnay = checkingDays[i];
                if (humnay.BandsBot * 1.1M > humnay.BandsTop) soPhienCoBienDoBand10PhanTram++;
                else break;
            }

            var bandsTopDiNgang = histories.PropertyDiNgangLienTucTrongNPhien(history, "BandsTop", trongNPhienQuaKhu, Models.Business.Patterns.LocCoPhieu.SoSanhEnum.LonHonHoacBang, 1.005M);
            var bandsBotDiNgang = histories.PropertyDiNgangLienTucTrongNPhien(history, "BandsBot", trongNPhienQuaKhu, Models.Business.Patterns.LocCoPhieu.SoSanhEnum.LonHonHoacBang, 1.005M);

            if (bandsBotDiNgang
                && bandsTopDiNgang
                && soPhienCoBienDoBand10PhanTram >= trongNPhienQuaKhu) return true;



            return false;
        }

        /// <summary>
        /// Nến giảm sát hoặc ngoài bands bot, mong đợi 1 cây bật ngược lại ở ngày mai nếu giá xanh và
        /// 1 - vol lớn hơn 100% MA 20 tại bất cứ giờ nào trong ngày tiếp theo thì đặt mua giá bands bot - 
        /// 2 - vol lớn hơn  80% MA 20 tại bất cứ giờ nào trong ngày tiếp theo thì đặt mua giá bands bot - thân nến xanh phải dài hơn 80% thân nến đỏ hôm nay
        /// 
        /// ==>Hiện tại ko xài, chung quy lại là nến đỏ ngoài bands bot, cần theo dõi phiên ngày mai nếu trong 30p đầu phiên mà xuất hiện nến xanh, vol > 1.5 MA 20 của chart h thì múc giá bands bot, càng thấp càng tốt
        /// </summary>
        /// <param name="histories"></param>
        /// <param name="history"></param>
        /// <param name="trongNPhienQuaKhu"></param>
        /// <returns></returns>
        public static bool NenGiamSatHoacNgoaiBandsBot(this List<History> histories, History history, int trongNPhienQuaKhu = 5)
        {
            var checkingDays = histories.OrderByDescending(h => h.Date).Where(h => h.Date <= history.Date).ToList();
            var soPhienCoBienDoBand10PhanTram = 0;
            for (int i = 0; i < trongNPhienQuaKhu; i++)
            {
                var humnay = checkingDays[i];
                if (humnay.BandsBot * 1.1M > humnay.BandsTop) soPhienCoBienDoBand10PhanTram++;
                else break;
            }

            var bandsTopDiNgang = histories.PropertyDiNgangLienTucTrongNPhien(history, "BandsTop", trongNPhienQuaKhu, Models.Business.Patterns.LocCoPhieu.SoSanhEnum.LonHonHoacBang, 1.005M);
            var bandsBotDiNgang = histories.PropertyDiNgangLienTucTrongNPhien(history, "BandsBot", trongNPhienQuaKhu, Models.Business.Patterns.LocCoPhieu.SoSanhEnum.LonHonHoacBang, 1.005M);

            if (bandsBotDiNgang
                && bandsTopDiNgang
                && soPhienCoBienDoBand10PhanTram >= trongNPhienQuaKhu) return true;



            return false;
        }

        public static bool DangTrendTang(this List<History> histories, History history, int trongNPhienQuaKhu = 5)
        {
            var humqua = histories.OrderByDescending(h => h.Date).First(h => h.Date < history.Date);

            var giaTrenTatCaIndicatorLines =
                //humqua.RSI < history.RSI
                humqua.RSI < 52
                //history.RSI > 50
                //&& history.RSI < 60
                //&& history.MACD > 0
                && history.MACD > history.MACDSignal;

            var macdDangTang = histories.PropertyTangLienTucTrongNPhien(history, "MACD", 2, Models.Business.Patterns.LocCoPhieu.SoSanhEnum.LonHonHoacBang);
            var rsiDangTang1 = histories.PropertyTangLienTucTrongNPhien(history, "RSI", 2, Models.Business.Patterns.LocCoPhieu.SoSanhEnum.LonHonHoacBang);
            var rsiDangTang2 = histories.PropertyDiNgangLienTucTrongNPhien(history, "RSI", 2, Models.Business.Patterns.LocCoPhieu.SoSanhEnum.LonHonHoacBang);

            var historiesInLast3Days = histories.OrderByDescending(h => h.Date).Where(h => h.Date < history.Date).Take(3).ToList();

            //và nenTop hum nay ko phải nến cao nhất trong 3 phiên trước nó 
            var notĐinh = historiesInLast3Days.OrderByDescending(h => h.H).First().H > history.H;

            //và MACD trong 3 phiên trước còn dưới 0
            var macdDuoi0Trong3PhienTruoc = historiesInLast3Days.Any(h => h.MACD < 0);

            var thaydoiGia3ThangBehon = 40;
            var historiesInLast3Months = histories.OrderByDescending(h => h.Date).Where(h => h.Date < history.Date).Take(65).ToList();
            var caoNhat = historiesInLast3Months.OrderByDescending(h => h.H).First().H;
            var thaydoi = caoNhat / history.C;

            if (
                //macdDuoi0Trong3PhienTruoc &&
                //notĐinh && 
                giaTrenTatCaIndicatorLines
                && rsiDangTang1
                //&& thaydoi > 1.01M
                && macdDangTang) return true;

            return false;
        }

        public static bool DangCoGame(this List<History> histories, History humnay, int trongNPhienQuaKhu = 2)
        {
            var humqua = histories.OrderByDescending(h => h.Date).First(h => h.Date < humnay.Date);
            //var humqua1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < humqua.Date);

            var giaTrenTatCaIndicatorLines =
                humnay.C > humqua.C
                && humnay.C >= humqua.C * 1.065M
                //&& humqua.C >= humqua1.C * 1.065M
                && humnay.V < humnay.VOL(histories, -20)
                && humnay.C == humnay.O
                && humqua.C == humqua.O
                ;

            if (giaTrenTatCaIndicatorLines) return true;

            return false;
        }

        public static bool KietVol(this List<History> histories, History humnay, int trongNPhienQuaKhu = 2)
        {
            var humqua = histories.OrderByDescending(h => h.Date).First(h => h.Date < humnay.Date);
            //var humqua1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < humqua.Date);

            var giaTrenTatCaIndicatorLines =
                humnay.C > humqua.C
                && humnay.C >= humqua.C * 1.065M
                //&& humqua.C >= humqua1.C * 1.065M
                && humnay.V < humnay.VOL(histories, -20)
                && humnay.C == humnay.O
                && humqua.C == humqua.O
                ;

            if (giaTrenTatCaIndicatorLines) return true;

            return false;
        }

        public static EnumPhanKi XacDinhPhanKi(this List<History> histories, History todaySession, string propertyPhanky = "RSI")
        {
            /*
             * What is the point
             * - it is created by 3 points (1, 2 and 3) in a row where the pattern could be
             * 1 - the middle point (2) is lower than the other 2 (1 & 3)
             * 2 - the last point (3) is higher than the middle point (2) and the middle point is equal with the 1st one (1)
             * 3 - the last point (3) is equal with the middle point (2) and the 1st point is higher than the 2nd one (2)
             * 
             * RSI - PKD - it will consider the points with pattern 1 & 2
             * 1 - take current RSI number
             * 2 - continue counting back to the past in a period between today and the last PKA
             * 3 - get the previous point where the RSI is lower than the current one
             * 4 - Compare 2 points, if the RSI of the current one is higher than the one in the past and the closing price is oppsite -> this is PKD
             * 
             * RSI - PKA - it will consider the points with pattern 1 & 3
             * 1 - take current RSI number
             * 2 - continue counting back to the past in a period between today and the last 60 sessions
             * 3 - get the previous point where the RSI is higher than the current one
             * 4 - Compare 2 points, if the RSI of the current one is lower than the one in the past and the closing price is oppsite -> this is PKA
             * 
             * The same for the MACD - remember that MACD is usually slowere than RSI 1 day
            */

            var currentValueToday = (decimal)todaySession.GetPropValue(propertyPhanky);
            var isPatternDay = todaySession.IsPatternSpikeForPhanKiDuong(histories, propertyPhanky);
            var isPatternDinh = todaySession.IsPatternSpikeForPhanKiAm(histories, propertyPhanky);

            var tmrData = histories.OrderBy(h => h.Date).Where(h => h.Date > todaySession.Date).FirstOrDefault();

            //------------------------PK DƯƠNG
            if (isPatternDay)
            {
                var theLastExpectedPKA = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < todaySession.Date && h.RSIPhanKi == EnumPhanKi.PKA);
                var theExpecetedPeriodForPKD = theLastExpectedPKA == null
                    ? histories.Where(h => h.Date < todaySession.Date).OrderByDescending(h => h.Date).ToList()
                    : histories.Where(h => h.Date < todaySession.Date && h.Date >= theLastExpectedPKA.Date).OrderByDescending(h => h.Date).ToList();
                var theExpectedLowerPoints = theExpecetedPeriodForPKD.Where(x => x.RSI < currentValueToday).OrderByDescending(h => h.Date).ToList();
                if (propertyPhanky == "MACD")
                    theExpectedLowerPoints = theExpecetedPeriodForPKD.Where(x => x.MACD < currentValueToday).OrderByDescending(h => h.Date).ToList();

                foreach (var lowerPointOfPropertyInThePast in theExpectedLowerPoints)
                {
                    if (lowerPointOfPropertyInThePast.IsPatternSpikeForPhanKiDuong(histories, propertyPhanky)
                        && todaySession.HasLowerPointInMiddle(lowerPointOfPropertyInThePast, histories, propertyPhanky)
                        && !todaySession.HasLowerPointInMiddle1(lowerPointOfPropertyInThePast, histories, propertyPhanky)
                        && lowerPointOfPropertyInThePast.NenBot > todaySession.NenBot)
                        return EnumPhanKi.PKD;
                }
            }

            //------------------------PK ÂM
            if (isPatternDinh)
            {
                var theLastExpectedCheck = histories.OrderByDescending(h => h.Date).Where(h => h.Date < todaySession.Date).Skip(59).FirstOrDefault();
                var theExpecetedPeriodForPKA = theLastExpectedCheck == null
                    ? histories.Where(h => h.Date < todaySession.Date).OrderByDescending(h => h.Date).ToList()
                    : histories.Where(h => h.Date < todaySession.Date && h.Date >= theLastExpectedCheck.Date).OrderByDescending(h => h.Date).ToList();
                var theExpectedHigherPoints = theExpecetedPeriodForPKA.Where(x => x.RSI > currentValueToday).OrderByDescending(h => h.Date).ToList();

                if (propertyPhanky == "MACD")
                    theExpectedHigherPoints = theExpecetedPeriodForPKA.Where(x => x.MACD > currentValueToday).OrderByDescending(h => h.Date).ToList();

                foreach (var higherPointOfPropertyInThePast in theExpectedHigherPoints)
                {
                    if (higherPointOfPropertyInThePast.IsPatternSpikeForPhanKiAm(histories, propertyPhanky)
                        && todaySession.HasHigherPointInMiddle(higherPointOfPropertyInThePast, histories, propertyPhanky)
                        && !todaySession.HasHigherPointInMiddle1(higherPointOfPropertyInThePast, histories, propertyPhanky)
                        && (higherPointOfPropertyInThePast.NenBot < todaySession.NenTop || (tmrData != null && tmrData.GiamGia() && higherPointOfPropertyInThePast.NenBot < tmrData.H)))
                        return EnumPhanKi.PKA;
                }
            }

            return EnumPhanKi.NA;
        }

        /// <summary>
        /// Wrong: BAF 26-12-22 (đáy 2) VS 03-10-22 (Đáy 1)
        /// </summary>
        public static bool HasLowerPointInMiddle(this History todaySession, History previousSession, List<History> histories, string propertyPhanky = "RSI")
        {
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > previousSession.Date && h.Date < todaySession.Date).ToList();
            if (middlePoints.Count < 2) return false; //ở giữa ít nhất 2 điểm luôn luôn true vì mình đã skip 3

            var propertyValuePoint1 = (decimal)previousSession.GetPropValue(propertyPhanky);
            var propertyValuePoint2 = (decimal)todaySession.GetPropValue(propertyPhanky);
            if (propertyValuePoint1 > propertyValuePoint2) return false;

            return HasCrossPointInMiddle(todaySession, previousSession, histories, propertyPhanky);
        }

        public static bool HasLowerPointInMiddle1(this History todaySession, History previousSession, List<History> histories, string propertyPhanky = "RSI")
        {
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > previousSession.Date && h.Date < todaySession.Date).ToList();
            if (middlePoints.Count < 2) return false; //ở giữa ít nhất 2 điểm luôn luôn true vì mình đã skip 3

            var propertyValuePoint1 = (decimal)previousSession.GetPropValue(propertyPhanky);
            var propertyValuePoint2 = (decimal)todaySession.GetPropValue(propertyPhanky);
            if (propertyValuePoint1 > propertyValuePoint2) return false;

            var indexDay1 = histories.IndexOf(todaySession);
            var indexDay2 = histories.IndexOf(previousSession);
            var rangeFromDay1ToiDay2 = Math.Abs(indexDay1 - indexDay2);
            var averageNumberEachDay = (propertyValuePoint2 - propertyValuePoint1) / rangeFromDay1ToiDay2;

            for (int i = 1; i < rangeFromDay1ToiDay2; i++)
            {
                var checkingDayPropertyValue = (decimal)middlePoints[i - 1].GetPropValue(propertyPhanky);
                var comparedValue = propertyValuePoint1 + averageNumberEachDay * i;
                if (comparedValue > checkingDayPropertyValue)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasCrossPointInMiddle(this History todaySession, History previousSession, List<History> histories, string propertyPhanky = "RSI")
        {
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > previousSession.Date && h.Date < todaySession.Date).ToList();
            if (middlePoints.Count < 2) return false; //ở giữa ít nhất 2 điểm luôn luôn true vì mình đã skip 3

            var propertyValuePoint1 = (decimal)previousSession.GetPropValue(propertyPhanky);
            var propertyValuePoint2 = (decimal)todaySession.GetPropValue(propertyPhanky);

            //Tất cả middle points ko có point nào dc phép nằm trên đường thằng nối từ đáy 1 tới đáy 2
            var propertyInfo = typeof(History).GetProperty(propertyPhanky);
            var highestValueInMiddlePoints = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).First();
            var lowestValueInMiddlePoints = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).Last();

            var trendLine = new Line();
            trendLine.x1 = 0;                            //x là trục tung - trục đối xứng
            trendLine.y1 = propertyValuePoint1;          //
            trendLine.x2 = middlePoints.Count() + 2;     //+ 2 vì tính từ ngày kiểm tra và ngày so sánh
            trendLine.y2 = propertyValuePoint2;

            var crossLine = new Line();
            var cr1 = 1;
            while (cr1 < middlePoints.Count())
            {
                if (previousSession.Date.AddDays(cr1) == highestValueInMiddlePoints.Date)
                    break;
                cr1++;
            }
            crossLine.x1 = cr1;
            crossLine.y1 = (decimal)highestValueInMiddlePoints.GetPropValue(propertyPhanky); //tcr2.RSI;//tcr.RSI;

            var cr2 = 1;
            while (cr2 < middlePoints.Count())
            {
                if (previousSession.Date.AddDays(cr2) == lowestValueInMiddlePoints.Date)
                    break;
                cr2++;
            }
            crossLine.x2 = cr2;
            crossLine.y2 = (decimal)lowestValueInMiddlePoints.GetPropValue(propertyPhanky); //tcr2.RSI;

            var commonPoint = trendLine.FindIntersection(crossLine);                      //if the trend line (from point 1 to point 2) has any cross point with crossline (lowest/highest points in middle) => then the trendline isn't a perfect line
            if (commonPoint != null)
                return false;

            return true;
        }

        public static bool HasHigherPointInMiddle(this History todaySession, History previousSession, List<History> histories, string propertyPhanky = "RSI")
        {
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > previousSession.Date && h.Date < todaySession.Date).ToList();
            if (middlePoints.Count < 2) return false; //ở giữa ít nhất 2 điểm luôn luôn true vì mình đã skip 3

            var propertyValuePoint1 = (decimal)previousSession.GetPropValue(propertyPhanky);
            var propertyValuePoint2 = (decimal)todaySession.GetPropValue(propertyPhanky);
            if (propertyValuePoint1 < propertyValuePoint2) return false;

            return HasCrossPointInMiddle(todaySession, previousSession, histories, propertyPhanky);
        }

        public static bool HasHigherPointInMiddle1(this History todaySession, History previousSession, List<History> histories, string propertyPhanky = "RSI")
        {
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > previousSession.Date && h.Date < todaySession.Date).ToList();
            if (middlePoints.Count < 2) return false; //ở giữa ít nhất 2 điểm luôn luôn true vì mình đã skip 3

            var propertyValuePoint1 = (decimal)previousSession.GetPropValue(propertyPhanky);
            var propertyValuePoint2 = (decimal)todaySession.GetPropValue(propertyPhanky);
            if (propertyValuePoint1 < propertyValuePoint2) return false;

            var indexDay1 = histories.IndexOf(todaySession);
            var indexDay2 = histories.IndexOf(previousSession);
            var rangeFromDay1ToiDay2 = Math.Abs(indexDay1 - indexDay2) + 1;
            var averageNumberEachDay = (propertyValuePoint1 - propertyValuePoint2) / rangeFromDay1ToiDay2;

            for (int i = 1; i <= rangeFromDay1ToiDay2 - 2; i++)
            {
                var checkingDayPropertyValue = (decimal)middlePoints[i - 1].GetPropValue(propertyPhanky);
                var comparedValue = propertyValuePoint1 - averageNumberEachDay * i;
                if (comparedValue < checkingDayPropertyValue)
                {
                    return true;
                }
            }

            return false;
        }


        public static bool HasLowerPointInMiddle(this HistoryHour todaySession, HistoryHour previousSession, List<HistoryHour> histories, string propertyPhanky = "RSI")
        {
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > previousSession.Date && h.Date < todaySession.Date).ToList();
            if (middlePoints.Count < 2) return false; //ở giữa ít nhất 2 điểm luôn luôn true vì mình đã skip 3

            var propertyValuePoint1 = (decimal)previousSession.GetPropValue(propertyPhanky);
            var propertyValuePoint2 = (decimal)todaySession.GetPropValue(propertyPhanky);
            if (propertyValuePoint1 > propertyValuePoint2) return false;

            return HasCrossPointInMiddle(todaySession, previousSession, histories, propertyPhanky);
        }

        public static bool HasLowerPointInMiddle1(this HistoryHour todaySession, HistoryHour previousSession, List<HistoryHour> histories, string propertyPhanky = "RSI")
        {
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > previousSession.Date && h.Date < todaySession.Date).ToList();
            if (middlePoints.Count < 2) return false; //ở giữa ít nhất 2 điểm luôn luôn true vì mình đã skip 3

            var propertyValuePoint1 = (decimal)previousSession.GetPropValue(propertyPhanky);
            var propertyValuePoint2 = (decimal)todaySession.GetPropValue(propertyPhanky);
            if (propertyValuePoint1 > propertyValuePoint2) return false;

            var indexDay1 = histories.IndexOf(todaySession);
            var indexDay2 = histories.IndexOf(previousSession);
            var rangeFromDay1ToiDay2 = Math.Abs(indexDay1 - indexDay2) + 1;
            var averageNumberEachDay = (propertyValuePoint2 - propertyValuePoint1) / rangeFromDay1ToiDay2;

            for (int i = 1; i <= rangeFromDay1ToiDay2 - 2; i++)
            {
                var checkingDayPropertyValue = (decimal)middlePoints[i - 1].GetPropValue(propertyPhanky);
                var comparedValue = propertyValuePoint1 + averageNumberEachDay * i;
                if (comparedValue > checkingDayPropertyValue)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasCrossPointInMiddle(this HistoryHour todaySession, HistoryHour previousSession, List<HistoryHour> histories, string propertyPhanky = "RSI")
        {
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > previousSession.Date && h.Date < todaySession.Date).ToList();
            if (middlePoints.Count < 2) return false; //ở giữa ít nhất 2 điểm luôn luôn true vì mình đã skip 3

            var propertyValuePoint1 = (decimal)previousSession.GetPropValue(propertyPhanky);
            var propertyValuePoint2 = (decimal)todaySession.GetPropValue(propertyPhanky);

            //Tất cả middle points ko có point nào dc phép nằm trên đường thằng nối từ đáy 1 tới đáy 2
            var propertyInfo = typeof(HistoryHour).GetProperty(propertyPhanky);
            var highestValueInMiddlePoints = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).First();
            var lowestValueInMiddlePoints = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).Last();

            var trendLine = new Line();
            trendLine.x1 = 0;                            //x là trục tung - trục đối xứng
            trendLine.y1 = propertyValuePoint1;          //
            trendLine.x2 = middlePoints.Count() + 2;     //+ 2 vì tính từ ngày kiểm tra và ngày so sánh
            trendLine.y2 = propertyValuePoint2;

            var crossLine = new Line();
            var cr1 = 1;
            while (cr1 < middlePoints.Count())
            {
                if (previousSession.Date.AddDays(cr1) == highestValueInMiddlePoints.Date)
                    break;
                cr1++;
            }
            crossLine.x1 = cr1;
            crossLine.y1 = (decimal)highestValueInMiddlePoints.GetPropValue(propertyPhanky); //tcr2.RSI;//tcr.RSI;

            var cr2 = 1;
            while (cr2 < middlePoints.Count())
            {
                if (previousSession.Date.AddDays(cr2) == lowestValueInMiddlePoints.Date)
                    break;
                cr2++;
            }
            crossLine.x2 = cr2;
            crossLine.y2 = (decimal)lowestValueInMiddlePoints.GetPropValue(propertyPhanky); //tcr2.RSI;

            var commonPoint = trendLine.FindIntersection(crossLine);                      //if the trend line (from point 1 to point 2) has any cross point with crossline (lowest/highest points in middle) => then the trendline isn't a perfect line
            if (commonPoint != null)
                return false;

            return true;
        }

        public static bool HasHigherPointInMiddle(this HistoryHour todaySession, HistoryHour previousSession, List<HistoryHour> histories, string propertyPhanky = "RSI")
        {
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > previousSession.Date && h.Date < todaySession.Date).ToList();
            if (middlePoints.Count < 2) return false; //ở giữa ít nhất 2 điểm luôn luôn true vì mình đã skip 3

            var propertyValuePoint1 = (decimal)previousSession.GetPropValue(propertyPhanky);
            var propertyValuePoint2 = (decimal)todaySession.GetPropValue(propertyPhanky);
            if (propertyValuePoint1 < propertyValuePoint2) return false;

            return HasCrossPointInMiddle(todaySession, previousSession, histories, propertyPhanky);
        }

        public static bool HasHigherPointInMiddle1(this HistoryHour todaySession, HistoryHour previousSession, List<HistoryHour> histories, string propertyPhanky = "RSI")
        {
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > previousSession.Date && h.Date < todaySession.Date).ToList();
            if (middlePoints.Count < 2) return false; //ở giữa ít nhất 2 điểm luôn luôn true vì mình đã skip 3

            var propertyValuePoint1 = (decimal)previousSession.GetPropValue(propertyPhanky);
            var propertyValuePoint2 = (decimal)todaySession.GetPropValue(propertyPhanky);
            if (propertyValuePoint1 < propertyValuePoint2) return false;

            var indexDay1 = histories.IndexOf(todaySession);
            var indexDay2 = histories.IndexOf(previousSession);
            var rangeFromDay1ToiDay2 = Math.Abs(indexDay1 - indexDay2) + 1;
            var averageNumberEachDay = (propertyValuePoint1 - propertyValuePoint2) / rangeFromDay1ToiDay2;

            for (int i = 1; i <= rangeFromDay1ToiDay2 - 2; i++)
            {
                try
                {
                    var checkingDayPropertyValue = (decimal)middlePoints[i - 1].GetPropValue(propertyPhanky);
                    var comparedValue = propertyValuePoint1 - averageNumberEachDay * i;
                    if (comparedValue < checkingDayPropertyValue)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }

            return false;
        }




        public static bool IsPatternSpikeForPhanKiAm(this History todaySession, List<History> histories, string propertyPhanky = "RSI")
        {
            var previousPoints = histories.Where(h => h.Date < todaySession.Date).OrderByDescending(h => h.Date).Take(3).ToList();
            if (previousPoints == null || !previousPoints.Any()) return false;

            var nextPoints = histories.Where(h => h.Date > todaySession.Date).OrderBy(h => h.Date).Take(3).ToList();
            if (nextPoints == null || !previousPoints.Any()) return false;

            if (todaySession.GiamDan(nextPoints, propertyPhanky) && todaySession.GiamDan(previousPoints, propertyPhanky))
                return true;

            return false;
        }

        public static bool IsPatternSpikeForPhanKiDuong(this History todaySession, List<History> histories, string propertyPhanky = "RSI")
        {
            if (!histories.Any()) return false;
            var previousPoints = histories.Where(h => h.Date < todaySession.Date).OrderByDescending(h => h.Date).Take(3).ToList();
            if (previousPoints == null || !previousPoints.Any()) return false;

            var nextPoints = histories.Where(h => h.Date > todaySession.Date).OrderBy(h => h.Date).Take(3).ToList();
            if (nextPoints == null || !previousPoints.Any()) return false;

            /*
             * What is the point
             * - it is created by 3 points (1, 2 and 3) in a row where the pattern could be
             * 1 - the middle point (2) is lower than the other 2 (1 & 3)
             * 2 - the last point (3) is higher than the middle point (2) and the middle point is equal with the 1st one (1)
             * 3 - the last point (3) is equal with the middle point (2) and the 1st point is higher than the 2nd one (2)
             */

            if (todaySession.TangDan(nextPoints, propertyPhanky) && todaySession.TangDan(previousPoints, propertyPhanky))
                return true;

            return false;
        }

        public static bool IsPatternSpikeForPhanKiAm(this HistoryHour todaySession, List<HistoryHour> histories, string propertyPhanky = "RSI")
        {
            var previousPoints = histories.Where(h => h.Date < todaySession.Date).OrderByDescending(h => h.Date).Take(3).ToList();
            if (previousPoints == null || !previousPoints.Any()) return false;

            var nextPoints = histories.Where(h => h.Date > todaySession.Date).OrderBy(h => h.Date).Take(3).ToList();
            if (nextPoints == null || !previousPoints.Any()) return false;

            if (todaySession.GiamDan(nextPoints, propertyPhanky) && todaySession.GiamDan(previousPoints, propertyPhanky))
                return true;

            return false;
        }

        public static bool IsPatternSpikeForPhanKiDuong(this HistoryHour todaySession, List<HistoryHour> histories, string propertyPhanky = "RSI")
        {
            if (!histories.Any()) return false;
            var previousPoints = histories.Where(h => h.Date < todaySession.Date).OrderByDescending(h => h.Date).Take(3).ToList();
            if (previousPoints == null || !previousPoints.Any()) return false;

            var nextPoints = histories.Where(h => h.Date > todaySession.Date).OrderBy(h => h.Date).Take(3).ToList();
            if (nextPoints == null || !previousPoints.Any()) return false;

            /*
             * What is the point
             * - it is created by 3 points (1, 2 and 3) in a row where the pattern could be
             * 1 - the middle point (2) is lower than the other 2 (1 & 3)
             * 2 - the last point (3) is higher than the middle point (2) and the middle point is equal with the 1st one (1)
             * 3 - the last point (3) is equal with the middle point (2) and the 1st point is higher than the 2nd one (2)
             */

            if (todaySession.TangDan(nextPoints, propertyPhanky) && todaySession.TangDan(previousPoints, propertyPhanky))
                return true;

            return false;
        }

        public static bool TangDan(this HistoryHour todaySession, List<HistoryHour> histories, string propertyPhanky = "RSI")
        {
            if (!histories.Any()) return false;
            var currentValueToday = (decimal)todaySession.GetPropValue(propertyPhanky);
            var value = false;

            var checkingValue = (decimal)histories[0].GetPropValue(propertyPhanky);
            if (checkingValue > currentValueToday) return true;
            if (checkingValue == currentValueToday)
            {
                var checkingHistories = histories.Where(h => h.Date != histories[0].Date).ToList();
                value = histories[0].TangDan(checkingHistories, propertyPhanky);
            }

            return value;
        }

        public static bool GiamDan(this HistoryHour todaySession, List<HistoryHour> histories, string propertyPhanky = "RSI")
        {
            if (!histories.Any()) return false;

            var currentValueToday = (decimal)todaySession.GetPropValue(propertyPhanky);
            var value = false;

            var checkingValue = (decimal)histories[0].GetPropValue(propertyPhanky);
            if (checkingValue < currentValueToday) return true;
            if (checkingValue == currentValueToday)
            {
                var checkingHistories = histories.Where(h => h.Date != histories[0].Date).ToList();
                value = histories[0].GiamDan(checkingHistories, propertyPhanky);
            }

            return value;
        }

        public static bool TangDan(this History todaySession, List<History> histories, string propertyPhanky = "RSI")
        {
            if (!histories.Any()) return false;
            var currentValueToday = (decimal)todaySession.GetPropValue(propertyPhanky);
            var value = false;

            var checkingValue = (decimal)histories[0].GetPropValue(propertyPhanky);
            if (checkingValue > currentValueToday) return true;
            if (checkingValue == currentValueToday)
            {
                var checkingHistories = histories.Where(h => h.Date != histories[0].Date).ToList();
                value = histories[0].TangDan(checkingHistories, propertyPhanky);
            }

            return value;
        }

        public static bool GiamDan(this History todaySession, List<History> histories, string propertyPhanky = "RSI")
        {
            if (!histories.Any()) return false;

            var currentValueToday = (decimal)todaySession.GetPropValue(propertyPhanky);
            var value = false;

            var checkingValue = (decimal)histories[0].GetPropValue(propertyPhanky);
            if (checkingValue < currentValueToday) return true;
            if (checkingValue == currentValueToday)
            {
                var checkingHistories = histories.Where(h => h.Date != histories[0].Date).ToList();
                value = histories[0].GiamDan(checkingHistories, propertyPhanky);
            }

            return value;
        }


        public static bool? HasPhanKyDuong(this List<HistoryHour> histories, HistoryHour đáy2, HistoryHour đáy1, string propertyPhanky = "RSI",
        int soPhienDeKiemTraPhanki = 60,
        decimal CL2G = 1.01M, //decimal CL2G = 0.95M,
        decimal CL2D = 0.1M)
        {
            if (đáy2.VOL(histories, -20) < 50000) return null;
            var chenhLechĐủ = đáy1.NenBot / đáy2.NenBot >= CL2G; //1.02M; 1/lọc giá trị vol < 100K
            if (chenhLechĐủ == false) return null;

            var đáy2Minus1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < đáy2.Date);
            var đáy1Minus1 = histories.OrderByDescending(h => h.Date).First(h => h.Date < đáy1.Date);
            var đáy1Add1 = histories.OrderBy(h => h.Date).First(h => h.Date > đáy1.Date);
            var đáy2Add1 = histories.OrderBy(h => h.Date).First(h => h.Date > đáy2.Date);

            var propertyValueĐáy2 = (decimal)đáy2.GetPropValue(propertyPhanky);
            var propertyValueĐáy1 = (decimal)đáy1.GetPropValue(propertyPhanky);

            //if (propertyValueĐáy1 + Math.Abs(propertyValueĐáy1 * CL2D) >= propertyValueĐáy2) return null;
            if (propertyValueĐáy2 < propertyValueĐáy1) return null;                                       //PVS 13/4/21 - 22/4/21

            var đáy1Minus1Value = (decimal)đáy1Minus1.GetPropValue(propertyPhanky);
            var đáy1Add1Value = (decimal)đáy1Add1.GetPropValue(propertyPhanky);
            var đáy2Minus1Value = (decimal)đáy2Minus1.GetPropValue(propertyPhanky);
            var đáy2Add1Value = (decimal)đáy2Add1.GetPropValue(propertyPhanky);

            var đáy2CenterPoint = new Vector2(0, (float)propertyValueĐáy2);
            var điểmTăngNgàyHumnay = new Vector2(1, (float)đáy2Add1Value);

            //var deg = điểmTăngNgàyHumnay.GetAngle(đáy2CenterPoint);
            //if (deg <= 20) return null;

            //Giữa 2 điểm so sánh, ko có 1 điểm nào bé hơn điểm đang xét cả
            var middlePoints = histories.OrderBy(h => h.Date).Where(h => h.Date > đáy1.Date && h.Date < đáy2.Date).ToList();
            if (middlePoints.Count < 2) return null; //ở giữa ít nhất 2 điểm - TODO: luôn luôn true vì mình đã skip 3

            var propertyInfo = typeof(HistoryHour).GetProperty(propertyPhanky);
            var tcr = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).First();
            var tcr2 = middlePoints.OrderByDescending(x => propertyInfo.GetValue(x, null)).Last();

            if (middlePoints.Any(x => (decimal)propertyInfo.GetValue(x, null) <= propertyValueĐáy1)) return null;

            var indexDay1 = histories.IndexOf(đáy2);
            var indexDay2 = histories.IndexOf(đáy1);
            var rangeFromDay1ToiDay2 = Math.Abs(indexDay1 - indexDay2);
            var averageNumberEachDay = (propertyValueĐáy2 - propertyValueĐáy1) / rangeFromDay1ToiDay2;

            ////Tất cả middle points ko có point nào dc phép nằm dưới đường thằng nối từ đáy 1 tới đáy 2
            //var coPointDeuBenDuoiLine = false;
            //for (int i = 1; i < rangeFromDay1ToiDay2; i++)
            //{
            //    var checkingDayPropertyValue = (decimal)middlePoints[i - 1].GetPropValue(propertyPhanky);
            //    var comparedValue = propertyValueĐáy1 + averageNumberEachDay * i;
            //    if (comparedValue > checkingDayPropertyValue)
            //    {
            //        coPointDeuBenDuoiLine = true;
            //        break;
            //    }
            //}
            //if (coPointDeuBenDuoiLine) return null;   //TODO: tạm thời bỏ qua dk này


            /*
             * Xác định đáy nói chung
             * từ đáy đi lên bên phải và trái, giá trị phải tăng liên tục, tăng tối thiếu 10% của giá trị đáy trước khi bị giảm hoặc đi ngang 
             *
             * Xác định đáy 2
             * đường thẳng nối từ đáy 1 tới đáy 2 kéo dài ra, nếu property (RSI/MACD) của ngày sau ngày tạo đáy nằm trên đường thẳng này, chứng tỏ đáy 2 đã được tạo thành công; ngược lại đây chưa phải đáy 2
             */
            var đãTạoĐáy2 = histories.PropertyTangDanToiKhiDatTargetTrai(đáy2, propertyPhanky, 0.9M);
            var đãTạoĐáy1 = histories.PropertyTangDanToiKhiDatTargetTrai(đáy1, propertyPhanky, 0.9M);
            if (!đãTạoĐáy1 || đáy1Add1Value <= propertyValueĐáy1) return null;
            //if (!đãTạoĐáy2 || đáy2Add1Value <= propertyValueĐáy2) return null;//TODO: điều kiện cần thêm
            if (!đãTạoĐáy2 || đáy2Add1Value <= propertyValueĐáy2 + averageNumberEachDay) return null;

            var trendLineRsi = new Line();
            trendLineRsi.x1 = 0;  //x là trục tung - trục đối xứng
            trendLineRsi.y1 = propertyValueĐáy1;   //
            trendLineRsi.x2 = middlePoints.Count() + 2; //+ 2 vì tính từ ngày kiểm tra và ngày so sánh
            trendLineRsi.y2 = propertyValueĐáy2;

            var crossLineRsi = new Line();
            var cr1 = 1;
            while (cr1 < middlePoints.Count())
            {
                if (đáy1.Date.AddDays(cr1) == tcr.Date)
                    break;
                cr1++;
            }
            crossLineRsi.x1 = cr1;
            crossLineRsi.y1 = (decimal)tcr.GetPropValue(propertyPhanky); //tcr2.RSI;//tcr.RSI;

            var cr2 = 1;
            while (cr2 < middlePoints.Count())
            {
                if (đáy1.Date.AddDays(cr2) == tcr2.Date)
                    break;
                cr2++;
            }
            crossLineRsi.x2 = cr2;
            crossLineRsi.y2 = (decimal)tcr2.GetPropValue(propertyPhanky); //tcr2.RSI;

            var pointRsi = trendLineRsi.FindIntersection(crossLineRsi);

            if (pointRsi == null)
            {
                return true;
            }

            return null;
        }

        /// <summary>
        /// Trong vong 30 phien, có > 60% số phiên với vol > vol của 20 phiên trước đó
        /// </summary>
        public static bool CoDongTienVo(this List<History> histories, History checkingSession)
        {
            var expectedSession = 20;
            var checkingHistories = histories.OrderByDescending(h => h.Date).ToList();
            if (!checkingHistories.Any() || checkingHistories.Count < expectedSession) return false;

            var numberOfSessionHasBigVol = 0;
            for (int i = 0; i < expectedSession; i++)
            {
                var ma20Vol = checkingHistories[i].VOL(checkingHistories, -20);
                if (checkingHistories[i].V > ma20Vol)
                    numberOfSessionHasBigVol++;
            }

            return numberOfSessionHasBigVol / expectedSession > 0.4M;
        }

        public static EnumPhanKi XacDinhPhanKi(this List<HistoryHour> histories, HistoryHour todaySession, string propertyPhanky = "RSI")
        {
            /*
             * What is the point
             * - it is created by 3 points (1, 2 and 3) in a row where the pattern could be
             * 1 - the middle point (2) is lower than the other 2 (1 & 3)
             * 2 - the last point (3) is higher than the middle point (2) and the middle point is equal with the 1st one (1)
             * 3 - the last point (3) is equal with the middle point (2) and the 1st point is higher than the 2nd one (2)
             * 
             * RSI - PKD - it will consider the points with pattern 1 & 2
             * 1 - take current RSI number
             * 2 - continue counting back to the past in a period between today and the last PKA
             * 3 - get the previous point where the RSI is lower than the current one
             * 4 - Compare 2 points, if the RSI of the current one is higher than the one in the past and the closing price is oppsite -> this is PKD
             * 
             * RSI - PKA - it will consider the points with pattern 1 & 3
             * 1 - take current RSI number
             * 2 - continue counting back to the past in a period between today and the last 60 sessions
             * 3 - get the previous point where the RSI is higher than the current one
             * 4 - Compare 2 points, if the RSI of the current one is lower than the one in the past and the closing price is oppsite -> this is PKA
             * 
             * The same for the MACD - remember that MACD is usually slowere than RSI 1 day
            */

            var currentValueToday = (decimal)todaySession.GetPropValue(propertyPhanky);
            var isPatternDay = todaySession.IsPatternSpikeForPhanKiDuong(histories, propertyPhanky);
            var isPatternDinh = todaySession.IsPatternSpikeForPhanKiAm(histories, propertyPhanky);

            var tmrData = histories.OrderBy(h => h.Date).Where(h => h.Date > todaySession.Date).FirstOrDefault();

            //------------------------PK DƯƠNG
            if (isPatternDay)
            {
                var theLastExpectedPKA = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < todaySession.Date && h.RSIPhanKi == EnumPhanKi.PKA);
                var theExpecetedPeriodForPKD = theLastExpectedPKA == null
                    ? histories.Where(h => h.Date < todaySession.Date).OrderByDescending(h => h.Date).ToList()
                    : histories.Where(h => h.Date < todaySession.Date && h.Date >= theLastExpectedPKA.Date).OrderByDescending(h => h.Date).ToList();
                var theExpectedLowerPoints = theExpecetedPeriodForPKD.Where(x => x.RSI < currentValueToday).OrderByDescending(h => h.Date).ToList();
                if (propertyPhanky == "MACD")
                    theExpectedLowerPoints = theExpecetedPeriodForPKD.Where(x => x.MACD < currentValueToday).OrderByDescending(h => h.Date).ToList();

                foreach (var lowerPointOfPropertyInThePast in theExpectedLowerPoints)
                {
                    if (lowerPointOfPropertyInThePast.IsPatternSpikeForPhanKiDuong(histories, propertyPhanky)
                        && todaySession.HasLowerPointInMiddle(lowerPointOfPropertyInThePast, histories, propertyPhanky)
                        && !todaySession.HasLowerPointInMiddle1(lowerPointOfPropertyInThePast, histories, propertyPhanky)
                        && lowerPointOfPropertyInThePast.NenBot > todaySession.NenBot)
                        return EnumPhanKi.PKD;
                }
            }

            //------------------------PK ÂM
            if (isPatternDinh)
            {
                var theLastExpectedCheck = histories.OrderByDescending(h => h.Date).Where(h => h.Date < todaySession.Date).Skip(59).FirstOrDefault();
                var theExpecetedPeriodForPKA = theLastExpectedCheck == null
                    ? histories.Where(h => h.Date < todaySession.Date).OrderByDescending(h => h.Date).ToList()
                    : histories.Where(h => h.Date < todaySession.Date && h.Date >= theLastExpectedCheck.Date).OrderByDescending(h => h.Date).ToList();
                var theExpectedHigherPoints = theExpecetedPeriodForPKA.Where(x => x.RSI > currentValueToday).OrderByDescending(h => h.Date).ToList();

                if (propertyPhanky == "MACD")
                    theExpectedHigherPoints = theExpecetedPeriodForPKA.Where(x => x.MACD > currentValueToday).OrderByDescending(h => h.Date).ToList();

                foreach (var higherPointOfPropertyInThePast in theExpectedHigherPoints)
                {
                    if (higherPointOfPropertyInThePast.IsPatternSpikeForPhanKiAm(histories, propertyPhanky)
                        && todaySession.HasHigherPointInMiddle(higherPointOfPropertyInThePast, histories, propertyPhanky)
                        && !todaySession.HasHigherPointInMiddle1(higherPointOfPropertyInThePast, histories, propertyPhanky)
                        && (higherPointOfPropertyInThePast.NenBot < todaySession.NenTop || (tmrData != null && tmrData.GiamGia() && higherPointOfPropertyInThePast.NenBot < tmrData.H)))
                        return EnumPhanKi.PKA;
                }
            }

            return EnumPhanKi.NA;
        }
    }
}
