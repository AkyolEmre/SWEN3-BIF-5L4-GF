using DMS.DAL;
using DMS.OCRWorker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Serilog;

// In OcrWorker's Program.cs
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<OcrWorker>();
// Add IConnectionFactory similar to DMS.API's Program.cs
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new ConnectionFactory
    {
        HostName = config["RabbitMQ:HostName"],
        UserName = config["RabbitMQ:UserName"],
        Password = config["RabbitMQ:Password"],
        Port = config.GetValue<int>("RabbitMQ:Port", 5672),
        VirtualHost = config["RabbitMQ:VirtualHost"] ?? "/"
    };
});
builder.Services.AddLogging();
await builder.Build().RunAsync();