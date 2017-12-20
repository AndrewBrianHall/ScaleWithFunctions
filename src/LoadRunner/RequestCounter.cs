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

    enum RequestCounterTypes { Load, Continuous}

    class RequestCounter
    {
        object _lockObj = new object();
        int _simultaneousRequests;
        bool _logRequests;
        CancellationTokenSource _tokenSource;

        public RequestCounterTypes CounterType { get; set; }
        public DateTime LoadStart { get; private set; }
        public DateTime LoadEnd { get; private set; }

        public List<RequestRecord> RequestHistory = new List<RequestRecord>();
        public Stopwatch RequestStopWatch { get; private set; }

        private long _totalTime = 0;
        public long TotalTime
        {
            get { lock (_lockObj) { return _totalTime; } }
            set { lock (_lockObj) { _totalTime = value; } }
        }
        int _madeRequests = 0;
        public int MadeRequests
        {
            get { lock (_lockObj) { return _madeRequests; } }
            set { lock (_lockObj) { _madeRequests = value; } }
        }
        int _successfulRequests = 0;
        public int SuccessfulRequests
        {
            get { lock (_lockObj) { return _successfulRequests; } }
            set { lock (_lockObj) { _successfulRequests = value; } }
        }

        public string Url { get; set; }

        public RequestCounter(string url, bool logRequests)
        {
            _logRequests = logRequests;

            this.Url = url;
            this.CounterType = RequestCounterTypes.Continuous;
        }

        public RequestCounter(int simultaneousRequests, string url, int min, int max, bool useFunction, bool logRequests)
        {
            _simultaneousRequests = simultaneousRequests;
            _logRequests = logRequests;

            this.Url = string.Format(url, min, max, useFunction);
            this.CounterType = RequestCounterTypes.Load;
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
            LoadStart = DateTime.Now;

            RequestStopWatch = Stopwatch.StartNew();
            for (int i = 0; i < _simultaneousRequests; i++)
            {
                var task = NextRequest(numberOfRequests);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);

            RequestStopWatch.Stop();
            LoadEnd = DateTime.Now;
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
            LogMessage($"Starting request {MadeRequests}");
            var begin = DateTime.Now;
            var sw = Stopwatch.StartNew();
            try
            {
                var req = WebRequest.Create(Url);
                var resp = await req.GetResponseAsync() as HttpWebResponse;
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    LogMessage($"Succeeded in {sw.ElapsedMilliseconds}");
                    SuccessfulRequests++;
                }
            }
            catch (Exception ex)
            {
                LogMessage("Request failed: " + ex.Message, true);
            }
            sw.Stop();
            RequestHistory.Add(new RequestRecord() { TimeStamp = begin, Url = Url, Duration = sw.ElapsedMilliseconds});
            TotalTime += sw.ElapsedMilliseconds;
        }

        private void LogMessage(string message, bool error = false)
        {
            if (_logRequests || error)
            {
                Console.WriteLine(message);
            }
        }
    }
}
