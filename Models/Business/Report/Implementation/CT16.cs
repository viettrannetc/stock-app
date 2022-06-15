using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    /// <summary>
    /// CT16 = "Nến xác nhận đáy có Vol less than tb 20 phiên";
    /// </summary>
    //public class ReportFormularCT16 : IReportFormular
    //{
    //    public ReportFormularModel Calculation(string code, DateTime ngay, List<History> histories, List<StockSymbolTradingHistory> tradingHistories)
    //    {
    //        var result = new ReportFormularModel();

    //        histories = histories.OrderBy(s => s.Date).ToList();

    //        var history = histories.FirstOrDefault(h => h.Date == ngay);        //nến xác nhận đáy
    //        if (history == null) return null;

    //        var lowest = history.LookingForLowestWithout2Percent(histories);
    //        if (lowest == null) return null;

    //        var secondLowest = lowest.LookingForSecondLowestWithout2Percent(histories, history);
    //        if (secondLowest == null) return null;
    //        var vol20Phien = history.VOL(histories, -20);

    //        var dk2 = history.V < vol20Phien;
    //        var dk3 = histories.Where(h => h.Date < history.Date).OrderByDescending(h => h.Date).First().ID == secondLowest.ID; //nến xác nhận đáy là ngày ngay sau đáy 2
            
    //        if (dk2 && dk3)
    //        {
    //            result.Name = ConstantData.CT16;
    //            result.Price = history.C;
    //        }

    //        return string.IsNullOrEmpty(result.Name)
    //            ? null
    //            : result;
    //    }

    //}
}
