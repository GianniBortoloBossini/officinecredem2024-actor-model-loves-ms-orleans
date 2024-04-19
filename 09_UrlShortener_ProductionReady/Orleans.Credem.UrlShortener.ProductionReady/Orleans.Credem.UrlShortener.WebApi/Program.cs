using Microsoft.AspNetCore.Mvc;
using Orleans.Configuration;
using Orleans.Credem.UrlShortener.Abstractions;
using Orleans.Credem.UrlShortener.Exceptions;
using Orleans.Credem.UrlShortener.Grains.Stateless;
using Orleans.Credem.UrlShortener.WebApi.Models;

var builder = WebApplication.CreateBuilder(args);

// *** REGISTRAZIONE E CONFIGURAZIONE DI ORLEANS ***
builder.Host.UseOrleans(siloBuilder =>
{
    if (builder.Environment.IsDevelopment())
    {
        siloBuilder.UseLocalhostClustering();

        // REGISTRAZIONE REMINDERS
        siloBuilder.UseInMemoryReminderService();

        // REGISTRAZIONE STORAGE IN MEMORIA
        siloBuilder.AddMemoryGrainStorage("statistics_storage");
        siloBuilder.AddMemoryGrainStorage("urlshortener_storage");
    }
    else
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
    }
});

// Registrazione dei servizi del supporto Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Aggiunta del supporto Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/shorten",
    async ([FromBody] PostCreateUrlShortnerModel.Request data, IClusterClient client, HttpRequest request) =>
    {
        var host = $"{request.Scheme}://{request.Host.Value}";

        // validazione del campo Url
        if (string.IsNullOrWhiteSpace(data.Url) && Uri.IsWellFormedUriString(data.Url, UriKind.Absolute) is false)
            return Results.BadRequest($"Valore del campo URL non valido.");

        // Creazione di un ID univoco
        var routeSegmentWorker = client.GetGrain<IShortenedRouteSegmentStatelessWorker>(0);
        var routeSegment = await routeSegmentWorker.CreateRouteSegment();

        // Creazione del grano 
        var shortenerGrain = client.GetGrain<IUrlShortenerGrain>(routeSegment);
        await shortenerGrain.CreateShortUrl(data.Url, data.IsOneShoot, data.DurationInSeconds);

        // Creazione della risposta
        var resultBuilder = new UriBuilder(host)
        {
            Path = $"/go/{routeSegment}"
        };

        return Results.Ok(new PostCreateUrlShortnerModel.Response { Url = resultBuilder.Uri });
    })
    .WithName("Shorten")
    .WithDescription("Endpoint per l'abbreviazione degli url")
    .Produces<PostCreateUrlShortnerModel.Response>()
    .WithOpenApi();


app.MapGet("/go/{shortenedRouteSegment:required}",
    async (IClusterClient client, string shortenedRouteSegment) =>
    {
        // Recupero della reference al grano identificato dall'ID univoco
        var shortenedGrain = client.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);

        try
        {
            // Recupero dell'url dal grano e redirect
            var url = await shortenedGrain.GetUrl();
            return Results.Redirect(url);
        }
        catch (ExpiredShortenedRouteSegmentException) { return Results.BadRequest(); }
        catch (InvocationExcedeedException) { return Results.StatusCode(429); }
        catch (ShortenedRouteSegmentNotFound) { return Results.NotFound(); }
    })
    .WithName("Go")
    .WithDescription("Endpoint per il recupero dell'url abbreviato")
    .WithOpenApi();

app.MapGet("/statistics",
    async (IClusterClient client) =>
    {
        // Recupero della reference al grano identificato dall'ID univoco
        var shortenedGrain = client.GetGrain<IUrlShortnerStatisticsGrain>("url_shortner_statistics");

        // Recupero della statistiche tramite metodo GetTotal del grano
        var total = await shortenedGrain.GetTotal();
        var totalActive = await shortenedGrain.GetTotalActive();

        return Results.Ok(new GetStatisticsModel.Response { TotalActivations = total, TotalActive = totalActive });
    })
    .WithName("Statistics")
    .WithDescription("Endpoint per il recupero delle statistiche")
    .WithOpenApi(); ;

app.Run();