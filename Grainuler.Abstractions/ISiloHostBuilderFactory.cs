using Orleans.Hosting;

namespace Grainuler.Abstractions
{
    public interface ISiloHostBuilderFactory
    {
        ISiloHostBuilder GetHostBuilder(string stateStoreConnectionString, string clusterId, string serviceId, string pubSubStorageName, string? clusteringConnectionString=null, string? pubSubStorConnectionString=null);
    }
}