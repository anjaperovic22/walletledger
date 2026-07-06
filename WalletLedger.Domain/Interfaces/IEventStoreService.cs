using System;
using System.Collections.Generic;
using System.Text;
using WalletLedger.Domain.Entities;
using System.Threading.Tasks;

namespace WalletLedger.Domain.Interfaces
{
    public interface IEventStoreService
    {
        Task SaveEventAsync(string aggregateId, string aggregateType, string eventType, object eventData, int version);
        Task<List<StoredEvent>> GetEventsAsync(string aggregateId);
    }
}
