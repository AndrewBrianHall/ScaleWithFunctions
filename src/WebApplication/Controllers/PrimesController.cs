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
using Newtonsoft.Json;
using PrimeLib;

namespace WebApplication.Controllers
{
    public class PrimesController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public async Task<string> Get(long min, long max, bool useFunc)
        {
            string primes;

            if (useFunc)
            {
                primes = await CallAzureFunction.GetPrimes(min, max);
            }
            else
            {
                primes =PrimeCalc.GetPrimesAsJson(min, max);
            }

            return primes;
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }

    public static class CallAzureFunction
    {
        const string FunctionKeyHeader = "x-functions-key";

        public static async Task<string> GetPrimes(long min, long max)
        {
            var code = ConfigurationManager.AppSettings[FunctionKeyHeader];
            var baseUrl = ConfigurationManager.AppSettings["PrimeFunctionUrl"];
            var url = baseUrl + $"?min={min}&max={max}";
            var req = WebRequest.Create(url);
            req.Headers.Add(FunctionKeyHeader, code);
            var resp = await req.GetResponseAsync() as HttpWebResponse;
            var str = new StreamReader(resp.GetResponseStream(), System.Text.Encoding.GetEncoding(resp.CharacterSet));
            var json = await str.ReadToEndAsync();
            return json;
        }

        
    }
}