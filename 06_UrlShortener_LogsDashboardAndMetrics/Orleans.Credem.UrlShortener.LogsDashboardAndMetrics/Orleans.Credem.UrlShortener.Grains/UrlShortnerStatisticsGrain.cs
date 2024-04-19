using Microsoft.Extensions.Logging;
using Orleans.Credem.UrlShortener.Abstractions;

namespace Orleans.Credem.UrlShortener.Grains;

public class UrlShortnerStatisticsGrain : Grain, IUrlShortnerStatisticsGrain
{
    private readonly ILogger<IUrlShortnerStatisticsGrain> logger;

    public UrlShortnerStatisticsGrain(ILogger<IUrlShortnerStatisticsGrain> logger)
    {
        this.logger = logger;
    }

    public int TotalActivations { get; set; }

    public Task<int> GetTotal()
    {
        logger.LogInformation("GetTotal");

        return Task.FromResult(TotalActivations);
    }

    public Task RegisterNew()
    {
        logger.LogInformation("RegisterNew");

        this.TotalActivations++;
        return Task.CompletedTask;
    }
}
