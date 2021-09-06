using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace eShopLabs.BuildingBlocks.EventBusServiceBus
{
    public class DefaultServiceBusPersisterConnection : IServiceBusPersisterConnection
    {
        private readonly ServiceBusConnectionStringBuilder _serviceBusConnectionStringBuilder;
        private readonly ILogger<DefaultServiceBusPersisterConnection> _logger;
        private ITopicClient _topicClient;
        bool _disposed;

        public ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder =>
            _serviceBusConnectionStringBuilder;

        public DefaultServiceBusPersisterConnection(
            ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder,
            ILogger<DefaultServiceBusPersisterConnection> logger)
        {
            _serviceBusConnectionStringBuilder = serviceBusConnectionStringBuilder;
            _logger = logger;
            _topicClient = new TopicClient(_serviceBusConnectionStringBuilder, RetryPolicy.Default);
        }
        public ITopicClient CreateModel()
        {
            if (_topicClient.IsClosedOrClosing)
            {
                _topicClient = new TopicClient(_serviceBusConnectionStringBuilder, RetryPolicy.Default);
            }

            return _topicClient;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
        }
    }
}
