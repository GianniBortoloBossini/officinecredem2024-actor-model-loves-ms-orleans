using Microsoft.Extensions.Logging;
using Orleans.Credem.UrlShortener.Abstractions;
using Orleans.Credem.UrlShortener.Exceptions;
using Orleans.Runtime;

namespace Orleans.Credem.UrlShortener.Grains;

[GenerateSerializer]
public class UrlShortenerState
{
    [Id(0)]
    public string FullUrl { get; set; }
    [Id(1)]
    public bool IsOneShoot { get; set; }
    [Id(2)]
    public int ValidFor { get; set; }
    [Id(3)]
    public DateTime Expiration { get; set; }
    [Id(4)]
    public int Invocations { get; set; }
}

public class UrlShortenerGrain : Grain, IUrlShortenerGrain, IRemindable
{
    private IGrainReminder _reminder = null;
    private IDisposable _timer = null;
    private readonly IPersistentState<UrlShortenerState> state;

    public UrlShortenerGrain(
        [PersistentState(stateName: "url-shortner", storageName: "urlshortener_storage")] IPersistentState<UrlShortenerState> state)
    {
        this.state = state;
    }

    public async Task CreateShortUrl(string fullUrl, bool? isOneShoot, int? validFor)
    {
        this.state.State.FullUrl = fullUrl;
        this.state.State.IsOneShoot = isOneShoot ?? false;
        this.state.State.ValidFor = validFor ?? 60;
        this.state.State.Expiration = DateTime.UtcNow.AddSeconds(this.state.State.ValidFor);

        var statsGrain = GrainFactory.GetGrain<IUrlShortnerStatisticsGrain>("url_shortner_statistics");
        await statsGrain.RegisterNew();

        if (this.state.State.ValidFor >= 60)
        {
            _reminder = await this.RegisterOrUpdateReminder("shortenedRouteSegmentExpired",
               TimeSpan.Zero,
               TimeSpan.FromSeconds(this.state.State.ValidFor));
        }
        else
        {
            _timer = this.RegisterTimer(ReceiveTimer, null,
                TimeSpan.FromSeconds(this.state.State.ValidFor),
                TimeSpan.FromSeconds(this.state.State.ValidFor));
        }

        await this.state.WriteStateAsync();
    }

    public Task<string> GetUrl()
    {
        this.state.State.Invocations += 1;

        if (string.IsNullOrWhiteSpace(this.state.State.FullUrl)) { throw new ShortenedRouteSegmentNotFound(); }
        if (this.state.State.IsOneShoot && this.state.State.Invocations > 1) { throw new InvocationExcedeedException(); }
        if (DateTime.UtcNow > this.state.State.Expiration) { throw new ExpiredShortenedRouteSegmentException(); }

        return Task.FromResult(this.state.State.FullUrl);
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

