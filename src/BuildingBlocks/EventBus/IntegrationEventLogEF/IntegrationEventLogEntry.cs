using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using eShopLabs.BuildingBlocks.EventBus.Events;
using Newtonsoft.Json;

namespace eShopLabs.BuildingBlocks.IntegrationEventLogEF
{
    public class IntegrationEventLogEntry
    {
        public Guid EventId { get; private set; }
        public DateTime CreationTime { get; private set; }
        public string EventTypeName { get; private set; }
        public string Content { get; private set; }
        public EventStateEnum State { get; set; }
        public int TimesSent { get; set; }
        public string TransactionId { get; private set; }
        [NotMapped]
        public string EventTypeShortName => EventTypeName.Split('.')?.Last();
        [NotMapped]
        public IntegrationEvent IntegrationEvent { get; private set; }

        private IntegrationEventLogEntry() { }
        public IntegrationEventLogEntry(IntegrationEvent @event, Guid transactionId)
        {
            EventId = @event.Id;
            CreationTime = @event.CreationDate;
            EventTypeName = @event.GetType().FullName;
            Content = JsonConvert.SerializeObject(@event);
            State = EventStateEnum.NotPublished;
            TimesSent = 0;
            TransactionId = transactionId.ToString();
        }

        public IntegrationEventLogEntry DeserializeJsonContent(Type type)
        {
            IntegrationEvent = JsonConvert.DeserializeObject(Content, type) as IntegrationEvent;

            return this;
        }
    }
}
