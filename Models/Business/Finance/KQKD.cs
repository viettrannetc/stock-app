using Newtonsoft.Json;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Models.Business.Finance
{
    public class KQKD
    {
        [JsonProperty(Order = 0, PropertyName = "0")]
        public List<FinanceByTimeModel> Time { get; set; }

        //[JsonProperty(Order = 1, PropertyName = "1")]
        //public FinanceByDataModel Data { get; set; }
    }

    public class FinanceByTimeModel
    {
        public int YearPeriod { get; set; }
        public int Quarter { get; set; }
        public string TermCode { get; set; }
        public string PeriodBegin { get; set; }
        /// <summary>
        /// 202103
        /// </summary>
        public string PeriodEnd { get; set; }
    }

    //public class FinanceByDataModel
    //{
    //    [JsonProperty(PropertyName = "Kết quả kinh doanh")]
    //    public List<FinanceByDetailDataModel> kqkd { get; set; }

    //    [JsonProperty(PropertyName = "Cơ cấu Chi phí")]
    //    public List<FinanceByDetailDataModel> cccp { get; set; }

    //    [JsonProperty(PropertyName = "Kết quả kinh doanh")]
    //    public List<FinanceByDetailDataModel> kqkd { get; set; }

    //    [JsonProperty(PropertyName = "Cơ cấu Chi phí")]
    //    public List<FinanceByDetailDataModel> cccp { get; set; }
    //    [JsonProperty(PropertyName = "Kết quả kinh doanh")]
    //    public List<FinanceByDetailDataModel> kqkd { get; set; }

    //    [JsonProperty(PropertyName = "Cơ cấu Chi phí")]
    //    public List<FinanceByDetailDataModel> cccp { get; set; }
    //    [JsonProperty(PropertyName = "Kết quả kinh doanh")]
    //    public List<FinanceByDetailDataModel> kqkd { get; set; }

    //    [JsonProperty(PropertyName = "Cơ cấu Chi phí")]
    //    public List<FinanceByDetailDataModel> cccp { get; set; }
    //    [JsonProperty(PropertyName = "Kết quả kinh doanh")]
    //    public List<FinanceByDetailDataModel> kqkd { get; set; }

    //    [JsonProperty(PropertyName = "Cơ cấu Chi phí")]
    //    public List<FinanceByDetailDataModel> cccp { get; set; }
    //    [JsonProperty(PropertyName = "Kết quả kinh doanh")]
    //    public List<FinanceByDetailDataModel> kqkd { get; set; }

    //    [JsonProperty(PropertyName = "Cơ cấu Chi phí")]
    //    public List<FinanceByDetailDataModel> cccp { get; set; }
    //}

    public class FinanceByDetailDataModel
    {
        /// <summary>
        /// "Name": "1. Doanh thu bán hàng và cung cấp dịch vụ ",
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// "NameEn": "1. Revenue",
        /// </summary>
        public string NameEn { get; set; }
        /// <summary>
        /// Value1 = Row1; Value1 = dữ liệu mới nhất của lần lấy
        /// </summary>
        public decimal? Value1 { get; set; }
        public decimal? Value2 { get; set; }
        public decimal? Value3 { get; set; }
        /// <summary>
        /// Value4 = Row4; Value1 = dữ liệu cũ nhất của lần lấy
        /// </summary>
        public decimal? Value4 { get; set; }

        ///// <summary>
        ///// "ReportComponentName": "Kết quả kinh doanh",
        ///// </summary>
        //public string ReportComponentName { get; set; }
        ///// <summary>
        ///// "ReportComponentNameEn": "Income Statement",
        ///// </summary>
        //public string ReportComponentNameEn { get; set; }
        //public decimal Value { get; set; }
    }
}
