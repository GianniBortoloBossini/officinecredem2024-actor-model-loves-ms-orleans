using Orleans.Concurrency;

namespace Orleans.Credem.UrlShortener.Abstractions;
public interface IUrlShortnerStatisticsGrain : IGrainWithStringKey
{
    //[OneWay]
    Task RegisterNew();
    //[ReadOnly]
    Task<int> GetTotal();
}
