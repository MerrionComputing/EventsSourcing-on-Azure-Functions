using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Projections
{
    /// <summary>
    /// The projection to get the list of all projection requests outstanding for a
    /// given command or query
    /// </summary>
    [ProjectionName("Outstanding Projections") ]
    public sealed class OutstandingProjections
        : ProjectionBase,
        IHandleEventType<ProjectionRequested>,
        IHandleEventType<ProjectionValueReturned > 
    {

        private List<IProjectionRequest> _requestedProjections = new List<IProjectionRequest>();

        private static ProjectionComparer _comparer = new ProjectionComparer();

        /// <summary>
        /// The set of projections that still need to be processed
        /// </summary>
        public IEnumerable<IProjectionRequest > ProjectionsToBeProcessed
        {
            get
            {
                return _requestedProjections; 
            }
        }

        public void HandleEventInstance(ProjectionRequested eventInstance)
        {
            if (! _requestedProjections.Contains(eventInstance, _comparer)  )
            {
                _requestedProjections.Add(eventInstance); 
            }
        }

        public void HandleEventInstance(ProjectionValueReturned eventInstance)
        {
            if (_requestedProjections.Contains(eventInstance, _comparer))
            {
                ProjectionComparer compareTo = new ProjectionComparer(eventInstance);
                int pos = _requestedProjections.FindIndex(0,  compareTo.Equals);
                if (pos >= 0)
                {
                    _requestedProjections.RemoveAt(pos);
                }
            }
        }


        public class ProjectionComparer
            : IEqualityComparer<IProjectionRequest>
        {

            readonly IProjectionRequest _compareTo = null;

            public bool Equals(
                [AllowNull] IProjectionRequest x, 
                [AllowNull] IProjectionRequest y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return true;
                    }
                    else
                    {
                        return false ; // y is greater
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
                        if (x.ProjectionDomainName == y.ProjectionDomainName)
                        {
                            if (x.ProjectionEntityTypeName == y.ProjectionEntityTypeName)
                            {
                                if (x.ProjectionInstanceKey == y.ProjectionInstanceKey)
                                {
                                    if (x.ProjectionTypeName == y.ProjectionTypeName)
                                    {
                                        if (x.AsOfDate.HasValue)
                                        {
                                            if (y.AsOfDate.HasValue)
                                            {
                                                return x.AsOfDate.Value.Equals(y.AsOfDate.Value);
                                            }
                                            else
                                            {
                                                return false ; //x is greater
                                            }
                                        }
                                        else
                                        {
                                            if (y.AsOfDate.HasValue)
                                            {
                                                return false ; // y is greater
                                            }
                                            else
                                            {
                                                return true ; // they are the same
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
                                    return false ;
                                }
                            }
                            else
                            {
                                return false ;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }


            public bool Equals([AllowNull] IProjectionRequest y)
            {
                return Equals(_compareTo, y);
            }

            public int GetHashCode([DisallowNull] IProjectionRequest obj)
            {
                return $"{obj.ProjectionDomainName}.{obj.ProjectionEntityTypeName}.{obj.ProjectionInstanceKey}.{obj.ProjectionTypeName}.{obj.AsOfDate.GetValueOrDefault()}".GetHashCode();
            }

            public ProjectionComparer()
            {
            }

            public ProjectionComparer(IProjectionRequest compareTo)
            {
                _compareTo = compareTo;
            }

        }

    }
}
