using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp
{
    public class MoneyDepositData
    {
        /// <summary>
        /// The text commentary provided when depositing money in an account
        /// </summary>
        public string Commentary { get; set; }

        /// <summary>
        /// The amount deposited
        /// </summary>
        public decimal DepositAmount { get; set; }

        /// <summary>
        /// Where did the deposit come from
        /// </summary>
        public string Source { get; set; }

    }
}
