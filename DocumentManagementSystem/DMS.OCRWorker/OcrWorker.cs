using Microsoft.Extensions.DependencyInjection;  // If needed for DI
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DMS.OCRWorker
{
    public sealed class OcrWorker : BackgroundService
    {
        private readonly IConnectionFactory _factory;
        private readonly ILogger<OcrWorker> _logger;

        public OcrWorker(IConnectionFactory factory, ILogger<OcrWorker> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OCR Worker starting...");

            // Use Task.Run for sync CreateConnection in async context
            var connectionTask = Task.Run(() => _factory.CreateConnection(), stoppingToken);
            var connection = await connectionTask;
            using var conn = connection;
            using var channel = conn.CreateModel();

            const string queueName = "ocr_queue";
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var docId = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogInformation("OCR started for document {DocumentId}", docId);

                    if (string.IsNullOrWhiteSpace(docId))
                        throw new ArgumentException("Invalid document id");

                    await Task.Delay(500, stoppingToken); // Simulate OCR work

                    _logger.LogInformation("OCR finished for document {DocumentId}", docId);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OCR failed for document {DocumentId}", Encoding.UTF8.GetString(ea.Body.ToArray()));
                    channel.BasicNack(ea.DeliveryTag, false, true); // Requeue
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}