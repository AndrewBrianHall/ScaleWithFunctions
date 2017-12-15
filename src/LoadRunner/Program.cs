using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LoadRunner
{
    class Program
    {
        const int NumberOfRequests = 50;
        const int SimultaneousRequests = 20;
        const int WarmUpRequestCount = SimultaneousRequests;
        const int Min = 1000000;
        const int Max = 3000000;
        const bool UseFunction = false;
        const string BaseUrl = "http://localhost:56053/api/Primes?min={0}&max={1}&useFunc={2}";
        const string OutputFile = "results-{0}.csv";
        //static Stopwatch _sw;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Warming up");
            var warmup = new RequestCounter(10, BaseUrl, 0, 10000, UseFunction, false);
            await warmup.MakeRequests(WarmUpRequestCount);

            Console.WriteLine("Warmup completed, starting test");

            var requests = new RequestCounter(SimultaneousRequests, BaseUrl, Min, Max, UseFunction, true);
            var task = requests.MakeRequests(NumberOfRequests);
            var mainPage = new RequestCounter("http://localhost:56053", false);
            var mainPageTask = mainPage.MakeRecurringRequest(100, -1);
            await task;

            mainPage.Stop();
            await mainPageTask;

            double requestAvg = requests.TotalTime / NumberOfRequests;
            Console.WriteLine($"Successfully completed {requests.SuccessfulRequests}/{NumberOfRequests} requests, avg/request: {requestAvg}");

            double mainPageAvg = mainPage.TotalTime / mainPage.SuccessfulRequests;
            Console.WriteLine($"Main page was loaded {mainPage.SuccessfulRequests} with an average load time of {mainPageAvg}");

            var outputFile = UseFunction ? string.Format(OutputFile, "function") : string.Format(OutputFile, "local");
            await PrintResults(outputFile, new List<RequestCounter>() { requests, mainPage });
        }

        static async Task PrintResults(string outputFile, List<RequestCounter> counters)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                await writer.WriteAsync("Time");
                for (int i = 0; i < counters.Count; i++)
                {
                    await writer.WriteAsync("," + counters[i].Url);
                }
                await writer.WriteLineAsync();

                for (int i = 0; i < counters.Count; i++)
                {
                    var padding = new string(',', i + 1);

                    foreach (var record in counters[i].RequestHistory)
                    {
                        await writer.WriteLineAsync(record.TimeStamp + padding + record.Duration);
                    }
                }
            }
        }
    }
}
