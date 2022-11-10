using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.IO;
using System;
using System.Threading;
using System.Diagnostics;

class Program
{
    public static ResourceSemaphore httpSemaphore, socks4Semaphore, socks5Semaphore;
    public static string httpProxies, socks4Proxies, socks5Proxies;
    public static Stopwatch httpStopwatch, socks4Stopwatch, socks5Stopwatch;

    public static List<Tuple<string, string>> httpRequests = new List<Tuple<string, string>>()
    {
        new Tuple<string, string>("https://api.proxyscrape.com/?request=displayproxies&proxytype=http", "api.proxyscrape.com"),
        new Tuple<string, string>("https://www.proxy-list.download/api/v1/get?type=http", "proxy-list.download"),
        new Tuple<string, string>("https://www.proxyscan.io/download?type=http", "proxyscan.io"),
        new Tuple<string, string>("https://api.openproxylist.xyz/http.txt", "api.openproxylist.xyz"),
        new Tuple<string, string>("https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/http.txt", "raw.githubusercontent.com"),
        new Tuple<string, string>("https://raw.githubusercontent.com/ShiftyTR/Proxy-List/master/http.txt", "raw.githubusercontent.com"),
        new Tuple<string, string>("https://raw.githubusercontent.com/jetkai/proxy-list/main/online-proxies/txt/proxies-http.txt", "raw.githubusercontent.com"),
        new Tuple<string, string>("https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list-raw.txt", "raw.githubusercontent.com"),
    };

    public static List<Tuple<string, string>> socks4Requests = new List<Tuple<string, string>>()
    {
        new Tuple<string, string>("https://api.proxyscrape.com/?request=displayproxies&proxytype=socks4", "api.proxyscrape.com"),
        new Tuple<string, string>("https://raw.githubusercontent.com/jetkai/proxy-list/main/online-proxies/txt/proxies-socks4.txt", "raw.githubusercontent.com"),
        new Tuple<string, string>("https://www.proxy-list.download/api/v1/get?type=socks4", "proxy-list.download"),
        new Tuple<string, string>("https://www.proxyscan.io/download?type=socks4", "proxyscan.io"),
        new Tuple<string, string>("https://api.openproxylist.xyz/socks4.txt", "api.openproxylist.xyz"),
        new Tuple<string, string>("https://raw.githubusercontent.com/ShiftyTR/Proxy-List/master/socks4.txt", "raw.githubusercontent.com"),
        new Tuple<string, string>("https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks4.txt", "raw.githubusercontent.com"),
    };

    public static List<Tuple<string, string>> socks5Requests = new List<Tuple<string, string>>()
    {
        new Tuple<string, string>("https://api.proxyscrape.com/?request=displayproxies&proxytype=socks5", "api.proxyscrape.com"),
        new Tuple<string, string>("https://www.proxy-list.download/api/v1/get?type=socks5", "proxy-list.download"),
        new Tuple<string, string>("https://www.proxyscan.io/download?type=socks5", "proxyscan.io"),
        new Tuple<string, string>("https://raw.githubusercontent.com/ShiftyTR/Proxy-List/master/socks5.txt", "raw.githubusercontent.com"),
        new Tuple<string, string>("https://raw.githubusercontent.com/jetkai/proxy-list/main/online-proxies/txt/proxies-socks5.txt", "raw.githubusercontent.com"),
        new Tuple<string, string>("https://api.openproxylist.xyz/socks5.txt", "api.openproxylist.xyz"),
        new Tuple<string, string>("https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks5.txt", "raw.githubusercontent.com"),
    };

    public static int http, socks4, socks5, finished;

    static void Main()
    {
        Console.Title = "BestProxyFetcher";
        Console.WriteLine("[!] Fetching proxies, please wait a while.");

        httpSemaphore = new ResourceSemaphore();
        socks4Semaphore = new ResourceSemaphore();
        socks5Semaphore = new ResourceSemaphore();

        httpStopwatch = new Stopwatch();
        socks4Stopwatch = new Stopwatch();
        socks5Stopwatch = new Stopwatch();

        httpStopwatch.Start();
        socks4Stopwatch.Start();
        socks5Stopwatch.Start();

        System.Net.ServicePointManager.DefaultConnectionLimit = int.MaxValue;
        System.Net.ServicePointManager.MaxServicePoints = int.MaxValue;
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        new Thread(() => CheckHTTP()).Start();
        new Thread(() => CheckSOCKS4()).Start();
        new Thread(() => CheckSOCKS5()).Start();

        foreach (Tuple<string, string> tuple in httpRequests)
        {
            new Thread(() => FetchHTTP(tuple.Item1, tuple.Item2)).Start();
        }

        foreach (Tuple<string, string> tuple in socks4Requests)
        {
            new Thread(() => FetchSOCKS4(tuple.Item1, tuple.Item2)).Start();
        }

        foreach (Tuple<string, string> tuple in socks5Requests)
        {
            new Thread(() => FetchSOCKS5(tuple.Item1, tuple.Item2)).Start();
        }

        while (finished != 3)
        {
            Thread.Sleep(100);
        }

        Console.WriteLine("[!] Press ENTER to exit.");
        Console.ReadLine();
    }

    public static void CheckHTTP()
    {
        while (http != httpRequests.Count)
        {
            Thread.Sleep(100);
        }

        Thread.Sleep(100);
        WriteProxies("http.txt", httpProxies, "HTTP");
    }

    public static void CheckSOCKS4()
    {
        while (socks4 != socks4Requests.Count)
        {
            Thread.Sleep(100);
        }

        Thread.Sleep(100);
        WriteProxies("socks4.txt", socks4Proxies, "SOCKS4");
    }

    public static void CheckSOCKS5()
    {
        while (socks5 != socks5Requests.Count)
        {
            Thread.Sleep(100);
        }

        Thread.Sleep(100);
        WriteProxies("socks5.txt", socks5Proxies, "SOCKS5");
    }

    public static void WriteProxies(string fileName, string fileContent, string proxyType)
    {
        string newContent = "";
        int count = 0;

        foreach (string line in SplitToLines(fileContent))
        {
            string newLine = line.Replace(" ", "").Replace('\t'.ToString(), "");
            
            if (newLine == "")
            {
                continue;
            }

            if (newContent == "")
            {
                newContent = newLine;
            }
            else
            {
                newContent = newContent + "\r\n" + newLine;
            }

            count++;
        }

        try
        {
            System.IO.File.WriteAllText(fileName, newContent);
            string tookTime = "";

            if (proxyType.Equals("HTTP"))
            {
                httpStopwatch.Stop();
                tookTime = httpStopwatch.ElapsedMilliseconds.ToString();
            }
            else if (proxyType.Equals("SOCKS4"))
            {
                socks4Stopwatch.Stop();
                tookTime = socks4Stopwatch.ElapsedMilliseconds.ToString();
            }
            else if (proxyType.Equals("SOCKS5"))
            {
                socks5Stopwatch.Stop();
                tookTime = socks5Stopwatch.ElapsedMilliseconds.ToString();
            }

            Console.WriteLine("[!] Succesfully fetched " + count + " " + proxyType + " proxies. Saved in '" + fileName + "' file. Operation time took " + tookTime + "ms.");
        }
        catch
        {
            Console.WriteLine("[!] There was an error on saving file '" + fileName + "'.");
        }

        Interlocked.Increment(ref finished);
    }

    public static IEnumerable<string> SplitToLines(string input)
    {
        if (input == null)
        {
            yield break;
        }

        using (System.IO.StringReader reader = new System.IO.StringReader(input))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }

    public static void FetchHTTP(string url, string host)
    {
        goHere: while (httpSemaphore.IsResourceNotAvailable())
        {
            Thread.Sleep(100);
        }

        if (httpSemaphore.IsResourceAvailable())
        {
            httpSemaphore.LockResource();

            if (httpProxies == "")
            {
                httpProxies = FetchResource(url, host);
            }
            else
            {
                httpProxies += Environment.NewLine + FetchResource(url, host);
            }

            httpSemaphore.UnlockResource();
        }
        else
        {
            goto goHere;
        }

        http++;
    }

    public static void FetchSOCKS4(string url, string host)
    {
        goHere: while (socks4Semaphore.IsResourceNotAvailable())
        {
            Thread.Sleep(100);
        }

        if (socks4Semaphore.IsResourceAvailable())
        {
            socks4Semaphore.LockResource();

            if (socks4Proxies == "")
            {
                socks4Proxies = FetchResource(url, host);
            }
            else
            {
                socks4Proxies += Environment.NewLine + FetchResource(url, host);
            }

            socks4Semaphore.UnlockResource();
        }
        else
        {
            goto goHere;
        }

        socks4++;
    }

    public static void FetchSOCKS5(string url, string host)
    {
        goHere: while (socks5Semaphore.IsResourceNotAvailable())
        {
            Thread.Sleep(100);
        }

        if (socks5Semaphore.IsResourceAvailable())
        {
            socks5Semaphore.LockResource();

            if (socks4Proxies == "")
            {
                socks5Proxies = FetchResource(url, host);
            }
            else
            {
                socks5Proxies += Environment.NewLine + FetchResource(url, host);
            }

            socks5Semaphore.UnlockResource();
        }
        else
        {
            goto goHere;
        }

        socks5++;
    }

    public static string FetchResource(string url, string host)
    {
        try
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Proxy = null;
            request.UseDefaultCredentials = false;
            request.AllowAutoRedirect = false;

            var field = typeof(HttpWebRequest).GetField("_HttpRequestHeaders", BindingFlags.Instance | BindingFlags.NonPublic);

            request.Method = "GET";

            var headers = new CustomWebHeaderCollection(new Dictionary<string, string>
            {
                ["Host"] = host,
            });

            field.SetValue(request, headers);

            var response = request.GetResponse();
            bool isValid = false;
            string content = Encoding.UTF8.GetString(ReadFully(response.GetResponseStream()));

            response.Close();
            response.Dispose();

            return content;
        }
        catch
        {
            return "";
        }
    }

    public static byte[] ReadFully(Stream input)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}