using Orleans.Credem.UrlShortener.Abstractions;
using Orleans.Credem.UrlShortener.Tests.Fixtures;

namespace Orleans.Credem.UrlShortener.Tests;

internal class UrlShortnerStatisticsGrainTests
{
    private ClusterFixture fixture;

    [SetUp]
    public void SetUp()
    {
        fixture = new ClusterFixture();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        fixture.Cluster.StopAllSilos();
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(100)]
    public async Task GetTotal_Should_Count_New_Registrations(int registrationTimes)
    {
        // ARRANGE
        var statisticsGrain = fixture.Cluster.GrainFactory.GetGrain<IUrlShortnerStatisticsGrain>("url_shortner_statistics");

        List<Task> tasks = new();
        for (int i = 0; i < registrationTimes; i++)
            tasks.Add(statisticsGrain.RegisterNew());

        await Task.WhenAll(tasks);

        // ACT

        /* PERCHE' QUESTO DELAY? 
         * Provare a decommentare gli attributi OneWay e ReadOnly sull'interfaccia del grano e scoprirai che i test falliscono.
         * E' legato alle migliorie alle performance introdotte da Orleans nella gestione del multi-threading in presenza di quei due attributi:
         * await non attende che il Task sia effettivamente terminato! 
         * Per questo è necessario attendere 
         */
        //await Task.Delay(100);

        var result = await statisticsGrain.GetTotal();

        // ASSERT
        Assert.That(result, Is.EqualTo(registrationTimes));
    }

    [TestCase(2)]
    [TestCase(60)]
    public async Task GetTotal_Registration_Expiration(int validFor)
    {
        // ARRANGE
        var shortenerGrain = fixture.Cluster.GrainFactory.GetGrain<IUrlShortenerGrain>("aaaa");
        var statisticsGrain = fixture.Cluster.GrainFactory.GetGrain<IUrlShortnerStatisticsGrain>("url_shortner_statistics");

        // ACT
        await shortenerGrain.CreateShortUrl("http://www.codiceplastico.com", false, validFor);

        // ASSERT
        var total = await statisticsGrain.GetTotal();
        var totalActive = await statisticsGrain.GetTotalActive();
        Assert.That(total, Is.EqualTo(1));
        Assert.That(totalActive, Is.EqualTo(1));
        
        await Task.Delay(TimeSpan.FromSeconds(validFor));

        total = await statisticsGrain.GetTotal();
        totalActive = await statisticsGrain.GetTotalActive();
        Assert.That(total, Is.EqualTo(1));
        Assert.That(totalActive, Is.EqualTo(0));
    }
}
