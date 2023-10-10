using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Infrastucture;
using CQRS.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Stores
{
    public class EventStore : IEventStore
    {
        private readonly IEventStoreRepository _eventStoreRepository;
        public EventStore(IEventStoreRepository eventStoreRepository)
        {
            _eventStoreRepository = eventStoreRepository;
        }
        public async Task<List<BaseEvent>> GetEventAsync(Guid aggreagateId)
        {
            var eventStream = await _eventStoreRepository.FindByAggregateId(aggreagateId);
            if (eventStream == null || !eventStream.Any()) throw new AggregateNotFoundException("Icorrect post ID");

            return eventStream.OrderBy(x => x.Version).Select(x => x.EventData).ToList();
        }

        public async Task SaveEventAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion)
        {
            var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);
            if (expectedVersion != -1 && eventStream[^1].Version != expectedVersion)
                throw new ConcurrencyException();

            var version = expectedVersion;
            foreach (var @event in events)
            {
                version++;
                @event.Version = version;
                var eventType = @event.GetType().Name;
                var eventModel = new EventModel
                {
                    TimeStamp = DateTime.Now,
                    AggregateIdentifier = aggregateId,
                    AggregateType = nameof(PostAggregate),
                    Version = version,
                    EventData = @event,
                    EventType = eventType
                };

                await _eventStoreRepository.SaveAsync(eventModel);
            }
        }
    }
}
