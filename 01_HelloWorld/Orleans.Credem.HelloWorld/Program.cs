using Microsoft.Extensions.Hosting;

// REGISTRAZIONE ORLEANS "SERVER"
IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();
    })
    .UseConsoleLifetime();

var host = builder.Build();

// RISOLVO ISTANZA ORLEANS CLIENT
var client = host.Services.GetService(typeof(IClusterClient)) as IClusterClient;

host.RunAsync();

Thread.Sleep(1000);

Console.WriteLine(Environment.NewLine);


// GO ORLEANS GO!!!
Console.WriteLine(await client.GetGrain<IResponseGrain>("Hello").Compose("World"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Ciao").Compose("Mondo"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Hello").Compose("Gianni"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Ciao").Compose("Terra"));



Console.WriteLine("Premere INVIO per uscire...");
Console.ReadLine();
