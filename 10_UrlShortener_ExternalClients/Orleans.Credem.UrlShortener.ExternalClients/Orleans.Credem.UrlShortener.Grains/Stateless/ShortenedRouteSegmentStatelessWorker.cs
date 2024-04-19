using Orleans.Concurrency;
using Orleans.Credem.UrlShortener.Abstractions.Stateless;

namespace Orleans.Credem.UrlShortener.Grains.Stateless;

[StatelessWorker]
public class ShortenedRouteSegmentStatelessWorker : Grain, IShortenedRouteSegmentStatelessWorker
{
    public Task<string> CreateRouteSegment()
        => Task.FromResult(Guid.NewGuid().GetHashCode().ToString("X"));
}
