using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Common;

namespace DotNetCoreSqlDb.Controllers
{
    public class StockController : Controller
    {
        private readonly MyDatabaseContext _context;

        public StockController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: Stock
        public async Task<List<string>> Index()
        {
            return await _context.StockSymbol.Select(s => s._sc_).ToListAsync();
        }

        // GET: Stock
        public async Task<List<string>> Vol(int minVol)
        {
            var t1 = await _context.StockSymbolHistory.Where(h => h.V >= minVol && h.Date == new DateTime(2022, 3, 10))
                .Select(h => h.StockSymbol).Distinct().ToListAsync();
            return t1;
        }

        // GET: Stock/Details/5
        public async Task<IActionResult> Details(string stockCode)
        {
            if (string.IsNullOrWhiteSpace(stockCode))
            {
                return NotFound();
            }

            var todo = await _context.StockSymbol.FirstOrDefaultAsync(m => m._sc_ == stockCode);
            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }



        /// <summary>
		/// Example: "https://api.vietstock.vn/ta/history?symbol=VIC&resolution=D&from=1609459200&to=1644796800";
		/// </summary>
		/// <params>{0}: symbol code</params>
		/// <params>{1}: resolution = D</params>
		/// <params>{2}: from: int from php code</params>
		/// <params>{3}: to: int from php code</params>
		public const string VietStock_GetAllSymbols = "https://api.vietstock.vn/finance/sectorInfo_v2?sectorID=0&catID=0&capitalID=0&languageID=1";

        // GET: Todos/Create
        [HttpPost]
        public async Task<bool> Create()
        {
            var restService = new RestServiceHelper();
            var allSharePointsObjects = await restService.Get<List<StockSymbol>>(VietStock_GetAllSymbols);

            var existingOnes = await _context.StockSymbol.ToListAsync();
            _context.StockSymbol.RemoveRange(existingOnes);

            await _context.StockSymbol.AddRangeAsync(allSharePointsObjects);

            await _context.SaveChangesAsync();

            return true;
        }


    }
}
