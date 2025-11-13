using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DMS.Common.Exceptions;
using DMS.API.Services;  // For IMessageProducer

namespace DMS.API.Services
{
    public interface IMessageProducer
    {
        Task SendMessageAsync(string message);
    }

    public sealed class MessageProducer : IMessageProducer
    {
        private readonly IConnectionFactory _factory;
        private readonly IConfiguration _cfg;

        public MessageProducer(IConnectionFactory factory, IConfiguration cfg)
        {
            _factory = factory;
            _cfg = cfg;
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                // Use Task.Run for async compatibility with sync CreateConnection
                await Task.Run(() =>
                {
                    var connection = _factory.CreateConnection();
                    using var conn = connection;
                    using var channel = conn.CreateModel();

                    var queueName = _cfg["RabbitMQ:QueueName"] ?? "ocr_queue";  // Fallback to match OcrWorker
                    channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                });
            }
            catch (Exception ex)
            {
                throw new QueueException($"RabbitMQ publish failed: {ex.Message}", ex);
            }
        }
    }
}