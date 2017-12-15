using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LoadRunner
{
    class RequestRecord
    {
        public DateTime TimeStamp { get; set; }
        public string Url { get; set; }
        public long Duration { get; set; }
    }

    class RequestCounter
    {
        public List<RequestRecord> RequestHistory = new List<RequestRecord>();

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
        public string Url { get; set; }

        CancellationTokenSource _tokenSource;

        public RequestCounter(string url)
        {
            Url = url;
        }

        public RequestCounter(int simultaneousRequests, string url, int min, int max, bool useFunction)
        {
            _simultaneousRequests = simultaneousRequests;
            Url = string.Format(url, min, max, useFunction);

        }

        public void Stop()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
            }
        }

        public async Task MakeRecurringRequest(int delay, int maxCount = -1)
        {
            _tokenSource = new CancellationTokenSource();
            CancellationToken token = _tokenSource.Token;
            int reqCount = 0;

            while (!token.IsCancellationRequested)
            {
                await MakeRequest();
                await Task.Delay(delay);
                reqCount++;
                if (maxCount > 0 && reqCount == maxCount)
                {
                    _tokenSource.Cancel();
                }
            }
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
            var begin = DateTime.Now;
            var sw = Stopwatch.StartNew();
            try
            {
                var req = WebRequest.Create(Url);
                var resp = await req.GetResponseAsync() as HttpWebResponse;
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"Succeeded in {sw.ElapsedMilliseconds}");
                    SuccessfulRequests++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Request failed: " + ex.Message);
            }
            sw.Stop();
            RequestHistory.Add(new RequestRecord() { TimeStamp = begin, Url = Url, Duration = sw.ElapsedMilliseconds});
            TotalTime += sw.ElapsedMilliseconds;
        }
    }
}
