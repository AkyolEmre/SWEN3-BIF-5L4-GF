using System.Text;
using System.Text.Json;
using DMS.API.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DMS.OCRWorker
{
    public sealed class OcrWorker : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly IMinIOService _minioService;
        private readonly ILogger<OcrWorker> _logger;

        public OcrWorker(
            IConnectionFactory factory,
            IMinIOService minioService,
            ILogger<OcrWorker> logger)
        {
            _factory = factory;
            _minioService = minioService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OCR Worker wird gestartet...");

            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            const string queueName = "ocr-queue";
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<OcrMessage>(json)!;

                    _logger.LogInformation("OCR gestartet für DocumentId {Id}, Object: {Object}",
                        message.DocumentId, message.ObjectName);

                    // PDF aus MinIO holen
                    using var pdfStream = await _minioService.GetFileAsync(message.ObjectName);

                    // OCR durchführen – korrekter Logger-Typ!
                    var ocrService = new OcrService(_logger);
                    var extractedText = ocrService.PerformOcr(pdfStream);

                    _logger.LogInformation(
                        "OCR erfolgreich für DocumentId {Id} → {Chars} Zeichen erkannt",
                        message.DocumentId, extractedText.Length);

                    if (extractedText.Length > 200)
                        _logger.LogInformation("Vorschau: {Preview}...", extractedText.Substring(0, 200));
                    else
                        _logger.LogInformation("Text: {Text}", extractedText);

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OCR Worker Fehler bei Nachricht");
                    channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}