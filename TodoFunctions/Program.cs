using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// make sure configuration loads local.settings.json etc.
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("local.settings.json", optional: true)
    .AddEnvironmentVariables();

builder.ConfigureFunctionsWebApplication();

// 👇 helper goes here
string Get(string key) =>
    builder.Configuration[key] ??
    builder.Configuration[$"Values:{key}"] ??
    throw new InvalidOperationException($"{key} missing");

// 👇 now safely pull your Cosmos settings
var endpoint = Get("Cosmos__EndpointUri");
var authKey = Get("Cosmos__PrimaryKey");
var dbName = Get("Cosmos__DatabaseName");
var contName = Get("Cosmos__ContainerName");

// register CosmosClient + Container
builder.Services.AddSingleton(_ => new CosmosClient(endpoint, authKey));
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<CosmosClient>();
    client.CreateDatabaseIfNotExistsAsync(dbName).GetAwaiter().GetResult();
    client.GetDatabase(dbName).CreateContainerIfNotExistsAsync(
        contName, "/partitionKey", 400).GetAwaiter().GetResult();
    return client.GetContainer(dbName, contName);
});

var app = builder.Build();
app.Run();
