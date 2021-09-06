using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace RabbitMQ.ApiProducer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private ILogger<PersonController> _logger;
        private readonly IConnection _connection;

        public PersonController(ILogger<PersonController> logger)
        {
            _logger = logger;
            
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };

            _connection = factory.CreateConnection();

        }

        [HttpPost]
        public void Post(string firstName, string lastName)
        {
            ProduceMessage(_connection, firstName, lastName);
        }
        
        void ProduceMessage(IConnection connection, string firstName, string lastName)
        {
            using var channel = connection.CreateModel();
            channel.QueueDeclare("queue-person-service",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var message = new { FirstName = firstName, LastName = lastName };
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

 

            channel.BasicPublish("", "queue-person-service", null, body);        }

    }
}