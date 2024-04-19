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

Console.WriteLine(Environment.NewLine);
Console.WriteLine(await client.GetGrain<IResponseGrain>("Hello").Compose("World"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Ciao").Compose("Mondo"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Hello").Compose("Gianni"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Ciao").Compose("Terra"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Hello").Compose("Tom"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Ciao").Compose("Amico"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Hello").Compose("Claudio"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Ciao").Compose("Italia90"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Hello").Compose("Marco"));

Console.WriteLine("Premere INVIO per uscire...");
Console.ReadLine();
