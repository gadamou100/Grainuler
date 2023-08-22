using Grainuler;
using Grainuler.Abstractions;
using Grainuler.RedisHosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.TestingHost;
using System;
using System.Linq;
using System.Net;


public class ClusterFixture : IDisposable
{
    public ClusterFixture()
    {

        var builderFactory = new RedisSiloHostBuilderFactory();
        var config = RedisBuilderConfiguration.CreateDefault("redis:6379", cluserId: "test", serviceId: "test", pubSubStoreName: "Grainuler");

        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose() => Cluster.StopAllSilos();

    public TestCluster Cluster { get; }

    public class TestSiloConfigurations : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<IPayloadInvoker, PayloadInvoker>();
            })
              .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "test";
                    options.ServiceId = "test";

                })
                .Configure<EndpointOptions>(options =>
                   {
                       var adressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                       options.AdvertisedIPAddress = adressList.First();
                       options.GatewayPort = 30000;
                       options.SiloPort = 11111;
                   })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ScheduleTaskGrain).Assembly).WithReferences())
              .AddMemoryGrainStorage(ScheduleTaskGrainBuilder.ProviderStorageName)
              .UseInMemoryReminderService()
              .AddMemoryGrainStorage("PubSubStore", optBuilder => optBuilder.Configure(opt =>
              {
                  opt.NumStorageGrains=2;
              }))
              .AddSimpleMessageStreamProvider(ScheduleTaskGrainBuilder.StreamProviderName, options =>
              {
                  options.FireAndForgetDelivery =true;
              })
              ;
        }
    }
}

