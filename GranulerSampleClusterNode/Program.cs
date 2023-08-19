using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Orleans.Reminders.Redis;
using Grainuler;
using Microsoft.Extensions.Configuration;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Grainuler.Abstractions;

namespace GranulerSampleClusterNode
{
    public class Program
    {

        public static int Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfiguration config = builder.Build();
            return RunMainAsync(config).Result;
        }



        private static async Task<int> RunMainAsync(IConfiguration configuration)
        {
            try
            {
                var host = await StartSilo(configuration);
                Console.WriteLine("\n\n Press Enter to terminate...\n\n");
                Console.ReadLine();

                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo(IConfiguration configuration)
        {
            var redisConnection = configuration["Redis:ServerAddress"];
            var clusterId = configuration["Orleans:ClusterId"];
            var serviceId = configuration["Orleans:ServiceId"];
            // define the cluster configuration
            var builder = new SiloHostBuilder()

                .UseRedisClustering(opt =>
                   {
                       opt.ConnectionString = redisConnection;
                       opt.Database = 0;
                   })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = clusterId;
                    options.ServiceId = serviceId;

                })
                .Configure<EndpointOptions>(options =>
                {
                    var adressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                    options.AdvertisedIPAddress = adressList.First();
                    options.GatewayPort = 30000;
                    options.SiloPort = 11111;
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ScheduleTaskGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole())
                 .AddRedisGrainStorage(ScheduleTaskGrainBuilder.ProviderStorageName, optionsBuilder => optionsBuilder.Configure(options =>
                 {
                     options.ConnectionString = redisConnection;
                     options.UseJson = true;
                     options.DatabaseNumber = 1;
                 }))
                 .ConfigureServices(services => services.UseRedisReminderService(options =>
                 {
                     options.ConnectionString = redisConnection;
                     options.DatabaseNumber = 2;
                 }))
                 .AddRedisGrainStorage(configuration["Orleans:PubSubGrainStorgeName"], optionsBuilder => optionsBuilder.Configure(options =>
                 {
                     options.ConnectionString = redisConnection;
                     options.UseJson = true;
                     options.DatabaseNumber = 3;
                 }))
                 .AddSimpleMessageStreamProvider(ScheduleTaskGrainBuilder.StreamProviderName, options =>
                 {
                     options.FireAndForgetDelivery = true;
                 })
                  .ConfigureServices((_, services) =>
                  {
                      services.AddSingleton<IPayloadInvoker, PayloadInvoker>();
                  });

            ;
            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}