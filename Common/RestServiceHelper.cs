//using Flurl.Http;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business.Finance;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DotNetCoreSqlDb.Common
{
    public class RestServiceHelper
    {
        private const string DataType = "application/json";

        public async Task<T> Get<T>(string url, bool? applyParseTwice = false,
            string token = "") where T : class
        {
            return await Hexecute<T>(url, "GET", applyParseTwice: applyParseTwice, token: token);
        }

        public async Task<T> Post<T>(string url, object data) where T : class
        {
            return await Hexecute<T>(url, "POST", JsonConvert.SerializeObject(data, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
        }

        public async Task<T> Put<T>(string url, object data) where T : class
        {
            return await Hexecute<T>(url, "PUT", JsonConvert.SerializeObject(data, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
        }

        public async Task<T> Delete<T>(string url) where T : class
        {
            return await Hexecute<T>(url, "DELETE");
        }

        private byte[] ReadFully(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private async Task<T> Hexecute<T>(string address, string method, string data = null, Stream streamData = null,
            string contentType = null, bool isStream = false, bool acceptJson = true,
            bool? applyParseTwice = false,
            string token = "") where T : class
        {
            var url = new Uri(address);
            var request = WebRequest.Create(url) as HttpWebRequest;

            //var encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(Configuration.Username + ":" + Configuration.Password));
            //request.Headers.Add("Authorization", "Basic " + encoded);

            if (address.Contains("fireant") && !string.IsNullOrEmpty(token))
                request.Headers.Add("cookie", token);

            if (address.Contains("vietstock"))
                request.Headers.Add("Referer", "https://stockchart.vietstock.vn");

            request.Method = method;
            request.ContentType = DataType;
            if (acceptJson)
            {
                request.Accept = DataType;
            }

            if (streamData != null)
            {
                var byteData = ReadFully(streamData);
                using (var postStream = await request.GetRequestStreamAsync())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }
            }

            if (data != null)
            {
                var byteData = Encoding.UTF8.GetBytes(data);

                using (var postStream = await request.GetRequestStreamAsync())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }
            }

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    var r = (HttpWebResponse)response;

                    var reader = new StreamReader(r.GetResponseStream());

                    var content = await reader.ReadToEndAsync();
                    if (content == null || content == "null") return null;
                    if (applyParseTwice == true)
                        return JsonConvert.DeserializeObject<T>(JsonConvert.DeserializeObject(content).ToString());
                    else
                        return JsonConvert.DeserializeObject<T>(content);
                }
            }
            catch (WebException ex)
            {
                return null;
            }
        }

        public async Task<List<StockSymbolFinanceHistory>> HexecuteVietStockPostman(
            string stockSymbol,
            FinanceType reportType,
            string yearInThePastFromNow,
            string address = "https://api.vietstock.vn/data/financeinfo")
        {
            var result = new List<StockSymbolFinanceHistory>();

            var client = new RestClient(address);
            //client.Timeout = -1;
            var request = new RestRequest(address, Method.Post);
            request.AddHeader("Host", "finance.vietstock.vn");
            request.AddHeader("Content-Length", "232");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Cookie", "_ga=GA1.2.713551826.1644247671; _ga=GA1.3.713551826.1644247671; dable_uid=43391554.1574785018857; dable_uid=43391554.1574785018857; AnonymousNotification=; language=vi-VN; Theme=Light; __gpi=UID=0000029fcfbf1e52:T=1647013809:RT=1647013809:S=ALNI_Mbnwz8R_W4WHz7DNQNBZDzQvsApJQ; ASP.NET_SessionId=m2egsnbcyuymr34zuoimieoo; __RequestVerificationToken=dNglG8kMDq09oaBufe3gxwAnLFhdRZGm0z13QEVZCEwzLmYZOoNuv2Am4tFL9_UOI5E3cJsN2C_MqWI75j6fZK0rhjuxNZHWN79YE3unhOI1; _gid=GA1.2.356828925.1648475526; vts_usr_lg=AA697487B6A84B1DFD5FBB731685026911EAED6A5739CF8D4B9E08AB9C0C39632E17B55D78783AD457047EA32EDDA6A072FB7E9BBE17EB11541DC9601607B128DB2F97013084ABB915F322D3BC20A2494DB4AC985AAAAA9025C809895DA74DCDDBBF6CF1B7241F5CEB8CF1BF16CE374D08435787C5CA97E8E0C5C53C810F0C85; vst_usr_lg_token=uUHaeIUDf0m7PjoQ5xSvaA==; finance_viewedstock=HUT,GIC,CIG,PVD,XMD,CEO,DGW,; _gat_gtag_UA_1460625_2=1; __gads=ID=3f7aab506428e100-22a2e49c64d1002e:T=1644247671:RT=1648483910:S=ALNI_MbD-QRlvz3Mc2X-6temXksp2u17ig; _gat=1");
            request.AddParameter("Code", stockSymbol);
            request.AddParameter("ReportType", reportType.ToString());
            request.AddParameter("ReportTermType", "2");
            request.AddParameter("Unit", "1000000000");
            request.AddParameter("Page", yearInThePastFromNow);
            request.AddParameter("PageSize", "1");
            request.AddParameter("__RequestVerificationToken", "xBn41_m3HO3gTm8p86LFvL667WRKMSx7TwBWZyQT9ZWzNkj9psjXFRybyk9a-W6N550n80WLix3hAFgDyZkAzHHcxTarL8HNnt4QMZsVTC-rF_vN4laUzXRMIjGhGR-C0");
            var response = await client.PostAsync(request);

            var test = response.Content;

            var jsonData = JsonConvert.DeserializeObject<List<dynamic>>(test);

            List<FinanceByTimeModel> lstTimeModel = JsonConvert.DeserializeObject<List<FinanceByTimeModel>>(jsonData[0].ToString());

            if (!lstTimeModel.Any()) return result;

            List<FinanceByDetailDataModel> criterials = new List<FinanceByDetailDataModel>();
            var t1 = JsonConvert.DeserializeObject<dynamic>(jsonData[1].ToString());

            try
            {
                switch (reportType)
                {
                    case FinanceType.KQKD:
                        if (t1.ContainsKey("Kết quả kinh doanh")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Kết quả kinh doanh"].ToString()));
                        break;
                    case FinanceType.CDKT://Cân đối kế toán
                        if (t1.ContainsKey("Cân đối kế toán")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Cân đối kế toán"].ToString()));
                        break;
                    case FinanceType.CSTC:
                        if (t1.ContainsKey("Cơ cấu Chi phí")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Cơ cấu Chi phí"].ToString()));
                        if (t1.ContainsKey("Cơ cấu Tài sản dài hạn")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Cơ cấu Tài sản dài hạn"].ToString()));
                        if (t1.ContainsKey("Cơ cấu Tài sản ngắn hạn")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Cơ cấu Tài sản ngắn hạn"].ToString()));
                        if (t1.ContainsKey("Nhóm chỉ số Dòng tiền")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Nhóm chỉ số Dòng tiền"].ToString()));
                        if (t1.ContainsKey("Nhóm chỉ số Hiệu quả hoạt động")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Nhóm chỉ số Hiệu quả hoạt động"].ToString()));
                        if (t1.ContainsKey("Nhóm chỉ số Sinh lợi")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Nhóm chỉ số Sinh lợi"].ToString()));
                        if (t1.ContainsKey("Nhóm chỉ số Thanh khoản")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Nhóm chỉ số Thanh khoản"].ToString()));
                        if (t1.ContainsKey("Nhóm chỉ số Tăng trưởng")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Nhóm chỉ số Tăng trưởng"].ToString()));
                        if (t1.ContainsKey("Nhóm chỉ số Đòn bẩy tài chính")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Nhóm chỉ số Đòn bẩy tài chính"].ToString()));
                        if (t1.ContainsKey("Nhóm chỉ số Định giá")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Nhóm chỉ số Định giá"].ToString()));
                        break;
                    case FinanceType.LCTT:
                        if (t1.ContainsKey("Lưu chuyển tiền tệ gián tiếp")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Lưu chuyển tiền tệ gián tiếp"].ToString()));
                        break;
                    case FinanceType.CTKH: //chỉ đi theo năm
                        criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(jsonData[1].ToString()));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{stockSymbol} - {reportType} - {yearInThePastFromNow} - {ex}");
            }


            foreach (FinanceByTimeModel time in lstTimeModel)
            {
                var year = time.YearPeriod;
                var quarter = reportType == FinanceType.CTKH
                    ? 4
                    : int.Parse(time.TermCode[1].ToString());

                foreach (var item in criterials)
                {
                    var quarterValue = quarter == 1
                            ? item.Value4
                            : quarter == 2
                                ? item.Value3
                                : quarter == 3
                                    ? item.Value2
                                    : item.Value1;

                    var yearValue = lstTimeModel.IndexOf(time) == 0
                        ? item.Value1
                        : lstTimeModel.IndexOf(time) == 1
                            ? item.Value2
                            : lstTimeModel.IndexOf(time) == 2
                                ? item.Value3
                                : lstTimeModel.IndexOf(time) == 3
                                    ? item.Value4
                                    : 0;

                    var d = new StockSymbolFinanceHistory
                    {
                        YearPeriod = year,
                        Quarter = quarter,
                        Name = item.Name,
                        NameEn = item.NameEn,
                        StockSymbol = stockSymbol,
                        Type = (int)reportType,
                        Value = reportType == FinanceType.CTKH
                            ? yearValue
                            : quarterValue
                    };

                    result.Add(d);
                }
            }

            return result;
        }


        public async Task<List<StockSymbolFinanceHistory>> HexecuteVietStockPostmanYearly(
            string stockSymbol,
            FinanceType reportType,
            string yearInThePastFromNow,
            string address = "https://api.vietstock.vn/data/financeinfo")
        {
            var result = new List<StockSymbolFinanceHistory>();

            var client = new RestClient(address);
            var request = new RestRequest(address, Method.Post);
            request.AddHeader("Host", "finance.vietstock.vn");
            request.AddHeader("Content-Length", "208");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Cookie", "_ga=GA1.2.713551826.1644247671; _ga=GA1.3.713551826.1644247671; dable_uid=43391554.1574785018857; dable_uid=43391554.1574785018857; AnonymousNotification=; language=vi-VN; Theme=Light; __gpi=UID=0000029fcfbf1e52:T=1647013809:RT=1647013809:S=ALNI_Mbnwz8R_W4WHz7DNQNBZDzQvsApJQ; ASP.NET_SessionId=m2egsnbcyuymr34zuoimieoo; __RequestVerificationToken=dNglG8kMDq09oaBufe3gxwAnLFhdRZGm0z13QEVZCEwzLmYZOoNuv2Am4tFL9_UOI5E3cJsN2C_MqWI75j6fZK0rhjuxNZHWN79YE3unhOI1; _gid=GA1.2.356828925.1648475526; vts_usr_lg=AA697487B6A84B1DFD5FBB731685026911EAED6A5739CF8D4B9E08AB9C0C39632E17B55D78783AD457047EA32EDDA6A072FB7E9BBE17EB11541DC9601607B128DB2F97013084ABB915F322D3BC20A2494DB4AC985AAAAA9025C809895DA74DCDDBBF6CF1B7241F5CEB8CF1BF16CE374D08435787C5CA97E8E0C5C53C810F0C85; vst_usr_lg_token=uUHaeIUDf0m7PjoQ5xSvaA==; finance_viewedstock=HUT,GIC,CIG,PVD,XMD,CEO,DGW,; _gat_gtag_UA_1460625_2=1; __gads=ID=3f7aab506428e100-22a2e49c64d1002e:T=1644247671:RT=1648483910:S=ALNI_MbD-QRlvz3Mc2X-6temXksp2u17ig; _gat=1");
            request.AddParameter("Code", stockSymbol);
            request.AddParameter("ReportType", reportType.ToString());
            request.AddParameter("ReportTermType", "1");
            request.AddParameter("Unit", "1000000");
            request.AddParameter("Page", yearInThePastFromNow);
            request.AddParameter("PageSize", "1");
            request.AddParameter("__RequestVerificationToken", "xBn41_m3HO3gTm8p86LFvL667WRKMSx7TwBWZyQT9ZWzNkj9psjXFRybyk9a-W6N550n80WLix3hAFgDyZkAzHHcxTarL8HNnt4QMZsVTC-rF_vN4laUzXRMIjGhGR-C0");
            var response = await client.PostAsync(request);

            var test = response.Content;

            var jsonData = JsonConvert.DeserializeObject<List<dynamic>>(test);

            List<FinanceByTimeModel> lstTimeModel = JsonConvert.DeserializeObject<List<FinanceByTimeModel>>(jsonData[0].ToString());

            if (!lstTimeModel.Any()) return result;

            List<FinanceByDetailDataModel> criterials = new List<FinanceByDetailDataModel>();
            var t1 = JsonConvert.DeserializeObject<dynamic>(jsonData[1].ToString());

            try
            {
                switch (reportType)
                {
                    case FinanceType.BCTQ:
                        if (t1.ContainsKey("Kết quả kinh doanh")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Kết quả kinh doanh"].ToString()));
                        if (t1.ContainsKey("Cân đối kế toán")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Cân đối kế toán"].ToString()));
                        if (t1.ContainsKey("Chỉ số tài chính")) criterials.AddRange(JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Chỉ số tài chính"].ToString()));
                        break;
                    case FinanceType.LCTT:
                        if (t1.ContainsKey("Lưu chuyển tiền tệ gián tiếp"))
                        {
                            var parsedData = (List<FinanceByDetailDataModel>)JsonConvert.DeserializeObject<List<FinanceByDetailDataModel>>(t1["Lưu chuyển tiền tệ gián tiếp"].ToString());
                            var expected = parsedData.Where(c => c.NameEn == "Net cash flows from operating activities");
                            criterials.AddRange(expected);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{stockSymbol} - {reportType} - {yearInThePastFromNow} - {ex}");
            }


            foreach (FinanceByTimeModel time in lstTimeModel)
            {
                var year = time.YearPeriod;

                foreach (var item in criterials)
                {
                    var yearValue = lstTimeModel.IndexOf(time) == 0
                        ? item.Value4
                        : lstTimeModel.IndexOf(time) == 1
                            ? item.Value3
                            : lstTimeModel.IndexOf(time) == 2
                                ? item.Value2
                                : lstTimeModel.IndexOf(time) == 3
                                    ? item.Value1
                                    : 0;

                    var d = new StockSymbolFinanceHistory
                    {
                        YearPeriod = year,
                        Name = item.Name,
                        NameEn = item.NameEn,
                        StockSymbol = stockSymbol,
                        Type = (int)reportType,
                        Value = yearValue
                    };

                    result.Add(d);
                }
            }

            return result;
        }
    }
}
