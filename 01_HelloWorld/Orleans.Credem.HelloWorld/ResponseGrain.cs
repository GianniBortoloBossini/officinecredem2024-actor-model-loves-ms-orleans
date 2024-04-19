public interface IResponseGrain : IGrainWithStringKey
{
    Task<string> Compose(string phrase);
}

public class ResponseGrain : Grain, IResponseGrain
{
    public Task<string> Compose(string phrase)
        => Task.FromResult($"{this.GetPrimaryKeyString()}, {phrase}");
}