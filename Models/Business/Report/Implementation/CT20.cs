﻿using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    /// <summary>
    /// CT20 = "Nến đáy 2 có CP less than OP less than CP * 1.02";
    /// Nến xác nhận đáy: ngày sau đáy 2 
    /// Nến đáy 2: đáy 2 
    /// </summary>
    public class ReportFormularCT20 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime ngay, List<History> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();

            histories = histories.OrderBy(s => s.Date).ToList();

            var history = histories.FirstOrDefault(h => h.Date == ngay);
            if (history == null) return null;

            var lowest = history.LookingForLowestWithout2Percent(histories);
            if (lowest == null) return null;

            var secondLowest = lowest.LookingForSecondLowestWithout2Percent(histories, history);
            if (secondLowest == null) return null;

            var vol20Phien = history.VOL(histories, -20);

            var dk1 = secondLowest.C < secondLowest.O;
            var dk2 = secondLowest.O < secondLowest.C * 1.02M;

            if (dk1 && dk2)
            {
                result.Name = ConstantData.CT20;
                result.Price = history.C;
            }

            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }

    }
}
