using Microsoft.Extensions.Hosting;
using Orleans.Configuration;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();
        silo.Configure<GrainCollectionOptions>(options =>
        {
            // Imposta Collection Age
            options.CollectionAge = TimeSpan.FromSeconds(90);

            // Sovrascrive il valore di CollectionAge per il grain MyGrainImplementation
            //options.ClassSpecificCollectionAge[typeof(MyGrainImplementation).FullName] =
            //    TimeSpan.FromMinutes(5);
        });
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

for (int a = 160; a >= 0; a--)
{
    Console.Write("\rRestart in {0:000}s...", a);
    Thread.Sleep(1000);
}

Console.WriteLine(Environment.NewLine);
Console.WriteLine(await client.GetGrain<IResponseGrain>("Hello").Compose("World"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Ciao").Compose("Mondo"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Hello").Compose("Gianni"));
Console.WriteLine(await client.GetGrain<IResponseGrain>("Ciao").Compose("Terra"));

Console.WriteLine("Premere INVIO per uscire...");
Console.ReadLine();
