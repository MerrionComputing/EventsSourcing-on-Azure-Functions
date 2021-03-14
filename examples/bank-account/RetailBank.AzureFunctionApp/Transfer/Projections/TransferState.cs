using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using RetailBank.AzureFunctionApp.Transfer.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;

namespace RetailBank.AzureFunctionApp.Transfer.Projections
{
    /// <summary>
    /// A projection to return the state of a given money transfer
    /// </summary>
    [ProjectionName("Money Transfer State")]
    public sealed class TransferState
        : ProjectionBase,
        IHandleEventType<TransferInitiated>,
        IHandleEventType<SourceFundsWithdrawn >,
        IHandleEventType<SourceFundsRefunded >, 
        IHandleEventType<TargetFundsDeposited >,
        IHandleEventType<RefundInitiated >
    {

        private string lastStateChange;
        /// <summary>
        /// The last state event that happened in this transfers life cycle
        /// </summary>
        public string LastStateChange
        {
            get
            {
                return lastStateChange;
            }
        }

        private decimal amountOfTransferRequest;
        /// <summary>
        /// How much money transfer was initially requested
        /// </summary>
        public decimal AmountOfTransferRequested
        {
            get
            {
                return amountOfTransferRequest;
            }
        }

        private decimal amountWithdrawn;
        /// <summary>
        /// The amount that has been withdrawn from the source account
        /// </summary>
        public decimal AmountWithdrawn
        {
            get
            {
                return amountWithdrawn;
            }
        }

        private decimal amountToRefund;
        public decimal AmountToRefund
        {
            get
            {
                return amountToRefund;
            }
        }


        private decimal amountDeposited;
        /// <summary>
        /// The amound that has been deposited into the target account of the money transfer
        /// </summary>
        public decimal AmountDeposited
        {
            get
            {
                return amountDeposited;
            }
        }

        public void HandleEventInstance(TransferInitiated eventInstance)
        {
            if (eventInstance != null)
            {
                lastStateChange = "Initiated";
                amountOfTransferRequest = eventInstance.Amount;
            }
        }

        public void HandleEventInstance(SourceFundsWithdrawn eventInstance)
        {
            if (eventInstance != null)
            {
                lastStateChange = "Source Funds Withdrawn";
                amountWithdrawn += eventInstance.AmountWithdrawn;
            }
        }

        public void HandleEventInstance(SourceFundsRefunded eventInstance)
        {
            if (eventInstance != null)
            {
                lastStateChange = "Source Funds Refunded";
                amountWithdrawn -= eventInstance.AmountRefunded;
            }
        }

        public void HandleEventInstance(TargetFundsDeposited eventInstance)
        {
            if (eventInstance != null)
            {
                lastStateChange = "Target Funds Deposited";
                amountDeposited  += eventInstance.AmountDeposited;
            }
        }

        public void HandleEventInstance(RefundInitiated eventInstance)
        {
            if (eventInstance != null)
            {
                lastStateChange = "Refund Initiated";
                amountToRefund += eventInstance.AmountToRefund;
            }
        }
    }
}
