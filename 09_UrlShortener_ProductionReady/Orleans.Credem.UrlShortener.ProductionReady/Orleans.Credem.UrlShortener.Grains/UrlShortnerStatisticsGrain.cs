using Orleans.Credem.UrlShortener.Abstractions;
using Orleans.Runtime;

namespace Orleans.Credem.UrlShortener.Grains;

[GenerateSerializer]
public class StatisticsState
{
    [Id(0)]
    public int TotalActivations { get; set; }
    [Id(1)]
    public int TotalActive { get; set; }
}

public class UrlShortnerStatisticsGrain : Grain, IUrlShortnerStatisticsGrain
{
    private readonly IPersistentState<StatisticsState> state;

    public UrlShortnerStatisticsGrain(
        [PersistentState(stateName: "url-shortner", storageName: "statistics_storage")] IPersistentState<StatisticsState> state)
    {
        this.state = state;
    }

    public Task<int> GetTotal()
    {
        return Task.FromResult(this.state.State.TotalActivations);
    }

    public Task<int> GetTotalActive()
    {
        return Task.FromResult(this.state.State.TotalActive);
    }

    public Task RegisterNew()
    {
        this.state.State.TotalActivations++;
        this.state.State.TotalActive++;

        return this.state.WriteStateAsync();
    }

    public Task RegisterExpiration()
    {
        this.state.State.TotalActive--;

        return this.state.WriteStateAsync();
    }
}
