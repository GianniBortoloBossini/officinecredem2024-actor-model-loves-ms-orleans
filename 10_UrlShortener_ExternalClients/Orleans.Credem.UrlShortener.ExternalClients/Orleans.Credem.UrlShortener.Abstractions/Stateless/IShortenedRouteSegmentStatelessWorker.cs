namespace Orleans.Credem.UrlShortener.Abstractions.Stateless;

public interface IShortenedRouteSegmentStatelessWorker : IGrainWithIntegerKey
{
    Task<string> CreateRouteSegment();
}
