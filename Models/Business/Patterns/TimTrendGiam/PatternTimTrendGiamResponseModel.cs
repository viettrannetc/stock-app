using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business
{
    public class PatternTimTrendGiamBySymbolResponseModel : PatternDetailResponseModel
    {
        public PatternTimTrendGiamBySymbolResponseModel()
        {
            FailedItems = new List<PatternIsFailedBySymbolResponseModel>();
        }

        public List<PatternIsFailedBySymbolResponseModel> FailedItems { get; set; }
    }
}
