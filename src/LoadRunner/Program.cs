using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LoadRunner
{
    class Program
    {
        static async void Main(string[] args)
        {
            const int NumberOfRequests = 100;
            const int Min = 1000000;
            const int Max = 4000000;
            string BaseUrl = $"http://localhost:56053/api/Primes?min={Min}&max={Max}";
            var rand = new Random();
            int success = 0;

            var sw = Stopwatch.StartNew();

            Parallel.For(0, NumberOfRequests, async i =>
            {
                try
                {
                    var req = WebRequest.Create(BaseUrl);
                    var resp = await req.GetResponseAsync() as HttpWebResponse;
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        lock (sw)
                        {
                            success++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Request failed: " + ex.Message);
                }
            });

            sw.Stop();

            double avg = sw.ElapsedMilliseconds / NumberOfRequests;
            Console.WriteLine($"Successfully completed {success}/{NumberOfRequests} requests in {sw.ElapsedMilliseconds}ms, avg/request: {avg}");
        }
    }
}
