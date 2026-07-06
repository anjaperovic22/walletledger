using Stateless;
using System;
using System.Collections.Generic;
using System.Text;
using WalletLedger.Domain.Enums;

namespace WalletLedger.Application.Common.Factories
{
    public class TransactionStateMachineFactory
    {

        public static StateMachine<TransactionStatus, TransactionTrigger> Create(TransactionStatus initialStatus)
        {
            var machine = new StateMachine<TransactionStatus, TransactionTrigger>(initialStatus);

            machine.Configure(TransactionStatus.Pending)
                .Permit(TransactionTrigger.Complete, TransactionStatus.Completed)
                .Permit(TransactionTrigger.Fail, TransactionStatus.Failed);

            machine.Configure(TransactionStatus.Completed)
                .Permit(TransactionTrigger.Reverse, TransactionStatus.Reversed);

            machine.Configure(TransactionStatus.Failed);
            // Failed je "terminalno" stanje - nema Permit iz njega, pokušaj bilo kog triggera baca exception

            return machine;
        }

    }
}
