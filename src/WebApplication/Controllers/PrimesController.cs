using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using PrimeLib;
using WebApplication.Models;

namespace WebApplication.Controllers
{

    public class PrimesController : ApiController
    {
        private static Secrets _secrets = new Secrets();
        private const string FunctionKeyHeader = "x-functions-key";

        // GET api/<controller>/5
        public async Task<string> Get(long min, long max, bool useFunc)
        {
            string primes;

             if (useFunc)
            {
                primes = await GetPrimes(min, max);
            }
            else
            {
                primes =PrimeCalc.GetPrimesAsJson(min, max);
            }

            return primes;
        }

        private async Task<string> GetPrimes(long min, long max)
        {
            var code = _secrets[FunctionKeyHeader];
            var baseUrl = ConfigurationManager.AppSettings["PrimeFunctionUrl"];
            var url = baseUrl + $"?min={min}&max={max}";
            var req = WebRequest.Create(url);
            req.Headers.Add(FunctionKeyHeader, code);
            var resp = await req.GetResponseAsync() as HttpWebResponse;
            var str = new StreamReader(resp.GetResponseStream(), System.Text.Encoding.GetEncoding(resp.CharacterSet));
            var json = await str.ReadToEndAsync();
            var standardized = UnescapeResult(json);
            return standardized;
        }

        private string UnescapeResult(string json)
        {
            var final = json.Substring(1, json.Length - 2).Replace("\\", "");
            return final;
        }
    }

}