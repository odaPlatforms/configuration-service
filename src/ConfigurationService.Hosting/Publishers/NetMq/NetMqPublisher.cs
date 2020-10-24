using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace ConfigurationService.Hosting.Publishers.NetMq
{
    public class NetMqPublisher : IPublisher
    {
        private readonly ILogger<NetMqPublisher> _logger;

        private readonly NetMqOptions _options;
        private static PublisherSocket _publisherSocket;

        public NetMqPublisher(ILogger<NetMqPublisher> logger, NetMqOptions options)
        {
            _logger = logger;

            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Initialize()
        {
            _publisherSocket = new PublisherSocket();

            _logger.LogInformation("NetMQ publisher socket binding to {Address}...", _options.Address);
            _publisherSocket.Bind(_options.Address);

            _logger.LogInformation("NetMQ publisher initialized.");
        }

        public Task Publish(string topic, string message)
        {
            _logger.LogInformation("Publishing message to NetMQ with topic {topic}.", topic);

            _publisherSocket.SendMoreFrame(topic).SendFrame(message);

            return Task.CompletedTask;
        }
    }
}