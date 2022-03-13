﻿using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    /// <summary>
    /// Đáy 2 
    /// </summary>
    public class ReportFormularCT2 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime ngay, List<StockSymbolHistory> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();

            histories = histories.OrderBy(s => s.Date).ToList();

            var history = histories.FirstOrDefault(h => h.Date == ngay);
            if (history == null) return null;

            var currentDateToCheck = history.Date;
            var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(30).ToList();

            //TODO: lowest & 2nd lowest can be reused to improve performance
            var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
            if (lowest == null) return null;

            var secondLowest = LookingForSecondLowest(histories, lowest, history);
            if (secondLowest == null) return null;

            var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(30).ToList();
            var highest = previousDaysForHigestFromLowest.OrderByDescending(h => h.C).FirstOrDefault();
            if (highest == null) return null;

            var dk1 = highest.C * 0.85M >= lowest.C;
            //var dk2 = history.C >= secondLowest.C * 1.02M;
            var dk3 = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).First().ID == secondLowest.ID;
            var dk4 = lowest.C * 1.15M >= secondLowest.C;

            if (dk1 /*&& dk2*/ && dk3 && dk4) //basically we should start buying
            {
                result.Name = ConstantData.CT02;
                result.Price = history.C;
            }
                        
            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }

        private StockSymbolHistory LookingForSecondLowest(List<StockSymbolHistory> histories, StockSymbolHistory lowest, StockSymbolHistory currentDateHistory)
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
                    dkSub2 = rangesFromLowestTo2ndLowest.Any(r => r.C > nextPhien.C) && nextPhien.C > secondLowestAssumption.C; // (secondLowestAssumption.C * 1.02M);
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
