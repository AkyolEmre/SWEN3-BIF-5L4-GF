// Program.cs – gesamte Datei
using DMS.API.Services; // IMessageProducer, MessageProducer
using DMS.DAL.Repositories; // IDocumentRepository, DocumentRepository
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; // Explizit für AddScoped (hilft beim Resolver)
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// 1. Konfiguration einlesen (wird implizit gemacht, aber hier explizit gezeigt)
IConfiguration cfg = builder.Configuration;

// 2. Services registrieren
builder.Services.AddControllers(); // API-Controller
builder.Services.AddEndpointsApiExplorer(); // für Swagger
builder.Services.AddSwaggerGen(); // Swagger / OpenAPI

// 3. RabbitMQ ConnectionFactory
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

// 4. Eigener Message-Producer (mit Workaround: vollqualifizierte Namen, falls Resolver hakt)
builder.Services.AddScoped<DMS.API.Services.IMessageProducer, DMS.API.Services.MessageProducer>();

// 5. Repositories (Beispiel EF-Core)
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// 6. DBContext (falls du EF nutzt)
// builder.Services.AddDbContext<DmsDbContext>(opt =>
//     opt.UseSqlServer(cfg.GetConnectionString("Default")));

var app = builder.Build();

// 7. HTTP-Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DMS.API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // alle Controller-Routen registrieren

app.Run();