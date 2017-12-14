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
        const int WarmUpRequestCount = 10;
        const int NumberOfRequests = 30;
        const int SimultaneousRequests = 10;
        const int Min = 1000000;
        const int Max = 2000000;
        const bool UseFunction = true;
        const string BaseUrl = "http://localhost:56053/api/Primes?min={0}&max={1}&useFunc={2}";
        //static Stopwatch _sw;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Warming up");
            var warmup = new RequestCounter(10, BaseUrl, 0, 10000, UseFunction);
            await warmup.MakeRequests(WarmUpRequestCount);

            Console.WriteLine("Warmup completed, starting test");

            var requests = new RequestCounter(SimultaneousRequests, BaseUrl, Min, Max, UseFunction);
            var sw = Stopwatch.StartNew();
            await requests.MakeRequests(NumberOfRequests);
            sw.Stop();

            double avg = requests.TotalTime / NumberOfRequests;
            Console.WriteLine($"Successfully completed {requests.SuccessfulRequests}/{NumberOfRequests} requests in {sw.ElapsedMilliseconds}ms, avg/request: {avg}");
        }
    }

    class RequestCounter
    {
        object lockObj = new object();
        private long _totalTime = 0;
        public long TotalTime
        {
            get { lock (lockObj) { return _totalTime; } }
            set { lock (lockObj) { _totalTime = value; } }
        }
        int _madeRequests = 0;
        public int MadeRequests
        {
            get { lock (lockObj) { return _madeRequests; } }
            set { lock (lockObj) { _madeRequests = value; } }
        }
        int _successfulRequests = 0;
        public int SuccessfulRequests
        {
            get { lock (lockObj) { return _successfulRequests; } }
            set { lock (lockObj) { _successfulRequests = value; } }
        }

        int _simultaneousRequests;
        string _url;

        public RequestCounter(int simultaneousRequests, string url, int min, int max, bool useFunction)
        {
            _simultaneousRequests = simultaneousRequests;
            _url = string.Format(url, min, max, useFunction);

        }

        public async Task MakeRequests(int numberOfRequests)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < _simultaneousRequests; i++)
            {
                var task = NextRequest(numberOfRequests);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        async Task NextRequest(int totalRequests)
        {
            while (MadeRequests < totalRequests)
            {
                MadeRequests++;
                await MakeRequest();
            }
        }

        async Task MakeRequest()
        {
            Console.WriteLine($"Starting request {MadeRequests}");
            var sw = Stopwatch.StartNew();
            try
            {
                var req = WebRequest.Create(_url);
                var resp = await req.GetResponseAsync() as HttpWebResponse;
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"Succeeded in {sw.ElapsedMilliseconds}");
                    lock (lockObj)
                    {
                        SuccessfulRequests++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Request failed: " + ex.Message);
            }
            sw.Stop();
            TotalTime += sw.ElapsedMilliseconds;
        }
    }
}
