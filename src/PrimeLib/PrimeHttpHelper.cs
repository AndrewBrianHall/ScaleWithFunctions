using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PrimeLib
{
    public class PrimeHttpHelper
    {
        public static async Task<string> GetJsonAsync(string url, long min, long max, NameValueCollection headers)
        {
            var json = await GetPrimes(url, min, max, headers);
            return json;
        }

        public static async Task<MinMaxPair> GetMinMaxAsync(string url, long min, long max, NameValueCollection headers)
        {
            var json = await GetJsonAsync(url, min, max, headers);
            var pair = JsonConvert.DeserializeObject<MinMaxPair>(json);
            return pair;
        }

        private static async Task<string> GetPrimes(string baseUrl, long min, long max, NameValueCollection headers)
        {
            var url = baseUrl + $"?min={min}&max={max}";
            var req = WebRequest.Create(url);
            req.Headers.Add(headers);

            var resp = await req.GetResponseAsync() as HttpWebResponse;
            var str = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(resp.CharacterSet));
            var json = await str.ReadToEndAsync();
            var standardized = SanitizeResult(json);
            return standardized;
        }

        private static string SanitizeResult(string json)
        {
            var final = json.Substring(1, json.Length - 2).Replace("\\", "");
            return final;
        }
    }
}
