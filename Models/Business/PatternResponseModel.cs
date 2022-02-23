using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Models.Business
{
    public class PatternWeekResearchModel : StockSymbolHistory
    {
        public PatternWeekResearchModel()
        {
        }

        public int Week { get; set; }
    }

    public class PatternResponseModel
    {
        public PatternResponseModel()
        {
            Symbols = new List<PatternBySymbolResponseModel>();
            SymbolCodes = new List<string>();
        }
        public string PatternName { get; set; }
        public decimal SuccessRate { get; set; }
        public List<PatternBySymbolResponseModel> Symbols { get; set; }
        public List<string> SymbolCodes { get; set; }
    }

    public class PatternBySymbolResponseModel
    {
        public PatternBySymbolResponseModel()
        {
            Details = new List<PatternDetailsResponseModel>();
        }
        public string StockCode { get; set; }
        public decimal SuccessRate { get; set; }
        public List<PatternDetailsResponseModel> Details { get; set; }
    }

    public class PatternDetailsResponseModel
    {
        public DateTime ConditionMatchAt { get; set; }
        public dynamic MoreInformation { get; set; }
    }
}
