using Microsoft.Extensions.Hosting;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();
    })
    .UseConsoleLifetime();

var host = builder.Build();

var client = host.Services.GetService(typeof(IClusterClient)) as IClusterClient;

host.RunAsync();

Thread.Sleep(1000);

List<Task<(DateTime, string, DateTime)>> tasks = [
    client.GetGrain<IResponseGrain>("Hello").Compose("World"),
    client.GetGrain<IResponseGrain>("Hello").Compose("Mondo"),
    client.GetGrain<IResponseGrain>("Hello").Compose("Gianni"),
    client.GetGrain<IResponseGrain>("Hello").Compose("Terra"),
    client.GetGrain<IResponseGrain>("Hello").Compose("Tom"),
    client.GetGrain<IResponseGrain>("Hello").Compose("Amico"),
    client.GetGrain<IResponseGrain>("Hello").Compose("Claudio"),
    client.GetGrain<IResponseGrain>("Hello").Compose("Italia90"),
    client.GetGrain<IResponseGrain>("Hello").Compose("Marco")
];
await Task.WhenAll(tasks);

List<(DateTime, string, DateTime)> phrases = new List<(DateTime, string, DateTime)>();
foreach (var task in tasks)
    phrases.Add(task.Result);

Console.WriteLine(Environment.NewLine);

foreach (var phrase in phrases.OrderBy(p => p.Item1))
    Console.WriteLine($"{phrase.Item1:yyyyMMdd HH:mm:ss.fff} - {phrase.Item3::yyyyMMdd HH:mm:ss.fff} - {phrase.Item2}");

Console.WriteLine("Premere INVIO per uscire...");
Console.ReadLine();
