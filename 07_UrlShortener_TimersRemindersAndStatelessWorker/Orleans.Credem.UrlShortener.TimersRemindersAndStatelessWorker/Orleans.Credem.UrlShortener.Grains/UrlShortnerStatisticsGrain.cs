using Microsoft.Extensions.Logging;
using Orleans.Credem.UrlShortener.Abstractions;

namespace Orleans.Credem.UrlShortener.Grains;

public class UrlShortnerStatisticsGrain : Grain, IUrlShortnerStatisticsGrain
{
    public int TotalActivations { get; set; }
    public int TotalActive { get; set; }

    public Task<int> GetTotal()
    {
        return Task.FromResult(TotalActivations);
    }

    public Task<int> GetTotalActive()
    {
        return Task.FromResult(TotalActive);
    }

    public Task RegisterNew()
    {
        this.TotalActivations++;
        this.TotalActive++;

        return Task.CompletedTask;
    }

    public Task RegisterExpiration()
    {
        this.TotalActive--;

        return Task.CompletedTask;
    }
}
