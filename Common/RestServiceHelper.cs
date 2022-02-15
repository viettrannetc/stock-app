using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreSqlDb.Common
{
    public class RestServiceHelper
    {
        private const string DataType = "application/json";

        public async Task<T> Get<T>(string url, bool? applyParseTwice = false)
        {
            return await Hexecute<T>(url, "GET", applyParseTwice: applyParseTwice);
        }

        public async Task<T> Post<T>(string url, object data)
        {
            return await Hexecute<T>(url, "POST", JsonConvert.SerializeObject(data, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
        }

        public async Task<T> Put<T>(string url, object data)
        {
            return await Hexecute<T>(url, "PUT", JsonConvert.SerializeObject(data, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
        }

        public async Task<T> Delete<T>(string url)
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
            bool? applyParseTwice = false)
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
                throw new Exception(ex.ToString());
            }
        }
    }
}
