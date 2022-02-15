using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Models.Business
{
	public class VietStockSymbolHistoryResquestModel
	{
		public string code { get; set; }
		public DateTime from { get; set; }
		public DateTime to { get; set; }
	}
}
