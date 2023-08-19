namespace Grainuler.Configuration
{
    public class BuilderConfiguration
    {
        public DataStoreConfiguration ClusterStoreConfiguration { get; set; }
        public DataStoreConfiguration StateStoreConfiguration { get; set; }
        public DataStoreConfiguration ReminderStoreConfiguration { get; set; }
        public DataStoreConfiguration PubSubStoreConfiguration { get; set; }

        public string ClusterId { get; set; } = "dev";
        public string ServiceId { get; set; } = "dev";
        public int GatewayPort { get; set; } = 30000;
        public int SilopPort { get; set; } = 11111;
        public string PubSubStoreName { get; set; } = "PubSubStore";

        public bool UseFireAndForgetStreamingDelivery { get; set; } = true;

    }
    public struct DataStoreConfiguration
    {
        public int DbNumber { get; private set; }
        public string ConnectionString { get; private set; }

        public DataStoreConfiguration(int dbNumber, string connectionString)
        {
            DbNumber = dbNumber;
            ConnectionString = connectionString;
        }
    }
}