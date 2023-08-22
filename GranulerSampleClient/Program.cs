using Grainuler;
using Grainuler.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using SampleTestJobs;
using System.Net;

namespace GranulerSampleClient
{
    public class Program
    {
        public const string MainTaskId = "job1";
        public const string DependedTaskId = "job2";

        static int Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();
            return RunMainAsync(config).GetAwaiter().GetResult();
        }

        private static async Task<int> RunMainAsync(IConfiguration config)
        {
            try
            {
                await Task.Delay(2000);
                using (var client = await ConnectClient(config))
                {
                    await InitateGrainuler(client);
                }
                Console.ReadLine();
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nException while trying to run client: {e.Message}");
                Console.WriteLine("Make sure the silo the client is trying to connect to is running.");
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
                return 1;
            }
        }


        private static async Task InitateGrainuler(IClusterClient client)
        {

            var builder = new ScheduleTaskGrainBuilder(client, MainTaskId);
            await builder
            .AddPayload(typeof(TestClass), "Run", new object[] { "testInstance1" }, new object[] { DateTime.Now }, false)
            .AddOnScheduleTrigger(TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1))
            .Trigger();


            var builder2 = new ScheduleTaskGrainBuilder(client, DependedTaskId);
            await builder2
            .AddPayload(typeof(ReactiveTestClass), "ReactiveRun", new object[] { }, new object[] { DateTime.Now }, true)
            .AddOnSuccededTrigger(MainTaskId)
            .Trigger();


            //var obs = client.GetGrain<ITaskCompletedEventObserver>("1");
            //await obs.SetSubscription(MainTaskId);

        }
        private static async Task<IClusterClient> ConnectClient(IConfiguration configuration)
        {
            var endpoints = new IPEndPoint[]
            {
                IPEndPoint.Parse(configuration["Orleans:SiloIp"])
            };
            IClusterClient client;
            client = new ClientBuilder()
                //UseStaticClustering(new IPEndPoint(IPAddress.Parse("192.168.3.10"), 30000))
                //.UseLocalhostClustering()
                .UseStaticClustering(endpoints)
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = configuration["Orleans:ClusterId"];
                    options.ServiceId = configuration["Orleans:ServiceId"];
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .AddSimpleMessageStreamProvider(ScheduleTaskGrainBuilder.StreamProviderName)
                .Build();
            //await Task.Delay(3000);
            await client.Connect();
            Console.WriteLine("Client successfully connected to silo host \n");
            return client;
        }
    }
}