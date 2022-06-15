using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreSqlDb.Models.Database.Finance;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreSqlDb.Models
{
    public class MyDatabaseContext : DbContext
    {
        public MyDatabaseContext(DbContextOptions<MyDatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<History> History { get; set; }
        public DbSet<StockSymbol> StockSymbol { get; set; }
        //public DbSet<StockSymbolHistory> StockSymbolHistory { get; set; }
        /// <summary>
        /// https://api-finance-t19.24hmoney.vn/v1/web/stock/transaction-list-ssi?symbol=VCB&page=1&per_page=20000
        /// </summary>
        public DbSet<StockSymbolTradingHistory> StockSymbolTradingHistory { get; set; }

        public DbSet<StockSymbolFinanceHistory> StockSymbolFinanceHistory { get; set; }
        public DbSet<StockSymbolFinanceYearlyHistory> StockSymbolFinanceYearlyHistory { get; set; }

        /// <summary>
        /// https://api-finance-t19.24hmoney.vn/v2/web/stock/transaction-detail-by-price?symbol=ACB
        /// </summary>
        public DbSet<KLGDMuaBan> KLGDMuaBan { get; set; }
    }
}
