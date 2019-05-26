using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Account.Events
{

    /// <summary>
    /// The person who owns the account has been set 
    /// </summary>
    /// <remarks>
    /// This is used for the anti-money laundering functions and also to link 
    /// a contact with the bank account
    /// </remarks>
    [EventName("Designated Benificiary Set")]
    public class BeneficiarySet
    {

        /// <summary>
        /// The name of the person or entity that is the beneficiary of this account
        /// </summary>
        public string BeneficiaryName { get; set; }

        /// <summary>
        /// The country of residence of the beneficiary
        /// </summary>
        public string CountryOfResidence { get; set; }

        /// <summary>
        /// The unique identifier of the beneficiary if they are an existing customer of the bank
        /// </summary>
        public string ExistingCustomerIdentifier { get; set; }
    }
}
