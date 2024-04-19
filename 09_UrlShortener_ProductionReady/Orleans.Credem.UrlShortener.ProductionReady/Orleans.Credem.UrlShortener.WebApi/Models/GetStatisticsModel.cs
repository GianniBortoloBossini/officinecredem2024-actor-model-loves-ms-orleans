namespace Orleans.Credem.UrlShortener.WebApi.Models;

public class GetStatisticsModel
{
    public class Response
    {
        public int TotalActivations { get; set; }
        public int TotalActive { get; set; }
    }
}