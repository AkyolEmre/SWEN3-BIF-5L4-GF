using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DMS.Common.Exceptions;
using System;

namespace DMS.API.Services
{
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
                await Task.Run(() =>
                {
                    using var connection = _factory.CreateConnection();
                    using var channel = connection.CreateModel();

                    // WICHTIG: Hier nutzen wir "ocr_queue" (mit Unterstrich)
                    var queueName = _cfg["RabbitMQ:QueueName"] ?? "ocr_queue";

                    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

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