using System.Threading.Tasks;

namespace Orleans.Credem.UrlShortener.Abstractions;
public interface IUrlShortnerStatisticsGrain : IGrainWithStringKey
{
    Task RegisterNew();
    Task RegisterExpiration();
    Task<int> GetTotal();
    Task<int> GetTotalActive();
}
