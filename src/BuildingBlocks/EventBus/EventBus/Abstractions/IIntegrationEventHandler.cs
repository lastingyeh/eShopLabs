using System.Threading.Tasks;
using eShopLabs.BuildingBlocks.EventBus.Events;

namespace eShopLabs.BuildingBlocks.EventBus.Abstractions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }

    public interface IIntegrationEventHandler { }
}
