using Grainuler.RedisHosting;
using Microsoft.Extensions.Configuration;
using Orleans.Hosting;

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
            var pubSubStotageName = configuration["Orleans:PubSubGrainStorgeName"];
            // define the cluster configuration
            var builderFactory = new RedisSiloHostBuilderFactory();
            var builder = builderFactory.GetHostBuilder(redisConnection, clusterId, serviceId, pubSubStotageName);
            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}