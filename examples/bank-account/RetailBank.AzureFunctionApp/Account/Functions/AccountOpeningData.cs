using System;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// The data payload that can be sent when opening a bank account
    /// </summary>
    internal class AccountOpeningData
    {

        /// <summary>
        /// The text commentary provided when opening the new bank account
        /// </summary>
        public string Commentary { get; set; }

        /// <summary>
        /// The initial balance used to open the account
        /// </summary>
        public Nullable<decimal> OpeningBalance { get; set; }

        /// <summary>
        /// The client in whose name the account is opened
        /// </summary>
        public string ClientName { get; set; }

    }
}