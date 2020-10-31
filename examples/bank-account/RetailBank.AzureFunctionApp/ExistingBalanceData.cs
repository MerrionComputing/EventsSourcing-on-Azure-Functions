using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// An existing balance to be passed in as a saved starting point when getting a current balance
    /// </summary>
    public sealed class ExistingBalanceData
    {

        public int AsOfSequenceNumber { get; set; }

        public decimal Balance { get; set; }

    }
}
