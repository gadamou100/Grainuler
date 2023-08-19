using Orleans.Hosting;
using Grainuler.Configuration;
namespace Grainuler.Abstractions
{
    public interface ISiloHostBuilderFactory<TConfiguration> where TConfiguration : BuilderConfiguration
    {
        ISiloHostBuilder GetHostBuilder(TConfiguration configuration);
    }
}