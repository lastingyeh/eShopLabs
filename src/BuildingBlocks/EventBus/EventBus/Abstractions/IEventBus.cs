using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;

namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions
{
    public interface IEventBus
    {
        void Publish(IntegrationEvent @event);
        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
        void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
        void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;
        void UnsubscribeDyanmic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;
    }
}
