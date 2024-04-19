using Orleans.TestingHost;

namespace Orleans.Credem.UrlShortener.Tests.Fixtures;
public class ClusterFixture
{
    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();

        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public TestCluster Cluster { get; }
}

public class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        
    }
}