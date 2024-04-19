using Orleans.Concurrency;

public interface IResponseGrain : IGrainWithStringKey
{
    Task<(DateTime, string, DateTime)> Compose(string phrase);
}

[Reentrant]
public class ResponseGrain : Grain, IResponseGrain
{
    public async Task<(DateTime, string, DateTime)> Compose(string phrase)
    {
        var begin = DateTime.Now;
        await Task.Delay(500 + Random.Shared.Next(-100, 100));
        return (begin, $"{this.GetPrimaryKeyString()}, {phrase}", DateTime.Now);
    }
}