using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
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
            bool? applyParseTwice = false) where T: class
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


        //public async Task<T> PostVietStock<T>(string url, string code)
        //{
        //    return await HexecuteVietStock<T>(url, "POST");
        //}

        //private async Task<T> HexecuteVietStock<T>(string address, string code)
        //{
        //    var url = new Uri(address);
        //    var postData = new Dictionary<string, string>();
        //    postData.Add("code", code);
        //    postData.Add("seq", "0");
        //    postData.Add("__RequestVerificationToken", "unYCND6M-Cq1zAaGVUmMDJtS0rzeZUW6Daje3B1ON25gy3jSlTKZctc1QGKzTozGebq1yDriUCkN9fj3FmQi4pi1zY16f76HOMdx0N8uFTbBAUbJt4apzyfuZTqQKdJK0");

        //    using (var httpClient = new HttpClient())
        //    {
        //        httpClient.DefaultRequestHeaders.Add("Host", "finance.vietstock.vn");
        //        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
        //        using (var content = new FormUrlEncodedContent(postData))
        //        {
        //            content.Headers.Clear();

                    

        //            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        //            content.Headers.Add("Content-Length", "171");

        //            content.Headers.Add("Cookie", "language=vi-VN; Theme=Light; _ga=GA1.2.713551826.1644247671; _ga=GA1.3.713551826.1644247671; dable_uid=43391554.1574785018857; AnonymousNotification=; dable_uid=43391554.1574785018857; _gid=GA1.2.146946559.1646059768; ASP.NET_SessionId=mbwci3g1rqtc5msadukkc304; finance_viewedstock=CEO,; __RequestVerificationToken=mTeG93mq-Qpab8L-KChv04jbDpm6zyWoF0wf2eHwJ26MbyvenH_lKVGEsMEgsgsA_jl4KHNz2gQHlgsRDM3uFlQnQW5P1KLbJAzS12sQu5c1; vts_usr_lg=A22548CC40A281CECF11BAC8D101E72D4EC3360B06B5B50F21CA2AEC1E526AC12983996A2BAF9C709B450C64A237839BD2DECB196A0F3D8B17339D024E57243E0097058C96F7367F192F293575B7E62CC6E0BC5AF785EA42B5FCA5C152A75469EB5CA31EA36176AD94220F1EE1C0FA7DC54EB52ACDC496B5DD423FABB1111CD5; vst_usr_lg_token=7V3EOCNNlU65gGzKnKWyYg==; __gads=ID=3f7aab506428e100-2235f7b1c6d000d2:T=1644247671:RT=1646142874:S=ALNI_MblLArLfmdNbRGEUQtnoagfsfj3CA");

        //            HttpResponseMessage response = await httpClient.PostAsync(url, content);

        //            using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
        //            {
        //                var s = await sr.ReadToEndAsync();
        //                var s1 = JsonConvert.DeserializeObject<T>(s);

        //                return s1;
        //            }
        //        }
        //    }


        //    //var client = new RestClient("https://finance.vietstock.vn/data/getstockdealdetailbytime");
        //    //client.Timeout = -1;
        //    //var request = new RestRequest(Method.POST);
        //    //request.AddHeader("Host", " finance.vietstock.vn");
        //    //request.AddHeader("Content-Length", " 151");
        //    //request.AddHeader("sec-ch-ua", " \" Not A;Brand\";v=\"99\", \"Chromium\";v=\"98\", \"Google Chrome\";v=\"98\"");
        //    //client.UserAgent = " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36";
        //    //request.AddHeader("sec-ch-ua-platform", " \"Windows\"");
        //    //request.AddHeader("Cookie", " language=vi-VN; ASP.NET_SessionId=4np4xnte3zcucc3y0n4jtxfg; __RequestVerificationToken=F-jR0emTgQS-oPhLp6k3muNICNRYvvU_YnsbF2w79oLk2XdwaK2oypcRmcQER54Ca5ow39A8vnjz9kr4oCtqdt6_o9UWU0jY2uZTgrl5nK81; Theme=Light; _ga=GA1.2.12576991.1644465709; _gid=GA1.2.1422348055.1644465709; _ga=GA1.3.12576991.1644465709; _gid=GA1.3.1422348055.1644465709; AnonymousNotification=; isShowLogin=true; dable_uid=2166588.1623985305222; _gat_gtag_UA_1460625_2=1; finance_viewedstock=CEO,; __gads=ID=f30175f1d5079bcc-22f3e1348bd000cd:T=1644465708:RT=1644492938:S=ALNI_MaCwQu8RYQzwn4mL6YgBUBgFADh9Q; language=vi-VN");
        //    //request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        //    //request.AddParameter("code", "VIC");
        //    //request.AddParameter("t", "");
        //    //request.AddParameter("__RequestVerificationToken", "DmL2-VppV1_q4Kp3HGPHrOkls7wHsmA_kgejrk6AdWvvTCN277uDx3CfCtqk172R2yaAQK6FNeP27NZw0syUXvd5s7UTmeV3OM0vpI7GlPk1");
        //    //request.AddParameter("seq", "0");
        //    //request.AddParameter("timetype", "1D");
        //    //IRestResponse response = client.Execute(request);
        //    //Console.WriteLine(response.Content);
        //}
    }
}
