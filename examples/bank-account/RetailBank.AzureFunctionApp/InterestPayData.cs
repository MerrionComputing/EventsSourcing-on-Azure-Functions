using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// The payload to pass to the "pay interest" command
    /// </summary>
    internal class InterestPayData
    {

        /// <summary>
        /// The text commentary provided when paying the interest
        /// </summary>
        public string Commentary { get; set; }

    }
}
