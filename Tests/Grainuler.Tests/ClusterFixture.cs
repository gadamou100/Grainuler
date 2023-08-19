using System;
using Orleans.TestingHost;

public class ClusterFixture : IDisposable
{
    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose() => Cluster.StopAllSilos();

    public TestCluster Cluster { get; }
}
