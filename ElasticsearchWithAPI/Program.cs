using Serilog;
using System.Reflection;
using Serilog.Sinks.Elasticsearch;
using Serilog.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
configureLogging();
builder.Host.UseSerilog();

//var configuration = builder.Configuration;
//var environment = builder.Environment;

//#region Serilog

//Log.Logger = new LoggerConfiguration()
//    .Enrich.FromLogContext()
//    .Enrich.WithMachineName()
//    .WriteTo.Debug()
//    .WriteTo.Console()
//    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
//    .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment.EnvironmentName))
//    .Enrich.WithProperty("Environment", environment.EnvironmentName)
//    .ReadFrom.Configuration(configuration)
//    .CreateLogger();


//static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
//{
//    return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
//    {
//        AutoRegisterTemplate = true,
//        IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
//    };
//}

//builder.Services.AddSerilog(Log.Logger);
//#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


void configureLogging()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile(
            $"appsettings.{environment}.json", optional: true
        ).Build();
    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .WriteTo.Debug()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
        .Enrich.WithProperty("Environment", environment)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
{
    return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace('.', '-')}-{environment.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
        NumberOfReplicas = 1,
        NumberOfShards = 2
    };
}