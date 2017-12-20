﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PrimeLib
{
    public class HttpHelper
    {
        public static async Task<string> GetJsonAsync(HttpWebResponse resp)
        {
            var str = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(resp.CharacterSet));
            var json = await str.ReadToEndAsync();
            var standardized = SanitizeResult(json);
            return standardized;
        }

        public static async Task<MinMaxPair> GetMinMaxAsync(HttpWebResponse resp)
        {
            var json = await GetJsonAsync(resp);
            var pair = JsonConvert.DeserializeObject<MinMaxPair>(json);
            return pair;
        }

        private static string SanitizeResult(string json)
        {
            var final = json.Substring(1, json.Length - 2).Replace("\\", "");
            return final;
        }
    }
}
