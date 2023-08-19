using Grainuler.Abstractions;
using Grainuler.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;



namespace Grainuler.RedisHosting
{
    public class RedisSiloHostBuilderFactory : ISiloHostBuilderFactory<RedisBuilderConfiguration>
    {
        public ISiloHostBuilder GetHostBuilder(RedisBuilderConfiguration configuration)
        {
            var builder = new SiloHostBuilder()

             .UseRedisClustering(opt =>
             {
                 opt.ConnectionString = configuration.ClusterStoreConfiguration.ConnectionString;
                 opt.Database = configuration.ClusterStoreConfiguration.DbNumber;
             })
             .Configure<ClusterOptions>(options =>
             {
                 options.ClusterId = configuration.ClusterId;
                 options.ServiceId = configuration.ServiceId;

             })
             .Configure<EndpointOptions>(options =>
             {
                 var adressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                 options.AdvertisedIPAddress = adressList.First();
                 options.GatewayPort = configuration.GatewayPort;
                 options.SiloPort = configuration.SilopPort;
             })
             .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ScheduleTaskGrain).Assembly).WithReferences())
             .ConfigureLogging(logging => logging.AddConsole())
             .AddRedisGrainStorage(ScheduleTaskGrainBuilder.ProviderStorageName, optionsBuilder => optionsBuilder.Configure(options =>
              {
                  options.ConnectionString = configuration.StateStoreConfiguration.ConnectionString;
                  options.UseJson = configuration.UseJsonForStateStore;
                  options.DatabaseNumber = configuration.StateStoreConfiguration.DbNumber;
              }))
              .ConfigureServices(services => services.UseRedisReminderService(options =>
              {
                  options.ConnectionString = configuration.ReminderStoreConfiguration.ConnectionString;
                  options.DatabaseNumber = configuration.ReminderStoreConfiguration.DbNumber;

              }))
              .AddRedisGrainStorage(configuration.PubSubStoreName, optionsBuilder => optionsBuilder.Configure(options =>
              {
                  options.ConnectionString = configuration.PubSubStoreConfiguration.ConnectionString;
                  options.UseJson = configuration.UseJsonForPubSubStore;
                  options.DatabaseNumber = configuration.PubSubStoreConfiguration.DbNumber;
              }))
              .AddSimpleMessageStreamProvider(ScheduleTaskGrainBuilder.StreamProviderName, options =>
              {
                  options.FireAndForgetDelivery = configuration.UseFireAndForgetStreamingDelivery;
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
