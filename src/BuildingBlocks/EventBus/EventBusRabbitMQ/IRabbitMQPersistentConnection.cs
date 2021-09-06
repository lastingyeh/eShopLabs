using System;
using RabbitMQ.Client;

namespace eShopLabs.BuildingBlocks.EventBusRabbitMQ
{
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        bool IsConnected { get; }
        bool TryConnect();
        IModel CreateModel();
    }
}
