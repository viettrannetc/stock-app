using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Prediction
{
    public class PredictionModel
    {
        public PredictionModel()
        {
            Prediction = new List<PredictionByUserModel>();
        }

        public string Name { get; set; }
        public List<PredictionByUserModel> Prediction { get; set; }
    }

    public class PredictionResultModel
    {
        public PredictionResultModel()
        {
            Details = new List<PredictionResultDetailsModel>();
        }
        public string Username { get; set; }
        public List<PredictionResultDetailsModel> Details { get; set; }

        public decimal Rate { get; set; }
        //public decimal Rate { get { return Details.Count(d => d.Result) / Details.Count(); } }


    }

    public class PredictionResultDetailsModel
    {
        public string Username { get; set; }
        public string Code { get; set; }
        public DateTime Ngay { get; set; }
        public bool Result { get; set; }
    }

    public class PredictionByUserModel
    {
        public PredictionByUserModel()
        {
            DuLieu = new List<string>();
            DuLieuDuocPhanTich = new List<PredictionDataModel>();
        }

        public List<string> DuLieu { get; set; }
        public DateTime Ngay { get; set; }
        public List<PredictionDataModel> DuLieuDuocPhanTich { get; set; }
        //public List<PredictionDataModel> DuLieuDuocPhanTich
        //{
        //    get
        //    {
        //        var rs = new List<PredictionDataModel>();
        //        foreach (var item in DuLieu)
        //        {
        //            var code = item.Split('-')[0];
        //            var suggestedPrice = item.Split('-')[1];

        //            rs.Add(new PredictionDataModel { Code = code, SuggestedPrice = decimal.Parse(suggestedPrice) });
        //        }

        //        return rs;
        //    }
        //}
    }

    public class PredictionDataModel
    {
        public string Code { get; set; }
        public decimal SuggestedPrice { get; set; }
    }
}
