using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business;

namespace DotNetCoreSqlDb.Controllers
{
    public class StockPatternController : Controller
    {
        private readonly MyDatabaseContext _context;

        public StockPatternController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: Stock
        public async Task<IActionResult> Index()
        {
            return View(await _context.StockSymbol.ToListAsync());
        }

        // GET: Stock/Details/5
        public async Task<IActionResult> Pattern(string pattern)
        {
            //https://stackoverflow.com/questions/8454974/c-sharp-net-equivalent-to-php-time
            switch (pattern)
            {
                case "aLien - Pattern 1":
                    //DK 1: Lowest today < Lowest in the last XXX day && Lowest today < 2nd Lowest in the last XXX day
                    //DK 2: C > L * 0.02
                    //DK3 = C <= O * 0.03;
                    //DK4 = V > MA(V, 21) * 1.5;
                    //Filter: DK1 && DK2 && DK3 && DK4
                    break;
                default:
                    break;
            }

            var result = new PatternResponseModel
            {
                PatternName = pattern,
                StockCode = "",
                ConditionMatchAt = DateTime.Now,
                MoreInformation = new {

                }
            };




            if (string.IsNullOrWhiteSpace(pattern))
            {
                return NotFound();
            }

            var todo = await _context.StockSymbol.FirstOrDefaultAsync(m => m.Description == pattern);
            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

    }
}
