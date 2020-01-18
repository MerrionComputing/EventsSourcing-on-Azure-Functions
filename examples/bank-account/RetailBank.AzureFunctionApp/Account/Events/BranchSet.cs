using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;

namespace RetailBank.AzureFunctionApp.Account.Events
{
    /// <summary>
    /// The branch that an account belongs to / is managed by has been set
    /// </summary>
    [EventName("Branch Set")]
    public class BranchSet
    {

        /// <summary>
        /// The unique identifier of the branch that has been set as the "owner"
        /// of this account
        /// </summary>
        public string BranchIdentifier { get; set; }

        /// <summary>
        /// Additional commentary on the change of branch owner
        /// </summary>
        public string Commentary { get; set; }


        /// <summary>
        /// As of when was this branch onwership effective
        /// </summary>
        public DateTime EffectiveDate { get; set; }
    }
}
