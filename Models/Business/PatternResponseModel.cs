using System;

namespace DotNetCoreSqlDb.Models.Business
{
    public class PatternResponseModel
    {
        public string PatternName { get; set;}
        public string StockCode { get; set; }
        public DateTime ConditionMatchAt { get; set; }
        public dynamic MoreInformation { get; set; }

    }
}
