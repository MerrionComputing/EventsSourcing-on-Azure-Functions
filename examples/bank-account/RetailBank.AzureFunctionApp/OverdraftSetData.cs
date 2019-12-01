using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// The data payload for a function to set the overdraft for an account
    /// </summary>
    public class OverdraftSetData
    {
        /// <summary>
        /// The new overdraft limitthe account
        /// </summary>
        public decimal NewOverdraftLimit { get; set; }

        /// <summary>
        /// The commentary attached to the overdraft limit 
        /// </summary>
        public string Commentary { get; set; }


    }
}
