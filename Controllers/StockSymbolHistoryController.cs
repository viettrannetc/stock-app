﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Common;

namespace DotNetCoreSqlDb.Controllers
{
    public class StockSymbolHistoryController : Controller
    {
        private readonly MyDatabaseContext _context;

        public StockSymbolHistoryController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: StockSymbolHistory
        public async Task<List<StockSymbolHistory>> Index()
        {
            return await _context.StockSymbolHistory.ToListAsync();
        }

        // GET: https://localhost:44359/StockSymbolHistory/Details?code=A32
        [HttpGet]
        public async Task<List<StockSymbolHistory>> Details(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var result = await _context.StockSymbolHistory.Where(m => m.StockSymbol == code).ToListAsync();
            if (result == null)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Example: "https://api.vietstock.vn/ta/history?symbol=VIC&resolution=D&from=1609459200&to=1644796800";
        /// </summary>
        /// <params>{0}: symbol code</params>
        /// <params>{1}: resolution = D</params>
        /// <params>{2}: from: int from php code</params>
        /// <params>{3}: to: int from php code</params>
        public const string VietStock_GetDetailsBySymbolCode = "https://api.vietstock.vn/ta/history?symbol={0}&resolution={1}&from={2}&to={3}";


        // GET: StockSymbolHistory/Create
        // Form Data:
        //      code: A32
        //      from: 01-01-1970  (12 AM as default)
        //      to: 15-02-2022    (12 AM as default)
        [HttpPost]
        public async Task<string> Create()
        {
            var restService = new RestServiceHelper();
            var huyNiemYet = new List<string>();
            huyNiemYet.Add("KSK");
            huyNiemYet.Add("TRT");
            huyNiemYet.Add("ABR");
            huyNiemYet.Add("GTN");
            huyNiemYet.Add("FUCTVGF2");

            var allSymbols = await _context.StockSymbol
                .OrderByDescending(s => s._sc_)
                .ToListAsync();

            allSymbols = allSymbols.Where(s => !huyNiemYet.Contains(s._sc_)).ToList();

            var result = new List<StockSymbolHistory>();
            var t1 = _context.StockSymbolHistory.Where(c => c.StockSymbol == "A32").OrderByDescending(r => r.Date).FirstOrDefault();
            var currentLatestDate = t1 == null ? new DateTime(2000, 1, 1) : t1.Date;
            var from = currentLatestDate;
            var to = DateTime.Now.WithoutHours();

            var service = new Service();
            await service.GetV(result, allSymbols, from, to, from, 0);

            result = result.Where(r => r.Date > currentLatestDate).ToList();

            if (result.Any())
            {
                await _context.StockSymbolHistory.AddRangeAsync(result);
                await _context.SaveChangesAsync();
            }

            return "true";
        }
    }
}
