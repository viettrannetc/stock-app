using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business
{
    
    public class PatternSellAndBuyBySymbolResponseModel
    {
        public PatternSellAndBuyBySymbolResponseModel()
        {
            Buy = new PatternSellAndBuyBySymbolDetailResponseModel();
            Sell = new PatternSellAndBuyBySymbolDetailResponseModel();
        }

        public PatternSellAndBuyBySymbolDetailResponseModel Buy { get; set; }
        public PatternSellAndBuyBySymbolDetailResponseModel Sell { get; set; }
    }

    public class PatternSellAndBuyBySymbolDetailResponseModel : PatternDetailResponseModel
    {
        public PatternSellAndBuyBySymbolDetailResponseModel()
        {
            Items = new List<PatternBySymbolResponseModel>();
        }
    }
}
