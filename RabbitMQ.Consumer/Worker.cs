using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Consumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConnection _connection;
        
        public Worker(ILogger<Worker> logger, ConnectionFactory factory)
        {
            _logger = logger;
            _connection = factory.CreateConnection();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(5 * 1000, stoppingToken);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var channel = _connection.CreateModel();
            channel.QueueDeclare("queue-person-service",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += new EventHandler<BasicDeliverEventArgs>(ConsumerOnReceived);

            return Task.CompletedTask;
        }

        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(message);
        }
    }
}
