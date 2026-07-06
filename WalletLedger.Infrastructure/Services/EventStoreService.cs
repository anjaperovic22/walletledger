using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WalletLedger.Domain.Entities;
using WalletLedger.Domain.Interfaces;
using WalletLedger.Infrastructure.Data;

namespace WalletLedger.Infrastructure.Services
{
    public class EventStoreService : IEventStoreService
    {
        private readonly AppDbContext _context;

        public EventStoreService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveEventAsync(string aggregateId, string aggregateType, string eventType, object eventData, int version)
        {
            var storedEvent = new StoredEvent
            {
                AggregateId = aggregateId,
                AggregateType = aggregateType,
                EventType = eventType,
                EventData = JsonSerializer.Serialize(eventData),
                Version = version,
                Timestamp = DateTime.UtcNow
            };

            await _context.StoredEvents.AddAsync(storedEvent);
            // Namerno NE zovemo SaveChangesAsync ovde - event se čuva
            // u istoj DB transakciji kad handler pozove SaveChangesAsync na kraju
        }

        public async Task<List<StoredEvent>> GetEventsAsync(string aggregateId)
        {
            return await _context.StoredEvents
                .Where(e => e.AggregateId == aggregateId && e.AggregateType == "Wallet")
                .OrderBy(e => e.Timestamp)
                .ToListAsync();
        }
    }
}