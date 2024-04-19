public interface IResponseGrain : IGrainWithStringKey
{
    Task<string> Compose(string phrase);
}

public class ResponseGrain : Grain, IResponseGrain
{
    private bool isNew;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        isNew = true;

        return base.OnActivateAsync(cancellationToken);
    }

    public Task<string> Compose(string phrase)
    {
        var res = $"{(isNew ? "<<NEW GRAIN!!!>>" : "")}{this.GetPrimaryKeyString()}, {phrase}";
        isNew = false;
        return Task.FromResult(res);
    }
}