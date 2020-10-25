using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// Settings in the [EventStreamSettings] section of the JSON settings file(s)
    /// </summary>
    public sealed  class EventMaps
        : IEventMaps
    {

        private ConcurrentDictionary<string, EventMap> AllMaps = new ConcurrentDictionary<string, EventMap>();



        /// <summary>
        /// Load the event maps as stored in any configuration files
        /// </summary>
        public void LoadFromConfig(string basePath = null)
        {

            if (string.IsNullOrWhiteSpace(basePath ) )
            {
                basePath = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot")  // local_root
                    ?? (Environment.GetEnvironmentVariable("HOME") == null
                        ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                        : $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot"); // azure_root
            }

            ConfigurationBuilder builder = new ConfigurationBuilder();
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                builder.SetBasePath(basePath);
            }

            builder.AddJsonFile("appsettings.json", true)
                .AddJsonFile("config.local.json", true)
                .AddJsonFile("config.json", true)
                .AddJsonFile("connectionstrings.json", true)
                .AddEnvironmentVariables();

            IConfigurationRoot config = builder.Build();

            // Get the [EventMaps] section
            IConfigurationSection eventMapSection = config.GetSection("EventMaps"); 
            if (null != eventMapSection )
            {
                if (eventMapSection.Exists()  )
                {
                    var configEventMaps = eventMapSection.Get<List<EventMap>>(c => c.BindNonPublicProperties = true);
                    
                    if (null != configEventMaps )
                    {
                        foreach (EventMap map in configEventMaps)
                        {
                            if (null != map)
                            {
                                if (!AllMaps.ContainsKey(map.EventName))
                                {
                                    Type eventType = null;
                                    if (TryFindType(map.EventImplementationClassName, out eventType))
                                    {
                                        AllMaps.TryAdd(map.EventName,
                                            new EventMap(map.EventName, eventType));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load the event maps by reading the attributes of any IEvent based classes in the existing
        /// application
        /// </summary>
        public void LoadByReflection()
        {
            foreach (Assembly loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // ignore system assemblies and known assemblies
                if (loadedAssembly.FullName.Contains ("Newtonsoft.Json")   )
                {
                    continue;
                }
                if (loadedAssembly.FullName.Contains("Microsoft.Azure."))
                {
                    continue;
                }
                if (loadedAssembly.FullName.Contains("Microsoft.Extensions."))
                {
                    continue;
                }
                if (loadedAssembly.IsDynamic  )
                {
                    // Dynamic assemblies cannot be scanned for exported types
                    continue;
                }
                foreach (Type eventType in loadedAssembly.GetExportedTypes())
                {
                    // does it have an EventName() attribute
                    foreach (EventNameAttribute item in eventType.GetCustomAttributes(typeof(EventNameAttribute), true))
                    {
                        if (!AllMaps.ContainsKey(item.Name))
                        {
                            AllMaps.TryAdd(item.Name, new EventMap(item.Name, eventType));
                        }
                    }
                }
            }
        }

        public  IEvent CreateEventClass(string eventName)
        {
            if (AllMaps.ContainsKey(eventName ) )
            {
                return AllMaps[eventName].CreateEventClass();
            }
            else
            {
                // maybe the event name already is the .NET class (bad practice)
                var type = Type.GetType(eventName, false);
                if (null == type )
                {
                    if (! TryFindType(eventName , out type) )
                    {
                        // Unable to create an event class for the name
                        return null;
                    }
                }
                if (null != type)
                {
                    return (IEvent)Activator.CreateInstance(type);
                }
            }
            // Unable to create an event class for the name
            return null;
        }


        private static EventMaps _defaultEventMaps;
        /// <summary>
        /// Create an event map that is the default for this application
        /// </summary>
        /// <remarks>
        /// This uses both the configuration and reflection to build the maps - if you need faster
        /// spin-up you should create a hard-coded map and use dependency injection to load it
        /// </remarks>
        public static IEventMaps CreateDefaultEventMaps()
        {
            if (null == _defaultEventMaps )
            {
                _defaultEventMaps = new EventMaps();
                _defaultEventMaps.LoadFromConfig();
                _defaultEventMaps.LoadByReflection(); 
            }
            return _defaultEventMaps;
        }


        private static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
        public static bool TryFindType(string typeName, out Type t)
        {
            lock (typeCache)
            {
                if (!typeCache.TryGetValue(typeName, out t))
                {
                    foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        t = a.GetType(typeName);
                        if (t != null)
                            break;
                    }
                    typeCache[typeName] = t; // perhaps null
                }
            }
            return t != null;
        }

    }

    /// <summary>
    /// A single mapping between an event name and the CLR class used to encapsulate it
    /// </summary>
    public sealed class EventMap
    {

        /// <summary>
        /// The unique event name as it is known to the application
        /// </summary>
       public string EventName { get; internal set; }

        /// <summary>
        /// The name of the CLR class that implements that event
        /// </summary>
        public string EventImplementationClassName { get; internal set; }

        public IEvent CreateEventClass()
        {
            if (! string.IsNullOrWhiteSpace(EventImplementationClassName ) )
            {
                Type typeRet;
                if (EventMaps.TryFindType(EventImplementationClassName, out typeRet))
                {
                    return EventInstance.Wrap( Activator.CreateInstance(typeRet));
                }
            }

            // Could not create any instance
            return null;
        }

        public EventMap(string eventName, Type eventImplementationType )
        {
            EventName = eventName;
            EventImplementationClassName = eventImplementationType.FullName; 
        }

        public EventMap()
        {
        }
    }

    /// <summary>
    /// Class for binding to the [EventMaps] 
    /// </summary>
    public sealed class EventMapsConfigurationSection
    {
        public ICollection<EventMap> EventMaps { get; set; }
    }
}
