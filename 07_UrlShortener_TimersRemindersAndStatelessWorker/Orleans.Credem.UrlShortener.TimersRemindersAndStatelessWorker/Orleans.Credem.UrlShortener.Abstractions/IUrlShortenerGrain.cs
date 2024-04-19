using System.Threading.Tasks;

namespace Orleans.Credem.UrlShortener.Abstractions;

public interface IUrlShortenerGrain : IGrainWithStringKey
{
    Task CreateShortUrl(string fullUrl, bool? isOneShoot, int? validFor);
    Task<string> GetUrl();
}
