using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business.Report;
using DotNetCoreSqlDb.Models.Business.Report.Implementation;
using DotNetCoreSqlDb.Common;
using DotNetCoreSqlDb.Models.Business;
using System.Data;
using System.Text;
using System.IO;
using DotNetCoreSqlDb.Models.Learning;
using CSharpItertools;

namespace DotNetCoreSqlDb.Controllers
{
    public class LearningController : Controller
    {
        private readonly MyDatabaseContext _context;


        public LearningController(MyDatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Merge all raw data from excel files
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Merge()
        {
            var folder = ConstantPath.Path;
            var g = new Guid();
            var masterFile = $@"{folder}{g}.xlsx";

            folder.Merge();

            return true;
        }

        /// <summary>
        /// Merge all raw data from excel files
        /// </summary>
        /// <returns></returns>
        public async Task<bool> MergeV2()
        {
            var folder = ConstantPath.Path;
            folder.MergeBigFiles(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });

            return true;
        }

        /// <summary>
        /// Build learning data
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<LearningDataResponseModel> BuildRaw([Bind("FileName,Columns, Condition, MeasureColumn")] LearningDataRequestModel requestModel)
        {
            /*
             * READ Excel file: 500K data for instance
             * Get all data from column with true/false data and the result of the expectation (T3 lãi hay ko)
             * Format: 
             *      + Condition1-value - true                                           :single condition
             *      + Condition2-value - false                                          :single condition
             *      + Condition3-value - true                                           :single condition
             *      + Condition1-value,Condition2-value - true                          :combination of them
             *      + Condition1-value,Condition2-value,Condition3-value - true         :combination of them
             * Example:
             *      + Đáy 2-True - true
             *      + Hum Nay Vol Tang-True - true
             *      + Đáy 2-True,Hum Nay Vol Tang-True - true
             * 
             * Foreach all elements to see how many
             *      + duplicated items on true/false -> success ratio
             *      
             * Extract to the file, it will help the machine to learn
             * 
            */

            var condition = new LearningDataConditionModel();// new Dictionary<EnumExcelColumnModel, bool>();
            if (!string.IsNullOrEmpty(requestModel.Condition))
            {
                var conditions = requestModel.Condition.Split(',');
                foreach (var item in conditions)
                {
                    var columnName = item.Split('=')[0];
                    var columnValue = item.Split('=')[1];
                    Enum.TryParse(columnName.Trim(), out EnumExcelColumnModel myStatus);
                    condition.Condition.Add(myStatus, ConstantData.Condition.Contains(columnValue.Trim().ToString()) ? true : false);
                }
            }

            var columnsArray = new List<EnumExcelColumnModel>();
            foreach (var item in requestModel.Columns[0].Split(','))
            {
                Enum.TryParse(item.Trim(), out EnumExcelColumnModel myStatus);
                columnsArray.Add(myStatus);
            }

            string pathExcel = $"{ConstantPath.Path}{requestModel.FileName}.xlsx";
            var data = pathExcel.ReadFromExcel();

            Enum.TryParse(requestModel.MeasureColumn.Trim(), out EnumExcelColumnModel measureColumn);
            var result = data.ExportTo(measureColumn, condition, columnsArray.ToArray());

            return result;


        }

        /// <summary>
        /// Start learning data
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Learn()
        {
            /*
             * Based on the learning data, the machine will learn and try to form the correct format
             * 
            */

            return View(await _context.Todo.ToListAsync());
        }

        /// <summary>
        /// Predict the future based on the historical data (learning data)
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Predict(DateTime date)
        {
            /*
             * 
             * 
            */

            var a = new string[] { "A", "B", "C", "D" };
            var itertools = new Itertools();

            var t1 = itertools.Combinations(a, 2);

            return View(await _context.Todo.ToListAsync());
        }

        public async Task<bool> Build(string code, DateTime? startFrom, DateTime? toDate)
        {
            try
            {
                var reportData = new ReportModel();

                if (startFrom == null)
                {
                    startFrom = _context.StockSymbolHistory
                        .Where(ss => ss.StockSymbol == code)
                        .OrderBy(ss => ss.Date)
                        .Skip(60)
                        .First()
                        .Date;
                }

                if (toDate == null)
                {
                    toDate = _context.StockSymbolHistory
                        .Where(ss => ss.StockSymbol == code)
                        .OrderByDescending(ss => ss.Date)
                        .Skip(3)
                        .First()
                        .Date;
                }

                var filename = $"{code}-{startFrom.Value.Year}-{startFrom.Value.Month}-{startFrom.Value.Day}-To-{toDate.Value.Year}-{toDate.Value.Month}-{toDate.Value.Day}";

                bool contains = Directory.EnumerateFiles(ConstantPath.Path).Any(f => f.IndexOf(filename, StringComparison.OrdinalIgnoreCase) > 0);
                if (contains) return true;

                var historiesInPeriodOfTimeByStockCode = await _context.StockSymbolHistory
                        .Where(ss => ss.StockSymbol == code && ss.Date >= startFrom.Value.AddDays(-60))
                        .OrderByDescending(ss => ss.Date)
                        .ToListAsync();

                var expectedT3 = historiesInPeriodOfTimeByStockCode.Where(h => h.Date > toDate).OrderBy(h => h.Date).Take(3).LastOrDefault();

                if (expectedT3 != null)
                    historiesInPeriodOfTimeByStockCode = historiesInPeriodOfTimeByStockCode.Where(h => h.Date <= expectedT3.Date).ToList();

                var dates = await _context.StockSymbolHistory.OrderByDescending(s => s.Date)
                    .Where(s => s.StockSymbol == code && s.Date > startFrom.Value.AddDays((30 * 8) * -1))
                    .ToListAsync();

                await ExecuteEachThread(startFrom.Value, code, historiesInPeriodOfTimeByStockCode, reportData, dates, toDate.Value);

                var specificFilename = $"{filename}-{Guid.NewGuid().ToString()}";

                reportData.ConvertToDataTable().WriteToExcel($"{ConstantPath.Path}{specificFilename}.xlsx");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task ExecuteEachThread(DateTime startFrom, string code, List<StockSymbolHistory> historiesInPeriodOfTimeByStockCode,
            ReportModel result,
            List<StockSymbolHistory> dates, DateTime toDate)
        {
            var patternOnsymbol = new PatternBySymbolResponseModel();
            patternOnsymbol.StockCode = code;

            var orderedHistoryByStockCode = historiesInPeriodOfTimeByStockCode
                .Where(ss => ss.StockSymbol == code)
                .OrderBy(s => s.Date)
                .ToList();

            var latestDate = orderedHistoryByStockCode.OrderByDescending(h => h.Date).FirstOrDefault();
            var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(orderedHistoryByStockCode);

            if (!biCanhCao)
            {
                var orderedHistoryByStockCodeFromStartDate = orderedHistoryByStockCode.Where(h => h.Date >= startFrom).ToList();

                Parallel.ForEach(orderedHistoryByStockCodeFromStartDate, async history =>
                {
                    await Test(startFrom, code, orderedHistoryByStockCode, history, result, dates, toDate);
                });
            }
        }

        private async Task Test(DateTime startFrom, string code,
            List<StockSymbolHistory> orderedHistoryByStockCode,
            StockSymbolHistory history,
            ReportModel result,
            List<StockSymbolHistory> dates,
            DateTime toDate)
        {
            if (history.Date < startFrom) return;

            if (history.Date > toDate) return;

            var histories = orderedHistoryByStockCode.Where(h => h.Date <= history.Date).ToList();

            ReportStockModel stockData = new ReportStockModel();
            stockData.Date = history.Date;
            stockData.Code = code;
            stockData.Price = history.C;
            stockData.Vol = history.V;
            stockData.PriceT3 = history.LayGiaCuaPhienSau(orderedHistoryByStockCode, 3);
            stockData.HPriceT4T10 = history.LayGiaCaoNhatCuaCacPhienSau(orderedHistoryByStockCode, 4, 10);

            var dk01 = new ReportFormularCT1().Calculation(code, history.Date, dates, null);
            var dk02 = new ReportFormularCT2().Calculation(code, history.Date, histories, null);
            var dk03 = new ReportFormularCT3().Calculation(code, history.Date, histories, null);
            var dk04 = new ReportFormularCT4().Calculation(code, history.Date, histories, null);
            var dk05 = new ReportFormularCT5().Calculation(code, history.Date, histories, null);
            var dk06 = new ReportFormularCT6().Calculation(code, history.Date, histories, null);
            var dk07 = new ReportFormularCT7().Calculation(code, history.Date, histories, null);
            var dk08 = new ReportFormularCT8().Calculation(code, history.Date, histories, null);
            var dk09 = new ReportFormularCT9().Calculation(code, history.Date, histories, null);
            var dk10 = new ReportFormularCT10().Calculation(code, history.Date, histories, null);
            var dk11 = new ReportFormularCT11().Calculation(code, history.Date, histories, null);
            var dk12 = new ReportFormularCT12().Calculation(code, history.Date, orderedHistoryByStockCode, null);

            var dk13 = new ReportFormularCT13().Calculation(code, history.Date, histories, null);
            var dk14 = new ReportFormularCT14().Calculation(code, history.Date, histories, null);
            var dk15 = new ReportFormularCT15().Calculation(code, history.Date, histories, null);
            //var dk16 = new ReportFormularCT16().Calculation(code, history.Date, histories, null);
            var dk17 = new ReportFormularCT17().Calculation(code, history.Date, histories, null);
            var dk18 = new ReportFormularCT18().Calculation(code, history.Date, histories, null);
            //var dk19 = new ReportFormularCT19().Calculation(code, history.Date, histories, null);
            var dk20 = new ReportFormularCT20().Calculation(code, history.Date, histories, null);
            var dk21 = new ReportFormularCT21().Calculation(code, history.Date, histories, null);
            var dk22 = new ReportFormularCT22().Calculation(code, history.Date, orderedHistoryByStockCode, null);

            //var dk23 = new ReportFormularCT23().Calculation(code, history.Date, histories, null);
            //var dk24 = new ReportFormularCT24().Calculation(code, history.Date, histories, null);

            stockData.Formulars
                .Plus(dk01)
                .Plus(dk02)
                .Plus(dk03)
                .Plus(dk04)
                .Plus(dk05)
                .Plus(dk06)
                .Plus(dk07)
                .Plus(dk08)
                .Plus(dk09)
                .Plus(dk10)
                .Plus(dk11)
                .Plus(dk12)
                .Plus(dk13)
                .Plus(dk14)
                .Plus(dk15)
                //.Plus(dk16)
                .Plus(dk17)
                .Plus(dk18)
                //.Plus(dk19)
                .Plus(dk20)
                .Plus(dk21)
                .Plus(dk22)
                //.Plus(dk23)
                //.Plus(dk24)
                ;

            result.Stocks.Add(stockData);
        }

        /// <summary>
        /// Xem xét mẫu có tỉ lệ >100
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public async Task<List<string>> Consider([Bind("FileName,Columns,Condition,ExpectedPercentage,minMatchedPattern,MeasureColumn")] LearningDataRequestModel requestModel)
        {
            var condition = new LearningDataConditionModel();// new Dictionary<EnumExcelColumnModel, bool>();
            if (!string.IsNullOrEmpty(requestModel.Condition))
            {
                var conditions = requestModel.Condition.Split(',');
                foreach (var item in conditions)
                {
                    var columnName = item.Split('=')[0];
                    var columnValue = item.Split('=')[1];
                    Enum.TryParse(columnName.Trim(), out EnumExcelColumnModel myStatus);
                    condition.Condition.Add(myStatus, ConstantData.Condition.Contains(columnValue.Trim().ToString()) ? true : false);
                }
            }

            var columnsArray = new List<EnumExcelColumnModel>();
            foreach (var item in requestModel.Columns[0].Split(','))
            {
                Enum.TryParse(item.Trim(), out EnumExcelColumnModel myStatus);
                columnsArray.Add(myStatus);
            }

            string pathExcel = $"{ConstantPath.Path}{requestModel.FileName}.xlsx";
            var data = pathExcel.ReadFromExcel();

            Enum.TryParse(requestModel.MeasureColumn.Trim(), out EnumExcelColumnModel measureColumn);
            var result = data.ExportToString(requestModel.minMatchedPattern, requestModel.ExpectedPercentage, measureColumn, condition, columnsArray.ToArray());

            return result;
        }

        /*
         * Ma
         * Tỉ lệ   (100% for example)
         * Số lần  (min 3 for instance)
         * Pattern (min 3-matched for instance)
         * Thời gian xảy ra ????
         */
    }
}
