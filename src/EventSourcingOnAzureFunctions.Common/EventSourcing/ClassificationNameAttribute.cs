using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// Attribute to allow a classification class to be tagged with the classification name
    /// </summary>
    /// <remarks>
    /// This allows the classification name to be independent of the language used to read/write it for notifications
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ClassificationNameAttribute
        : Attribute
    {

        /// <summary>
        /// The business meaningful name of the event held in this class
        /// </summary>
        public string Name { get; private set; }


        public ClassificationNameAttribute(string eventName)
        {
            Name = eventName;
        }


        public static string GetClassificationName(Type classificationType)
        {
            foreach (ClassificationNameAttribute item in classificationType.GetCustomAttributes(typeof(ClassificationNameAttribute), true))
            {
                return item.Name;
            }

            // fall back on type name
            return classificationType.Name ;
        }

    }
}
