using Microsoft.Extensions.Logging;
using Orleans.Credem.UrlShortener.Abstractions;
using Orleans.Credem.UrlShortener.Exceptions;
using Orleans.Runtime;

namespace Orleans.Credem.UrlShortener.Grains;

public class UrlShortenerGrain : Grain, IUrlShortenerGrain, IRemindable
{
    private IGrainReminder _reminder = null;
    private IDisposable _timer = null;

    private string FullUrl { get; set; }
    private bool IsOneShoot { get; set; }
    private int ValidFor { get; set; }
    private DateTime Expiration { get; set; }
    private int Invocations { get; set; }

    public async Task CreateShortUrl(string fullUrl, bool? isOneShoot, int? validFor)
    {
        this.FullUrl = fullUrl;
        this.IsOneShoot = isOneShoot ?? false;
        this.ValidFor = validFor ?? 60;
        this.Expiration = DateTime.UtcNow.AddSeconds(ValidFor);

        var statsGrain = GrainFactory.GetGrain<IUrlShortnerStatisticsGrain>("url_shortner_statistics");
        await statsGrain.RegisterNew();

        if (ValidFor >= 60)
        {
            _reminder = await this.RegisterOrUpdateReminder("shortenedRouteSegmentExpired",
               TimeSpan.Zero,
               TimeSpan.FromSeconds(ValidFor));
        }
        else
        {
            _timer = this.RegisterTimer(ReceiveTimer, null,
                TimeSpan.FromSeconds(ValidFor),
                TimeSpan.FromSeconds(ValidFor));
        }
    }

    public Task<string> GetUrl()
    {
        this.Invocations += 1;

        if (string.IsNullOrWhiteSpace(this.FullUrl)) { throw new ShortenedRouteSegmentNotFound(); }
        if (IsOneShoot && this.Invocations > 1) { throw new InvocationExcedeedException(); }
        if (DateTime.UtcNow > this.Expiration) { throw new ExpiredShortenedRouteSegmentException(); }

        return Task.FromResult(this.FullUrl);
    }

    // REMINDER
    public Task ReceiveReminder(string reminderName, TickStatus status)
    {
        return reminderName switch
        {
            "shortenedRouteSegmentExpired" => ShortenedRouteSegmentExpired(),
            _ => Task.CompletedTask
        };
    }

    private async Task ShortenedRouteSegmentExpired()
    {
        if (_reminder is not null)
        {
            await this.UnregisterReminder(_reminder);
            _reminder = null;
        }

        var statsGrain = GrainFactory.GetGrain<IUrlShortnerStatisticsGrain>("url_shortner_statistics");
        await statsGrain.RegisterExpiration();
    }

    // TIMER
    private Task ReceiveTimer(object _)
    {
        if (_timer is null)
            return Task.CompletedTask;

        _timer?.Dispose();
        
        var statsGrain = GrainFactory.GetGrain<IUrlShortnerStatisticsGrain>("url_shortner_statistics");
        return statsGrain.RegisterExpiration();
    }
}

