using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;

var builder = Host.CreateApplicationBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();
    });

// *** REGISTRAZIONE E CONFIGURAZIONE DI ORLEANS ***
builder.UseOrleans(siloBuilder =>
{
    // CREAZIONE DEL CLUSTER PER AMBIENTI DI STAGING / PRODUZIONE
    siloBuilder.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "CodicePlasticoCluster";
        options.ServiceId = "OrleansUrlShortener";
    })
    .UseAdoNetClustering(options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("SqlOrleans");
        options.Invariant = "System.Data.SqlClient";
    })
    .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000);

    // REGISTRAZIONE REMINDERS PER AMBIENTI DI STAGING / PRODUZIONE
    siloBuilder.UseAdoNetReminderService(reminderOptions => {
        reminderOptions.ConnectionString = builder.Configuration.GetConnectionString("SqlOrleans");
        reminderOptions.Invariant = "System.Data.SqlClient";
    });

    // REGISTRAZIONE STORAGE PER AMBIENTI DI STAGING / PRODUZIONE
    siloBuilder.AddAdoNetGrainStorage("statistics_storage", storageOptions =>
    {
        storageOptions.ConnectionString = builder.Configuration.GetConnectionString("SqlOrleans");
        storageOptions.Invariant = "System.Data.SqlClient";
    }).AddAdoNetGrainStorage("urlshortener_storage", storageOptions =>
    {
        storageOptions.ConnectionString = builder.Configuration.GetConnectionString("SqlOrleans");
        storageOptions.Invariant = "System.Data.SqlClient";
    });
});

var host = builder.Build();

await host.RunAsync();

Console.WriteLine("Premere INVIO per uscire...");
Console.ReadLine();