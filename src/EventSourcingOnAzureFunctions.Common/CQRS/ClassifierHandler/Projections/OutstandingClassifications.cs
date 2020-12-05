using EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.ClassifierHandler.Projections
{
    /// <summary>
    /// The projection to get the list of all classification requests outstanding for a
    /// given command or query
    /// </summary>
    [ProjectionName("Classifications To Run") ]
    public sealed class OutstandingClassifications
        : ProjectionBase,
        IHandleEventType<ClassifierRequested>,
        IHandleEventType<ClassifierResultReturned > 
    {

        private List<IClassifierRequest > _requestedClassifications = new List<IClassifierRequest>();

        private static ClassifierComparer _comparer = new ClassifierComparer();

        /// <summary>
        /// The set of classifiers that still need to be processed
        /// </summary>
        public IEnumerable<IClassifierRequest> ClassificationsToBeProcessed
        {
            get
            {
                return _requestedClassifications ;
            }
        }

        public void HandleEventInstance(ClassifierRequested eventInstance)
        {
            if (!_requestedClassifications.Contains(eventInstance, _comparer  ) )
            {
                _requestedClassifications.Add(eventInstance);
            }
        }

        public void HandleEventInstance(ClassifierResultReturned eventInstance)
        {
            if (_requestedClassifications.Contains(eventInstance, _comparer ))
            {
                ClassifierComparer compareTo = new ClassifierComparer(eventInstance);
                int pos = _requestedClassifications.FindIndex(0, compareTo.Equals);
                if (pos >= 0)
                {
                    _requestedClassifications.RemoveAt(pos);
                }
            }
        }


    }

    public sealed class ClassifierComparer
    : IEqualityComparer<IClassifierRequest>
    {

        readonly IClassifierRequest _compareTo = null;

        public bool Equals(
            [AllowNull] IClassifierRequest x,
            [AllowNull] IClassifierRequest y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return true;
                }
                else
                {
                    return false; // y is greater
                }
            }
            else
            {
                if (y == null)
                {
                    return false; // x is greater
                }
                else
                {
                    if (x.DomainName == y.DomainName)
                    {
                        if (x.EntityTypeName == y.EntityTypeName)
                        {
                            if (x.InstanceKey == y.InstanceKey)
                            {
                                if (x.ClassifierTypeName  == y.ClassifierTypeName )
                                {
                                    if (x.AsOfDate.HasValue)
                                    {
                                        if (y.AsOfDate.HasValue)
                                        {
                                            return x.AsOfDate.Value.Equals(y.AsOfDate.Value);
                                        }
                                        else
                                        {
                                            return false; //x is greater
                                        }
                                    }
                                    else
                                    {
                                        if (y.AsOfDate.HasValue)
                                        {
                                            return false; // y is greater
                                        }
                                        else
                                        {
                                            return true; // they are the same
                                        }
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }


        public bool Equals([AllowNull] IClassifierRequest y)
        {
            return Equals(_compareTo, y);
        }

        public int GetHashCode([DisallowNull] IClassifierRequest obj)
        {
            return $"{obj.DomainName}.{obj.EntityTypeName}.{obj.InstanceKey}.{obj.ClassifierTypeName}.{obj.AsOfDate.GetValueOrDefault()}".GetHashCode();
        }

        public ClassifierComparer()
        {
        }

        public ClassifierComparer(IClassifierRequest compareTo)
        {
            _compareTo = compareTo;
        }

    }

}
