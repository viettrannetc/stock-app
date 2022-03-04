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

        public static bool IsUpFromSideway(this StockSymbolHistory today, List<StockSymbolHistory> histories, int numberOfSideway)
        {
            var todayVol = today.V;
            var averageOfVolInSideway = today.MASideway(histories, numberOfSideway);

            return todayVol > averageOfVolInSideway * 2;
        }

        public static decimal MASideway(this StockSymbolHistory today, List<StockSymbolHistory> histories, int numberOfSideway)
        {
            return histories.Where(h => h.Date < today.Date).OrderByDescending(h => h.Date).Take(numberOfSideway).Sum(h => h.V) / numberOfSideway;
        }

    }

}
