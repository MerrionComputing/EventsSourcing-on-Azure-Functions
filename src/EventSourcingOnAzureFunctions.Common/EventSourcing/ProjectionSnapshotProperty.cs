using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    public class ProjectionSnapshotProperty
    {

        public const int NO_ROW_NUMBER = 0;

        /// <summary>
        /// The unique name of the property (akin to a column header)
        /// </summary>
        public string Name {get;}

        /// <summary>
        /// The row this property is in 
        /// </summary>
        /// <remarks>
        /// If this is NO_ROW_NUMBER the projection may be one that does not have rows
        /// </remarks>
        public int RowNumber { get; }

        /// <summary>
        /// The value of this projection property
        /// </summary>
        public object ValueAsObject { get; }

        protected internal ProjectionSnapshotProperty(string name,
            int rowNumber = NO_ROW_NUMBER ,
            object value = null)
        {
            Name = name;
            RowNumber = rowNumber;
            if (null != value )
            {
                ValueAsObject = value;
            }
        }


        /// <summary>
        /// Create a type-safe projection snapshot property
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        public static ProjectionSnapshotProperty<TValue> Create<TValue>(string name, TValue value, int rowNumber = NO_ROW_NUMBER )
        {
            return new ProjectionSnapshotProperty<TValue>(name, rowNumber, value);
        }
    }

    public class ProjectionSnapshotProperty<TValue>
        : ProjectionSnapshotProperty 
    {

        public TValue Value
        {
            get
            {
                if (null != ValueAsObject )
                {
                    return (TValue)ValueAsObject;
                }
                else
                {
                    return default;
                }
            }
        }

        protected internal ProjectionSnapshotProperty(string name,
            int rowNumber,
            TValue value )
            : base(null,rowNumber,value )
        {
            
        }
    }
}
