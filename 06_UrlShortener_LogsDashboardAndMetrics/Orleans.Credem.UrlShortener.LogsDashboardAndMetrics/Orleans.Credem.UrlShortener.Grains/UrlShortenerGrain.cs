using Microsoft.Extensions.Logging;
using Orleans.Credem.UrlShortener.Abstractions;
using Orleans.Credem.UrlShortener.Exceptions;

namespace Orleans.Credem.UrlShortener.Grains;

public class UrlShortenerGrain : Grain, IUrlShortenerGrain
{
    private readonly ILogger<IUrlShortenerGrain> logger;

    public UrlShortenerGrain(ILogger<IUrlShortenerGrain> logger)
    {
        this.logger = logger;
    }

    private string FullUrl { get; set; }
    private bool IsOneShoot { get; set; }
    private int ValidFor { get; set; }
    private DateTime Expiration { get; set; }
    private int Invocations { get; set; }

    public Task CreateShortUrl(string fullUrl, bool? isOneShoot, int? validFor)
    {
        logger.LogInformation("CreateShortUrl");

        this.FullUrl = fullUrl;
        this.IsOneShoot = isOneShoot ?? false;
        this.ValidFor = validFor ?? 60;
        this.Expiration = DateTime.UtcNow.AddSeconds(ValidFor);

        var statsGrain = GrainFactory.GetGrain<IUrlShortnerStatisticsGrain>("url_shortner_statistics");
        return statsGrain.RegisterNew();
    }

    public Task<string> GetUrl()
    {
        logger.LogInformation("GetUrl");

        this.Invocations += 1;

        if (string.IsNullOrWhiteSpace(this.FullUrl)) { throw new ShortenedRouteSegmentNotFound(); }
        if (IsOneShoot && this.Invocations > 1) { throw new InvocationExcedeedException(); }
        if (DateTime.UtcNow > this.Expiration) { throw new ExpiredShortenedRouteSegmentException(); }

        return Task.FromResult(this.FullUrl);
    }
}

