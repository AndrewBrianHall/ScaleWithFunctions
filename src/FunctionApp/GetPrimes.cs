using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using PrimeLib;

namespace FunctionApp
{
    public static class GetPrimes
    {
        [FunctionName("GetPrimes")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            long min = long.Parse(req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "Min", true) == 0)
                .Value);
            long max = long.Parse(req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "Max", true) == 0)
                .Value);

            var primes = PrimeCalc.GetPrimesAsJson(min, max);
            

            return req.CreateResponse(HttpStatusCode.OK, primes);
        }
    }
}
