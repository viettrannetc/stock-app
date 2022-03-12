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
        /// <summary>
        /// Laptop cty - @$"C:\Projects\Test\Stock-app\Data\Learning\Source\"
        /// Home       - C:\Users\Viet\Documents\GitHub\stock-app\Data\Testing\
        /// </summary>
        public const string path = @$"C:\Users\Viet\Documents\GitHub\stock-app\Data\Testing\";

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
            var folder = path;
            var g = new Guid();
            var masterFile = $@"{path}{g}.xlsx";

            folder.Merge(masterFile);

            return true;
        }

        /// <summary>
        /// Build learning data
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<LearningDataResponseModel> BuildRaw([Bind("FileName,Columns, MinCombination")] LearningDataRequestModel requestModel)
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

            var columnsArray = new List<EnumExcelColumnModel>();
            foreach (var item in requestModel.Columns[0].Split(','))
            {
                Enum.TryParse(item.Trim(), out EnumExcelColumnModel myStatus);
                columnsArray.Add(myStatus);
            }

            string pathExcel = $"{path}{requestModel.FileName}.xlsx";
            var data = pathExcel.ReadFromExcel();
            //Q or AB
            var test = data.ExportTo(requestModel.MinCombination, EnumExcelColumnModel.AB, columnsArray.ToArray());
            var groupBy = test.Data.GroupBy(d => d.Combination).ToDictionary(d => d.Key, d => d.ToList());

            var result = new LearningDataResponseModel();

            foreach (var item in groupBy)
            {
                var t = item.Value.Count;
                var s = item.Value.Count(t => t.Combination == item.Key && t.Result);
                var p = Math.Round((decimal)s / (decimal)t, 2) * 100;
                result.Pattern.Add(new LearningDataPatternResponseModel
                {
                    Pattern = item.Key,
                    Tile = p,
                    Tong = t
                });
            }

            result.Pattern = result.Pattern.OrderByDescending(r => r.Tile).ToList();

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

                bool contains = Directory.EnumerateFiles(path).Any(f => f.IndexOf(filename, StringComparison.OrdinalIgnoreCase) > 0);
                if (contains) return true;

                var historiesInPeriodOfTimeByStockCode =

                    await _context.StockSymbolHistory
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

                reportData.ConvertToDataTable().WriteToExcel($"{path}{specificFilename}.xlsx");

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

                Parallel.ForEach(orderedHistoryByStockCodeFromStartDate, history =>
                {
                    Test(startFrom, code, orderedHistoryByStockCode, history, result, dates, toDate);
                });
            }
        }

        private void Test(DateTime startFrom, string code,
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

            var dk1 = new ReportFormularCT1().Calculation(code, history.Date, dates, null);
            var dk2 = new ReportFormularCT2().Calculation(code, history.Date, histories, null);
            var dk3 = new ReportFormularCT3().Calculation(code, history.Date, histories, null);
            var dk4 = new ReportFormularCT4().Calculation(code, history.Date, histories, null);
            var dk5 = new ReportFormularCT5().Calculation(code, history.Date, histories, null);
            var dk6 = new ReportFormularCT6().Calculation(code, history.Date, histories, null);
            var dk7 = new ReportFormularCT7().Calculation(code, history.Date, histories, null);
            var dk8 = new ReportFormularCT8().Calculation(code, history.Date, histories, null);
            var dk9 = new ReportFormularCT9().Calculation(code, history.Date, histories, null);
            var dk10 = new ReportFormularCT10().Calculation(code, history.Date, histories, null);
            var dk11 = new ReportFormularCT11().Calculation(code, history.Date, histories, null);
            var dk12 = new ReportFormularCT12().Calculation(code, history.Date, orderedHistoryByStockCode, null);

            var dk13 = new ReportFormularCT13().Calculation(code, history.Date, histories, null);
            var dk14 = new ReportFormularCT14().Calculation(code, history.Date, histories, null);
            var dk15 = new ReportFormularCT15().Calculation(code, history.Date, histories, null);
            var dk16 = new ReportFormularCT16().Calculation(code, history.Date, histories, null);
            var dk17 = new ReportFormularCT17().Calculation(code, history.Date, histories, null);
            var dk18 = new ReportFormularCT18().Calculation(code, history.Date, histories, null);
            var dk19 = new ReportFormularCT19().Calculation(code, history.Date, histories, null);
            var dk20 = new ReportFormularCT20().Calculation(code, history.Date, histories, null);
            var dk21 = new ReportFormularCT21().Calculation(code, history.Date, histories, null);
            var dk22 = new ReportFormularCT22().Calculation(code, history.Date, orderedHistoryByStockCode, null);

            stockData.Formulars
                .Plus(dk1)
                .Plus(dk2)
                .Plus(dk3)
                .Plus(dk4)
                .Plus(dk5)
                .Plus(dk6)
                .Plus(dk7)
                .Plus(dk8)
                .Plus(dk9)
                .Plus(dk10)
                .Plus(dk11)
                .Plus(dk12)

                .Plus(dk13)
                .Plus(dk14)
                .Plus(dk15)
                .Plus(dk16)
                .Plus(dk17)
                .Plus(dk18)
                .Plus(dk19)
                .Plus(dk20)
                .Plus(dk21)
                .Plus(dk22)
                ;

            result.Stocks.Add(stockData);
        }
    }
}
