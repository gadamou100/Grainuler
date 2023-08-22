using Grainuler.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grainuler.RedisHosting
{
    public class RedisBuilderConfiguration : BuilderConfiguration
    {
        public bool UseJsonForStateStore { get; set; }=true;
        public bool UseJsonForPubSubStore { get; set; } = true;

        public static RedisBuilderConfiguration CreateDefault(string redisConnectionString, string pubSubStoreName= "PubSubStore", string cluserId="dev",string serviceId="dev", bool useFireAndForgetStreamingDelivery=true, bool useJsonForStateStore=true,bool useJsonForPubSubStore=true, int gatewayPort= 30000, int siloPort= 11111)
        {
            var clusterStoreConfig = new DataStoreConfiguration(0,redisConnectionString);
            var stateStoreConfig = new DataStoreConfiguration(1, redisConnectionString);
            var reminderStoreConfig = new DataStoreConfiguration(2, redisConnectionString);
            var pubSubStoreConfig = new DataStoreConfiguration(3, redisConnectionString);

            return new RedisBuilderConfiguration 
            {
                ClusterStoreConfiguration = clusterStoreConfig,
                StateStoreConfiguration = stateStoreConfig,
                ReminderStoreConfiguration = reminderStoreConfig,
                PubSubStoreConfiguration = pubSubStoreConfig,
                PubSubStoreName = pubSubStoreName,
                ClusterId = cluserId,
                ServiceId = serviceId,
                UseFireAndForgetStreamingDelivery = useFireAndForgetStreamingDelivery,
                UseJsonForPubSubStore = useJsonForPubSubStore,
                UseJsonForStateStore = useJsonForStateStore,
                GatewayPort = gatewayPort,
                SilopPort = siloPort
            };
        }

    }
}
