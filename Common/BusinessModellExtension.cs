﻿using DotNetCoreSqlDb.Common.ArrayExtensions;
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

        public static bool VOLBienDong(this StockSymbolHistory checkingDate, List<StockSymbolHistory> histories, int numberOfPreviousPhien, decimal bienDong)
        {
            if (numberOfPreviousPhien == 0) return false;

            var checkingRange = new List<StockSymbolHistory>();
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

        public static decimal VOL(this StockSymbolHistory checkingDate, List<StockSymbolHistory> histories, int numberOfPreviousPhien)
        {
            if (numberOfPreviousPhien == 0) return 0;

            var checkingRange = new List<StockSymbolHistory>();
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

        public static decimal MA(this StockSymbolHistory checkingDate, List<StockSymbolHistory> histories, int numberOfPreviousPhien)
        {
            if (numberOfPreviousPhien == 0) return 0;

            var checkingRange = new List<StockSymbolHistory>();
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

        public static bool IsPriceUp(this StockSymbolHistory today, List<StockSymbolHistory> histories, int numberOfSideway)
        {
            var todayVol = today.C;
            var averageOfVolInSideway = today.MAPrice(histories, numberOfSideway);

            return todayVol > averageOfVolInSideway * 1.05M;
        }

        public static bool IsPriceDown(this StockSymbolHistory today, List<StockSymbolHistory> histories, int numberOfSideway)
        {
            var todayVol = today.C;
            var averageOfVolInSideway = today.MAPrice(histories, numberOfSideway);

            return todayVol * 1.05M < averageOfVolInSideway;
        }


        public static bool IsVolUp(this StockSymbolHistory today, List<StockSymbolHistory> histories, int numberOfSideway)
        {
            var todayVol = today.V;
            var averageOfVolInSideway = today.MASideway(histories, numberOfSideway);

            return todayVol > averageOfVolInSideway * 1.05M;
        }

        public static bool IsVolDown(this StockSymbolHistory today, List<StockSymbolHistory> histories, int numberOfSideway)
        {
            var todayVol = today.V;
            var averageOfVolInSideway = today.MASideway(histories, numberOfSideway);

            return todayVol * 1.05M < averageOfVolInSideway;
        }

        public static decimal MASideway(this StockSymbolHistory today, List<StockSymbolHistory> histories, int numberOfSideway)
        {
            return histories.Where(h => h.Date < today.Date).OrderByDescending(h => h.Date).Take(numberOfSideway).Sum(h => h.V) / numberOfSideway;
        }

        public static decimal MAPrice(this StockSymbolHistory today, List<StockSymbolHistory> histories, int numberOfSideway)
        {
            return histories.Where(h => h.Date < today.Date).OrderByDescending(h => h.Date).Take(numberOfSideway).Sum(h => h.C) / numberOfSideway;
        }

        public static bool DangBiCanhCaoGD1Tuan(this StockSymbolHistory checkingDate, List<StockSymbolHistory> histories)
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
        public static decimal LayGiaCuaPhienSau(this StockSymbolHistory checkingDate, List<StockSymbolHistory> histories, int T)
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
        public static decimal LayGiaCaoNhatCuaCacPhienSau(this StockSymbolHistory checkingDate, List<StockSymbolHistory> histories, int fromT, int toT)
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

        public static StockSymbolHistory LookingForLowestWithout2Percent(this StockSymbolHistory currentDateHistory, List<StockSymbolHistory> histories)//, StockSymbolHistory currentDateHistory)
        {
            if (currentDateHistory == null) return null;

            var history = histories.FirstOrDefault(h => h.Date == currentDateHistory.Date);
            if (history == null) return null;

            var currentDateToCheck = history.Date;
            var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(30).ToList();

            var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();

            return lowest;
        }

        public static StockSymbolHistory LookingForSecondLowestWithout2Percent(this StockSymbolHistory lowest, List<StockSymbolHistory> histories, StockSymbolHistory currentDateHistory)
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
    }

}
