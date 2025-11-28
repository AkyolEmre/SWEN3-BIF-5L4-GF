using DMS.API.Services;           // IMessageProducer + MessageProducer
using DMS.DAL;                    // ApplicationDbContext
using DMS.DAL.Repositories;       // IDocumentRepository, DocumentRepository
using Microsoft.EntityFrameworkCore;
using Minio;                       // <-- WICHTIG: Minio-Paket muss installiert sein!
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// --------------------------
// 1. Controller + Swagger
// --------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --------------------------
// 2. PostgreSQL (EF Core + Npgsql)
// --------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------------
// 3. Repositories
// --------------------------
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// --------------------------
// 4. MinIO Service
// --------------------------
builder.Services.AddScoped<IMinIOService, MinIOService>();

// --------------------------
// 5. RabbitMQ ConnectionFactory
// --------------------------
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

// --------------------------
// 6. MessageProducer – MUSS IMessageProducer implementieren!
// --------------------------
builder.Services.AddScoped<IMessageProducer, MessageProducer>();

var app = builder.Build();

// --------------------------
// 7. HTTP Pipeline
// --------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DMS.API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();