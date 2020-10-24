using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace ConfigurationService.Client.Subscribers.RabbitMq
{
    public class NetMqSubscriber : ISubscriber
    {
        private readonly ILogger _logger;

        private readonly string _address;
        private static SubscriberSocket _subscriberSocket;

        public string Name => "NetMQ";

        public NetMqSubscriber(string address)
        {
            _logger = Logger.CreateLogger<NetMqSubscriber>();

            _address = address ?? throw new ArgumentNullException(nameof(address));
        }

        public void Initialize()
        {
            _subscriberSocket = new SubscriberSocket();

            _logger.LogInformation("NetMQ subscriber socket connecting to {_address}...", _address);
            _subscriberSocket.Connect(_address);
        }

        public void Subscribe(string topic, Action<string> handler)
        {
            _logger.LogInformation("Binding to NetMQ socket with topic '{topic}'.", topic);

            _subscriberSocket.Subscribe(topic);

            _ = Task.Run(() =>
            {
                while (true)
                {
                    var topicReceived = _subscriberSocket.ReceiveFrameString();
                    var messageReceived = _subscriberSocket.ReceiveFrameString();

                    _logger.LogInformation("Received message with topic '{topicReceived}'.", topicReceived);

                    handler(messageReceived);
                }
            });
        }
    }
}