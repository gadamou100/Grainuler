using Grainuler.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;



namespace Grainuler.RedisHosting
{
    public class RedisSiloHostBuilderFactory : ISiloHostBuilderFactory
    {
        public ISiloHostBuilder GetHostBuilder(string stateStoreConnectionString, string clusterId, string serviceId, string pubSubStorageName, string? clusteringConnectionString = null, string? pubSubStorConnectionString = null)
        {
            clusteringConnectionString ??= stateStoreConnectionString;
            pubSubStorConnectionString ??= stateStoreConnectionString;
            var builder = new SiloHostBuilder()

             .UseRedisClustering(opt =>
             {
                 opt.ConnectionString = stateStoreConnectionString;
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
                  options.ConnectionString = stateStoreConnectionString;
                  options.UseJson = true;
                  options.DatabaseNumber = 1;
              }))
              .ConfigureServices(services => services.UseRedisReminderService(options =>
              {
                  options.ConnectionString = stateStoreConnectionString;
                  options.DatabaseNumber = 2;
              }))
              .AddRedisGrainStorage(pubSubStorageName, optionsBuilder => optionsBuilder.Configure(options =>
              {
                  options.ConnectionString = stateStoreConnectionString;
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
            return builder;
        }
    }
}
