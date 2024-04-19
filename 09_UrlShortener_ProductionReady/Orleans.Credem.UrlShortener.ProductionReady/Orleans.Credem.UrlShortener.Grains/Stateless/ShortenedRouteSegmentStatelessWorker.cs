using Orleans.Concurrency;

namespace Orleans.Credem.UrlShortener.Grains.Stateless;
public interface IShortenedRouteSegmentStatelessWorker : IGrainWithIntegerKey
{
    Task<string> CreateRouteSegment();
}

[StatelessWorker]
public class ShortenedRouteSegmentStatelessWorker : Grain, IShortenedRouteSegmentStatelessWorker
{
    public Task<string> CreateRouteSegment()
        => Task.FromResult(Guid.NewGuid().GetHashCode().ToString("X"));
}
