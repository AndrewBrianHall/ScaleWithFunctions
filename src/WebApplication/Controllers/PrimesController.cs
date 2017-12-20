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
using WebApplication.App_Start;

namespace WebApplication.Controllers
{

    public class PrimesController : ApiController
    {
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
                primes = PrimeCalc.GetPrimesAsJson(min, max);
            }

            return primes;
        }

        private async Task<string> GetPrimes(long min, long max)
        {
            var code = AppSecrets.Secrets[FunctionKeyHeader];
            var baseUrl = ConfigurationManager.AppSettings["PrimeFunctionUrl"];
            var url = baseUrl + $"?min={min}&max={max}";
            var req = WebRequest.Create(url);
            req.Headers.Add(FunctionKeyHeader, code);
            var resp = await req.GetResponseAsync() as HttpWebResponse;
            var jsonResult = await HttpHelper.GetJsonAsync(resp);
            return jsonResult;
        }

    }

}