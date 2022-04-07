using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business
{
    public class PatternWeekResearchModel : StockSymbolHistory
    {
        public PatternWeekResearchModel()
        {
        }

        public int Week { get; set; }
        public DateTime DateInWeek { get; set; }
    }

    public class PatternResponseModel
    {
        public PatternResponseModel()
        {
            //BuyAndSell = new PatternSellAndBuyBySymbolResponseModel();
            TimTrendGiam = new PatternTimTrendGiamBySymbolResponseModel();
            TimDay2 = new PatternTimDay2BySymbolResponseModel();
            TimDay2Moi = new PatternTimDay2BySymbolResponseModel();
        }

        //public PatternSellAndBuyBySymbolResponseModel BuyAndSell { get; set; }
        public PatternTimDay2BySymbolResponseModel TimDay2Moi { get; set; }
        public PatternTimDay2BySymbolResponseModel TimDay2 { get; set; }
        public PatternTimTrendGiamBySymbolResponseModel TimTrendGiam { get; set; }
        public PatternTimDay2BySymbolResponseModel GiamSau { get; set; }
    }

    //public class PatternTimDay2BySymbolResponseModel : PatternDetailResponseModel
    //{
    //    public PatternTimDay2BySymbolResponseModel()
    //    {
    //        FailedItems = new List<PatternIsFailedBySymbolResponseModel>();
    //    }

    //    //public List<PatternBySymbolResponseModel> Symbols { get; set; }
    //    //public List<string> SymbolCodes { get; set; }
    //    public List<PatternIsFailedBySymbolResponseModel> FailedItems { get; set; }
    //}

    //public class PatternSellAndBuyBySymbolResponseModel
    //{
    //    public PatternSellAndBuyBySymbolResponseModel()
    //    {
    //        Buy = new PatternSellAndBuyBySymbolDetailResponseModel();
    //        Sell = new PatternSellAndBuyBySymbolDetailResponseModel();
    //    }

    //    public PatternSellAndBuyBySymbolDetailResponseModel Buy { get; set; }
    //    public PatternSellAndBuyBySymbolDetailResponseModel Sell { get; set; }
    //}

    //public class PatternSellAndBuyBySymbolDetailResponseModel : PatternDetailResponseModel
    //{
    //    public PatternSellAndBuyBySymbolDetailResponseModel()
    //    {
    //        Items = new List<PatternBySymbolResponseModel>();
    //    }
    //    //public decimal SuccessRate
    //    //{
    //    //    get
    //    //    {
    //    //        if (Items.Any())
    //    //        {
    //    //            var details = Items.SelectMany(i => i.Details).ToList();
    //    //            var availableDetails = details.Where(d => !string.IsNullOrEmpty(d.MoreInformation.RealityExpectation));
    //    //            var count = availableDetails.Count();
    //    //            if (count > 0)
    //    //            {
    //    //                var successNumber = availableDetails.Count(d => d.MoreInformation.RealityExpectation == "true");
    //    //                return (decimal)successNumber / (decimal)count;
    //    //            }
    //    //        }
    //    //        return 0;
    //    //    }
    //    //}

    //    //public List<string> SymbolCodes { get { return Items.Select(i => i.StockCode).ToList(); } }
    //    //public List<PatternBySymbolResponseModel> Items { get; set; }
    //}

    public class PatternDetailResponseModel
    {
        public PatternDetailResponseModel()
        {
            Items = new List<PatternBySymbolResponseModel>();
        }
        public decimal SuccessRate
        {
            get
            {
                if (Items.Any())
                {
                    var details = Items.SelectMany(i => i.Details).ToList();
                    var availableDetails = details.Where(d => !string.IsNullOrEmpty(d.MoreInformation.RealityExpectation));
                    var count = availableDetails.Count();
                    if (count > 0)
                    {
                        var successNumber = availableDetails.Count(d => d.MoreInformation.RealityExpectation == "true");
                        return (decimal)successNumber / (decimal)count;
                    }
                }
                return 0;
            }
        }

        public string SymbolCodes { get { return string.Join(",", Items.Select(i => i.StockCode)); } }
        public List<PatternBySymbolResponseModel> Items { get; set; }
    }



    public class PatternIsFailedBySymbolResponseModel
    {
        public string StockCode { get; set; }
        public DateTime Date { get; set; }
    }

    public class PatternBySymbolResponseModel
    {
        public PatternBySymbolResponseModel()
        {
            Details = new List<PatternDetailsResponseModel>();
        }
        public string StockCode { get; set; }
        public decimal SuccessRate
        {
            get
            {
                if (Details.Any())
                {
                    var availableDetails = Details.Where(d => !string.IsNullOrEmpty(d.MoreInformation.RealityExpectation));
                    var count = availableDetails.Count();
                    if (count > 0)
                    {
                        var successNumber = availableDetails.Count(d => d.MoreInformation.RealityExpectation == "true");
                        return (decimal)successNumber / (decimal)count;
                    }
                }

                return 0;
            }
        }
        public List<PatternDetailsResponseModel> Details { get; set; }
    }

    public class PatternDetailsResponseModel
    {
        public DateTime ConditionMatchAt { get; set; }
        public dynamic MoreInformation { get; set; }
    }
}
