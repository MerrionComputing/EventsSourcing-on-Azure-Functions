using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// Attribute to allow a projection class to be tagged with the projection business name
    /// </summary>
    /// <remarks>
    /// This allows the projection name to be independent of the language used to read/write it for shared event streams
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProjectionNameAttribute
       : Attribute
    {

        /// <summary>
        /// The business meaningful name of the projection held in this class
        /// </summary>
        public string Name { get; private set; }


        public ProjectionNameAttribute(string eventName)
        {
            Name = eventName;
        }


        public static string GetProjectionName(Type projectionType)
        {
            foreach (ProjectionNameAttribute item in projectionType.GetCustomAttributes(typeof(ProjectionNameAttribute), true))
            {
                return item.Name;
            }

            // fall back on type full name
            return projectionType.FullName;
        }



    }
}
