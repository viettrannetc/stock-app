using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreSqlDb.Models
{
    public class MyDatabaseContext : DbContext
    {
        public MyDatabaseContext (DbContextOptions<MyDatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Todo> Todo { get; set; }
        public DbSet<StockSymbol> StockSymbol { get; set; }
        public DbSet<StockSymbolHistory> StockSymbolHistory { get; set; }
        /// <summary>
        /// https://api-finance-t19.24hmoney.vn/v1/web/stock/transaction-list-ssi?device_id=web&device_name=INVALID&device_model=Windows+10&network_carrier=INVALID&connection_type=INVALID&os=Chrome&os_version=98.0.4758.102&app_version=INVALID&access_token=INVALID&push_token=INVALID&locale=vi&symbol=VCB&page=1&per_page=2000
        /// </summary>
        public DbSet<StockSymbolTradingHistory> StockSymbolTradingHistory { get; set; }

        public DbSet<StockSymbolFinanceHistory> StockSymbolFinanceHistory { get; set; }
        //public DbSet<StockSymbolKQKD> StockSymbolKQKD { get; set; }
        //public DbSet<StockSymbolCDKT> StockSymbolCDKT { get; set; }
        //public DbSet<StockSymbolCSTC> StockSymbolCSTC { get; set; }
        //public DbSet<StockSymbolLCTT> StockSymbolLCTT { get; set; }
        //public DbSet<StockSymbolCTKH> StockSymbolCTKH { get; set; }
    }
}
