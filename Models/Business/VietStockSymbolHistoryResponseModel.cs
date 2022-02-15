using System.Collections.Generic;

namespace DotNetCoreSqlDb.Models.Business
{
	public class VietStockSymbolHistoryResponseModel
	{
		public VietStockSymbolHistoryResponseModel()
		{
			t = new List<int>();
			o = new List<decimal>();
			c = new List<decimal>();
			h = new List<decimal>();
			l = new List<decimal>();
			v = new List<decimal>();
		}
		/// <summary>
		/// Time
		/// </summary>
		public List<int> t { get; set; }
		/// <summary>
		/// Opening value
		/// </summary>
		public List<decimal> o { get; set; }
		/// <summary>
		/// closing value
		/// </summary>
		public List<decimal> c { get; set; }
		/// <summary>
		/// Highest value
		/// </summary>
		public List<decimal> h { get; set; }
		/// <summary>
		/// Lowest value
		/// </summary>
		public List<decimal> l { get; set; }
		/// <summary>
		/// Number of transaction
		/// </summary>
		public List<decimal> v { get; set; }
		/// <summary>
		/// ok - simplify inform the result
		/// </summary>
		public string s { get; set; }
	}
}
