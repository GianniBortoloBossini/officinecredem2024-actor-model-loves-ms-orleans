using Orleans.Credem.UrlShortener.Abstractions;

namespace Orleans.Credem.UrlShortener.Grains;

public class UrlShortnerStatisticsGrain : Grain, IUrlShortnerStatisticsGrain
{
    public int TotalActivations { get; set; }

    public Task<int> GetTotal()
    {
        return Task.FromResult(TotalActivations);
    }

    public Task RegisterNew()
    {
        this.TotalActivations++;
        return Task.CompletedTask;
    }
}
