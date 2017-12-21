using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
                var apiKey = AppSecrets.Secrets[FunctionKeyHeader];
                var baseUrl = ConfigurationManager.AppSettings["PrimeFunctionUrl"];
                var headers = new NameValueCollection();
                headers.Add(FunctionKeyHeader, apiKey);
                primes = await PrimeHttpHelper.GetJsonAsync(baseUrl, min, max, headers);
            }
            else
            {
                primes = PrimeCalc.GetPrimesAsJson(min, max);
            }

            return primes;
        }

    }

}