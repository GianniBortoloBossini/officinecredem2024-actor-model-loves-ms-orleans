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

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{reason.ReasonCode} - {reason.Description}");

        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    public Task<string> Compose(string phrase)
    {
        // Disabilita attore al termine della chiamata
        //DeactivateOnIdle();

        // Impostazione delay aggiuntivo quando avviene disattivazione
        //DelayDeactivation(TimeSpan.FromSeconds(10));

        var res = $"{(isNew ? "<<NEW GRAIN!!!>>": "")}{this.GetPrimaryKeyString()}, {phrase}";
        isNew = false;
        return Task.FromResult(res);
    }
}