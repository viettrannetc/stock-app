//using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreSqlDb.Common
{
    public class RestServiceHelper
    {
        private const string DataType = "application/json";

        public async Task<T> Get<T>(string url, bool? applyParseTwice = false) where T : class
        {
            return await Hexecute<T>(url, "GET", applyParseTwice: applyParseTwice);
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
            bool? applyParseTwice = false) where T : class
        {
            var url = new Uri(address);
            var request = WebRequest.Create(url) as HttpWebRequest;

            //var encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(Configuration.Username + ":" + Configuration.Password));
            //request.Headers.Add("Authorization", "Basic " + encoded);

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

                    if (applyParseTwice == true)
                        return JsonConvert.DeserializeObject<T>(JsonConvert.DeserializeObject(content).ToString());
                    else
                        return JsonConvert.DeserializeObject<T>(content);
                }
            }
            catch (WebException ex)
            {
                return null;
                //throw new Exception(ex.ToString());
            }
        }


        public async Task<string> HexecuteVietStock(string address = "https://api.vietstock.vn/data/financeinfo")
        {

            var url = new Uri(address);
            var postData = new Dictionary<string, string>();

            /*
             * Code: VIC
		ReportType: BCTT
		ReportTermType: 1
		Unit: 1000000000
		Page: 1
		PageSize: 4
		__RequestVerificationToken: adMS9gKy5_ugeDlh3_Ff0bLzeCCP7L0JKNBsdulAoR5GNifDnVygmL-z3QNUaXFLBPBoPauMJGUv8M_Sm1uckABuu4xL3KZLcgTgw47pkMo1
             */

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Host", "finance.vietstock.vn");
                //client.DefaultRequestHeaders.Add("Content-Length", "232");

                //client.DefaultRequestHeaders.Add("Accept", "*/*");                    
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));                    

                client.DefaultRequestHeaders.Add("User-Agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.82 Safari/537.36");
                //client.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded");
                //client.DefaultRequestHeaders.Add("Cookie", "_ga=GA1.2.713551826.1644247671; _ga=GA1.3.713551826.1644247671; dable_uid=43391554.1574785018857; dable_uid=43391554.1574785018857; AnonymousNotification=; language=vi-VN; Theme=Light; __gpi=UID=0000029fcfbf1e52:T=1647013809:RT=1647013809:S=ALNI_Mbnwz8R_W4WHz7DNQNBZDzQvsApJQ; ASP.NET_SessionId=m2egsnbcyuymr34zuoimieoo; __RequestVerificationToken=dNglG8kMDq09oaBufe3gxwAnLFhdRZGm0z13QEVZCEwzLmYZOoNuv2Am4tFL9_UOI5E3cJsN2C_MqWI75j6fZK0rhjuxNZHWN79YE3unhOI1; _gid=GA1.2.356828925.1648475526; vts_usr_lg=AA697487B6A84B1DFD5FBB731685026911EAED6A5739CF8D4B9E08AB9C0C39632E17B55D78783AD457047EA32EDDA6A072FB7E9BBE17EB11541DC9601607B128DB2F97013084ABB915F322D3BC20A2494DB4AC985AAAAA9025C809895DA74DCDDBBF6CF1B7241F5CEB8CF1BF16CE374D08435787C5CA97E8E0C5C53C810F0C85; vst_usr_lg_token=uUHaeIUDf0m7PjoQ5xSvaA==; finance_viewedstock=HUT,GIC,CIG,PVD,XMD,CEO,DGW,; _gat_gtag_UA_1460625_2=1; __gads=ID=3f7aab506428e100-22a2e49c64d1002e:T=1644247671:RT=1648483910:S=ALNI_MbD-QRlvz3Mc2X-6temXksp2u17ig; _gat=1");
                client.DefaultRequestHeaders.Add("__RequestVerificationToken", "xBn41_m3HO3gTm8p86LFvL667WRKMSx7TwBWZyQT9ZWzNkj9psjXFRybyk9a-W6N550n80WLix3hAFgDyZkAzHHcxTarL8HNnt4QMZsVTC-rF_vN4laUzXRMIjGhGR-C0");


                using (var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    //content.Add(new StringContent("Code"), "VIC");
                    //content.Add(new StringContent("ReportType"), "BCTT");
                    //content.Add(new StringContent("ReportTermType"), "1");
                    //content.Add(new StringContent("Unit"), "1000000000");
                    //content.Add(new StringContent("Page"), "1");
                    //content.Add(new StringContent("PageSize"), "4");
                    //content.Add(new StringContent("__RequestVerificationToken"), "xBn41_m3HO3gTm8p86LFvL667WRKMSx7TwBWZyQT9ZWzNkj9psjXFRybyk9a-W6N550n80WLix3hAFgDyZkAzHHcxTarL8HNnt4QMZsVTC-rF_vN4laUzXRMIjGhGR-C0");


                    content.Add(new StringContent("VIC"), "Code");
                    content.Add(new StringContent("BCTT"), "ReportType");
                    content.Add(new StringContent("1"), "ReportTermType");
                    content.Add(new StringContent("1000000000"), "Unit");
                    content.Add(new StringContent("1"), "Page");
                    content.Add(new StringContent("4"), "PageSize");
                    content.Add(new StringContent("xBn41_m3HO3gTm8p86LFvL667WRKMSx7TwBWZyQT9ZWzNkj9psjXFRybyk9a-W6N550n80WLix3hAFgDyZkAzHHcxTarL8HNnt4QMZsVTC-rF_vN4laUzXRMIjGhGR-C0"), "__RequestVerificationToken");

                    //content.Add(new StringContent("application/x-www-form-urlencoded"), "ContentType");

                    //content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded;charset=UTF-8");


                    //content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    content.Headers.ContentType.CharSet = "UTF-8";
                    content.Headers.ContentLength = 232;
                    //content.Headers.ContentType.("application/x-www-form-urlencoded; charset=UTF-8");

                    try
                    {
                        using (var message = await client.PostAsync(url, content))
                        {
                            var input = await message.Content.ReadAsStringAsync();

                            var test = input;

                            return test;
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                }
            }










            //postData.Add("code", code);
            //postData.Add("seq", "0");
            //postData.Add("__RequestVerificationToken", "unYCND6M-Cq1zAaGVUmMDJtS0rzeZUW6Daje3B1ON25gy3jSlTKZctc1QGKzTozGebq1yDriUCkN9fj3FmQi4pi1zY16f76HOMdx0N8uFTbBAUbJt4apzyfuZTqQKdJK0");

            //using (var httpClient = new HttpClient())
            //{
            //    httpClient.DefaultRequestHeaders.Add("Host", "finance.vietstock.vn");
            //    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
            //    using (var content = new FormUrlEncodedContent(postData))
            //    {
            //        content.Headers.Clear();



            //        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            //        content.Headers.Add("Content-Length", "171");

            //        content.Headers.Add("Cookie", "language=vi-VN; Theme=Light; _ga=GA1.2.713551826.1644247671; _ga=GA1.3.713551826.1644247671; dable_uid=43391554.1574785018857; AnonymousNotification=; dable_uid=43391554.1574785018857; _gid=GA1.2.146946559.1646059768; ASP.NET_SessionId=mbwci3g1rqtc5msadukkc304; finance_viewedstock=CEO,; __RequestVerificationToken=mTeG93mq-Qpab8L-KChv04jbDpm6zyWoF0wf2eHwJ26MbyvenH_lKVGEsMEgsgsA_jl4KHNz2gQHlgsRDM3uFlQnQW5P1KLbJAzS12sQu5c1; vts_usr_lg=A22548CC40A281CECF11BAC8D101E72D4EC3360B06B5B50F21CA2AEC1E526AC12983996A2BAF9C709B450C64A237839BD2DECB196A0F3D8B17339D024E57243E0097058C96F7367F192F293575B7E62CC6E0BC5AF785EA42B5FCA5C152A75469EB5CA31EA36176AD94220F1EE1C0FA7DC54EB52ACDC496B5DD423FABB1111CD5; vst_usr_lg_token=7V3EOCNNlU65gGzKnKWyYg==; __gads=ID=3f7aab506428e100-2235f7b1c6d000d2:T=1644247671:RT=1646142874:S=ALNI_MblLArLfmdNbRGEUQtnoagfsfj3CA");

            //        HttpResponseMessage response = await httpClient.PostAsync(url, content);

            //        using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
            //        {
            //            var s = await sr.ReadToEndAsync();
            //            var s1 = JsonConvert.DeserializeObject<T>(s);

            //            return s1;
            //        }
            //    }
            //}


            //var client = new RestClient("https://finance.vietstock.vn/data/getstockdealdetailbytime");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.POST);
            //request.AddHeader("Host", " finance.vietstock.vn");
            //request.AddHeader("Content-Length", " 151");
            //request.AddHeader("sec-ch-ua", " \" Not A;Brand\";v=\"99\", \"Chromium\";v=\"98\", \"Google Chrome\";v=\"98\"");
            //client.UserAgent = " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36";
            //request.AddHeader("sec-ch-ua-platform", " \"Windows\"");
            //request.AddHeader("Cookie", " language=vi-VN; ASP.NET_SessionId=4np4xnte3zcucc3y0n4jtxfg; __RequestVerificationToken=F-jR0emTgQS-oPhLp6k3muNICNRYvvU_YnsbF2w79oLk2XdwaK2oypcRmcQER54Ca5ow39A8vnjz9kr4oCtqdt6_o9UWU0jY2uZTgrl5nK81; Theme=Light; _ga=GA1.2.12576991.1644465709; _gid=GA1.2.1422348055.1644465709; _ga=GA1.3.12576991.1644465709; _gid=GA1.3.1422348055.1644465709; AnonymousNotification=; isShowLogin=true; dable_uid=2166588.1623985305222; _gat_gtag_UA_1460625_2=1; finance_viewedstock=CEO,; __gads=ID=f30175f1d5079bcc-22f3e1348bd000cd:T=1644465708:RT=1644492938:S=ALNI_MaCwQu8RYQzwn4mL6YgBUBgFADh9Q; language=vi-VN");
            //request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            //request.AddParameter("code", "VIC");
            //request.AddParameter("t", "");
            //request.AddParameter("__RequestVerificationToken", "DmL2-VppV1_q4Kp3HGPHrOkls7wHsmA_kgejrk6AdWvvTCN277uDx3CfCtqk172R2yaAQK6FNeP27NZw0syUXvd5s7UTmeV3OM0vpI7GlPk1");
            //request.AddParameter("seq", "0");
            //request.AddParameter("timetype", "1D");
            //IRestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);
        }

        //public async Task<String> HexecuteVietStockFlurl(string address = "https://api.vietstock.vn/data/financeinfo")
        //{

        //    var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));

        //    //content.Add(new StringContent("Code"), "VIC");
        //    //content.Add(new StringContent("ReportType"), "BCTT");
        //    //content.Add(new StringContent("ReportTermType"), "1");
        //    //content.Add(new StringContent("Unit"), "1000000000");
        //    //content.Add(new StringContent("Page"), "1");
        //    //content.Add(new StringContent("PageSize"), "4");
        //    //content.Add(new StringContent("__RequestVerificationToken"), "xBn41_m3HO3gTm8p86LFvL667WRKMSx7TwBWZyQT9ZWzNkj9psjXFRybyk9a-W6N550n80WLix3hAFgDyZkAzHHcxTarL8HNnt4QMZsVTC-rF_vN4laUzXRMIjGhGR-C0");


        //    content.Add(new StringContent("VIC"), "Code");
        //    content.Add(new StringContent("BCTT"), "ReportType");
        //    content.Add(new StringContent("1"), "ReportTermType");
        //    content.Add(new StringContent("1000000000"), "Unit");
        //    content.Add(new StringContent("1"), "Page");
        //    content.Add(new StringContent("4"), "PageSize");
        //    content.Add(new StringContent("xBn41_m3HO3gTm8p86LFvL667WRKMSx7TwBWZyQT9ZWzNkj9psjXFRybyk9a-W6N550n80WLix3hAFgDyZkAzHHcxTarL8HNnt4QMZsVTC-rF_vN4laUzXRMIjGhGR-C0"), "__RequestVerificationToken");


        //    var resp = await address
        //        .WithHeader("Host", "finance.vietstock.vn")
        //        //client.DefaultRequestHeaders.Add("Content-Length", "232");

        //        //client.DefaultRequestHeaders.Add("Accept", "*/*");                    
        //        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));                    
        //        //.WithHeader("User-Agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.82 Safari/537.36")


        //        //.WithCookies("_ga=GA1.2.713551826.1644247671; _ga=GA1.3.713551826.1644247671; dable_uid=43391554.1574785018857; dable_uid=43391554.1574785018857; AnonymousNotification=; language=vi-VN; Theme=Light; __gpi=UID=0000029fcfbf1e52:T=1647013809:RT=1647013809:S=ALNI_Mbnwz8R_W4WHz7DNQNBZDzQvsApJQ; ASP.NET_SessionId=m2egsnbcyuymr34zuoimieoo; __RequestVerificationToken=dNglG8kMDq09oaBufe3gxwAnLFhdRZGm0z13QEVZCEwzLmYZOoNuv2Am4tFL9_UOI5E3cJsN2C_MqWI75j6fZK0rhjuxNZHWN79YE3unhOI1; _gid=GA1.2.356828925.1648475526; vts_usr_lg=AA697487B6A84B1DFD5FBB731685026911EAED6A5739CF8D4B9E08AB9C0C39632E17B55D78783AD457047EA32EDDA6A072FB7E9BBE17EB11541DC9601607B128DB2F97013084ABB915F322D3BC20A2494DB4AC985AAAAA9025C809895DA74DCDDBBF6CF1B7241F5CEB8CF1BF16CE374D08435787C5CA97E8E0C5C53C810F0C85; vst_usr_lg_token=uUHaeIUDf0m7PjoQ5xSvaA==; finance_viewedstock=HUT,GIC,CIG,PVD,XMD,CEO,DGW,; _gat_gtag_UA_1460625_2=1; __gads=ID=3f7aab506428e100-22a2e49c64d1002e:T=1644247671:RT=1648483910:S=ALNI_MbD-QRlvz3Mc2X-6temXksp2u17ig; _gat=1")


        //        .PostUrlEncodedAsync(content);

        //    //    var resp = await address

        //    //    .WithHeader("Host", "finance.vietstock.vn")
        //    ////client.DefaultRequestHeaders.Add("Content-Length", "232");

        //    ////client.DefaultRequestHeaders.Add("Accept", "*/*");                    
        //    ////client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));                    
        //    //.WithHeader("User-Agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.82 Safari/537.36")


        //    //    .WithCookies("_ga=GA1.2.713551826.1644247671; _ga=GA1.3.713551826.1644247671; dable_uid=43391554.1574785018857; dable_uid=43391554.1574785018857; AnonymousNotification=; language=vi-VN; Theme=Light; __gpi=UID=0000029fcfbf1e52:T=1647013809:RT=1647013809:S=ALNI_Mbnwz8R_W4WHz7DNQNBZDzQvsApJQ; ASP.NET_SessionId=m2egsnbcyuymr34zuoimieoo; __RequestVerificationToken=dNglG8kMDq09oaBufe3gxwAnLFhdRZGm0z13QEVZCEwzLmYZOoNuv2Am4tFL9_UOI5E3cJsN2C_MqWI75j6fZK0rhjuxNZHWN79YE3unhOI1; _gid=GA1.2.356828925.1648475526; vts_usr_lg=AA697487B6A84B1DFD5FBB731685026911EAED6A5739CF8D4B9E08AB9C0C39632E17B55D78783AD457047EA32EDDA6A072FB7E9BBE17EB11541DC9601607B128DB2F97013084ABB915F322D3BC20A2494DB4AC985AAAAA9025C809895DA74DCDDBBF6CF1B7241F5CEB8CF1BF16CE374D08435787C5CA97E8E0C5C53C810F0C85; vst_usr_lg_token=uUHaeIUDf0m7PjoQ5xSvaA==; finance_viewedstock=HUT,GIC,CIG,PVD,XMD,CEO,DGW,; _gat_gtag_UA_1460625_2=1; __gads=ID=3f7aab506428e100-22a2e49c64d1002e:T=1644247671:RT=1648483910:S=ALNI_MbD-QRlvz3Mc2X-6temXksp2u17ig; _gat=1")
        //    //    .PostMultipartAsync(mp => mp
        //    //    .AddString("Code", "VIC")
        //    //    .AddString("ReportType", "BCTT")
        //    //    .AddString("ReportTermType", "1")
        //    //    .AddString("Unit", "1000000000")
        //    //    .AddString("Page", "1")
        //    //    .AddString("PageSize", "4")
        //    //    .AddString("__RequestVerificationToken", "adMS9gKy5_ugeDlh3_Ff0bLzeCCP7L0JKNBsdulAoR5GNifDnVygmL - z3QNUaXFLBPBoPauMJGUv8M_Sm1uckABuu4xL3KZLcgTgw47pkMo1"))
        //    ;

        //    var test = await resp.GetStringAsync();

        //    var a = 1;

        //    return test;
        //}



        public async Task<String> HexecuteVietStockPostman(string address = "https://api.vietstock.vn/data/financeinfo")
        {
            var client = new RestClient("https://api.vietstock.vn/data/financeinfo");
            //client.Timeout = -1;
            var request = new RestRequest(address, Method.Post);
            request.AddHeader("Host", "finance.vietstock.vn");
            request.AddHeader("Content-Length", "232");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Cookie", "_ga=GA1.2.713551826.1644247671; _ga=GA1.3.713551826.1644247671; dable_uid=43391554.1574785018857; dable_uid=43391554.1574785018857; AnonymousNotification=; language=vi-VN; Theme=Light; __gpi=UID=0000029fcfbf1e52:T=1647013809:RT=1647013809:S=ALNI_Mbnwz8R_W4WHz7DNQNBZDzQvsApJQ; ASP.NET_SessionId=m2egsnbcyuymr34zuoimieoo; __RequestVerificationToken=dNglG8kMDq09oaBufe3gxwAnLFhdRZGm0z13QEVZCEwzLmYZOoNuv2Am4tFL9_UOI5E3cJsN2C_MqWI75j6fZK0rhjuxNZHWN79YE3unhOI1; _gid=GA1.2.356828925.1648475526; vts_usr_lg=AA697487B6A84B1DFD5FBB731685026911EAED6A5739CF8D4B9E08AB9C0C39632E17B55D78783AD457047EA32EDDA6A072FB7E9BBE17EB11541DC9601607B128DB2F97013084ABB915F322D3BC20A2494DB4AC985AAAAA9025C809895DA74DCDDBBF6CF1B7241F5CEB8CF1BF16CE374D08435787C5CA97E8E0C5C53C810F0C85; vst_usr_lg_token=uUHaeIUDf0m7PjoQ5xSvaA==; finance_viewedstock=HUT,GIC,CIG,PVD,XMD,CEO,DGW,; _gat_gtag_UA_1460625_2=1; __gads=ID=3f7aab506428e100-22a2e49c64d1002e:T=1644247671:RT=1648483910:S=ALNI_MbD-QRlvz3Mc2X-6temXksp2u17ig; _gat=1");
            request.AddParameter("Code", "DGW");
            request.AddParameter("ReportType", "KQKD");
            request.AddParameter("ReportTermType", "2");
            request.AddParameter("Unit", "1000000000");
            request.AddParameter("Page", "8");
            request.AddParameter("PageSize", "9");
            request.AddParameter("__RequestVerificationToken", "xBn41_m3HO3gTm8p86LFvL667WRKMSx7TwBWZyQT9ZWzNkj9psjXFRybyk9a-W6N550n80WLix3hAFgDyZkAzHHcxTarL8HNnt4QMZsVTC-rF_vN4laUzXRMIjGhGR-C0");
            var response = await client.PostAsync(request);
            
            //Console.WriteLine(response.Content);

            //var test = await response.Content.GetStringAsync();

            var a = 1;

            return response.Content;
        }
    }
}
