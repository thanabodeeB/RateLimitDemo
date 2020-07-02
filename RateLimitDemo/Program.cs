using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace RateLimitDemo
{

    /*
     
        This is a mere console application to demonstrate rate limitng on certain paths
        by call GET to the path in accelerated interval which ensure inducing rate limit.

        Ex. if rate limit is  10 requests every 5 seconds
        This application will make a call every 0.412 seconds
        which will take 4.2 seconds(less then 5 seconds) to reach the limit of 10 requests.

    */

    class Program
    {
        static HttpClient Client = new HttpClient();
        static private readonly string LocalCityPath = "http://localhost:58145/hotel/city?city=bangkok";
        static private readonly string LocalRoomPath = "http://localhost:58145/hotel/room?room=deluxe";
        static private readonly string DeployedCityPath = "http://hotelinfo.azurewebsites.net/hotel/city?city=bangkok";
        static private readonly string DeployedRoomPath = "http://hotelinfo.azurewebsites.net/hotel/room?room=deluxe";
        static string Path;
        static System.Timers.Timer Timer1;
        static float Timepassed;
        static int ElapsedCount;
        static int Interval;
        static int Request;
        static int Second;
        static float ExpectedTime;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\nThis is a mere console application to demonstrate rate limitng on certain paths" +
                    "\nby call GET to the path in accelerated interval which ensure inducing rate limit" +
                    "\n\nEx. if rate limit is 10 requests every 5 seconds" +
                    "\nThis application will make a call every 0.412 seconds" +
                    "\nwhich will take 4.2 seconds(less then 5 seconds) to reach the limit of 10 requests");
                Console.WriteLine("\nDo you want to run demo for Deployed(1), Localhost(2), UserInput(3)");
                Console.WriteLine("type 1, 2 or 3 then press Enter");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1": ShowDeployed(); break;
                    case "2": ShowLocal(); break;
                    case "3": UserInput(); break;
                }
            }

        }

        static void UserInput()
        {
            try
            {
                Console.Clear();


                Console.Write("Input Path URL: ");
                string path = Console.ReadLine();

                Console.WriteLine("\nEx. 10 requests every 5 seconds \nnumber of request = 10, timespan = 5\n");

                Console.Write("Input number of request: ");
                int request = Convert.ToInt32(Console.ReadLine());

                Console.Write("Input timespan: ");
                int timespan = Convert.ToInt32(Console.ReadLine());

                ShowCustom(path, request, timespan);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void ShowCustom(string path, int request, int second)
        {
            Console.Clear();
            Demo(path, request, second);
            Console.ReadLine();
            Console.Clear();
            Timer1.Stop();
            Timer1.Dispose();
        }

        static void ShowLocal()
        {
            Console.Clear();
            Demo(LocalCityPath, 10, 5);
            Console.ReadLine();
            Console.Clear();
            Timer1.Stop();
            Demo(LocalRoomPath, 100, 10);
            Console.ReadLine();
            Timer1.Stop();
            Timer1.Dispose();
        }

        static void ShowDeployed()
        {
            Console.Clear();
            Demo(DeployedCityPath, 10, 5);
            Console.ReadLine();
            Console.Clear();
            Timer1.Stop();
            Demo(DeployedRoomPath, 100, 10);
            Console.ReadLine();
            Timer1.Stop();
            Timer1.Dispose();
        }

        static void Demo(string path, int request, int second)
        {
            Request = request;
            Second = second;
            ElapsedCount = 0;
            Timepassed = 0;
            Interval = (int)((float)(Second * 1000) / (float)(Request * 1.2));
            ExpectedTime = (float)Interval * (float)(Request) / 1000;
            SetTimer(Interval);
            Path = path;
            Console.WriteLine($"\nPath: {path}");
            Console.WriteLine($"Rate Limit: {request} requests in {second} seconds \n");
            Console.WriteLine($"Call after {ExpectedTime:N1} Seconds should return 429 for a configured period of time.");
            if (path.Contains("city"))
            {
                Console.WriteLine($"( Press Enter to demo another endpoint. )");
            }

            Console.WriteLine("____________________________________________________\n");
            Thread.Sleep(3000);
            Timer1.Start();
        }

        static void SetTimer(int interval)
        {
            Timer1 = new System.Timers.Timer(interval);
            Timer1.Elapsed += Timer1_Elapsed;
            Timer1.AutoReset = true;

        }

        private static void Timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            Timepassed += (float)Interval / 1000;
            ElapsedCount++;
            try
            {
                var result = CallGet(Path).Result;
                Console.WriteLine($"Call #{ElapsedCount.ToString().PadRight(3, ' ')} | Time Passed: {Timepassed:N1} seconds | Status Code: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
                Timer1.Stop();
                Console.ReadLine();
            }
        }

        //Call GET to endpoint
        private static async Task<object> CallGet(string path)
        {
            HttpResponseMessage response = await Client.GetAsync(path);
            try
            {
                response.EnsureSuccessStatusCode();
                return response.StatusCode;
            }
            catch
            {
                return response.StatusCode;
            }
        }
    }
}
