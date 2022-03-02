using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business
{
    public class PatternTimDay2BySymbolResponseModel : PatternDetailResponseModel
    {
        public PatternTimDay2BySymbolResponseModel()
        {
            FailedItems = new List<PatternIsFailedBySymbolResponseModel>();
        }

        public List<PatternIsFailedBySymbolResponseModel> FailedItems { get; set; }
    }

}
