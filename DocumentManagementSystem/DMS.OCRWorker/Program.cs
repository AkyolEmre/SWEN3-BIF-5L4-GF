using DMS.OCRWorker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Minio;
using Microsoft.Extensions.Configuration;
using DMS.API.Services;

var builder = Host.CreateApplicationBuilder(args);

// Worker registrieren
builder.Services.AddHostedService<OcrWorker>();

// MinIO Service
builder.Services.AddSingleton<IMinIOService, MinIOService>();

// RabbitMQ
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new ConnectionFactory
    {
        HostName = config["RabbitMQ:HostName"] ?? "rabbitmq",
        UserName = config["RabbitMQ:UserName"] ?? "guest",
        Password = config["RabbitMQ:Password"] ?? "guest",
        Port = config.GetValue<int>("RabbitMQ:Port", 5672)
    };
});

var host = builder.Build();
await host.RunAsync();