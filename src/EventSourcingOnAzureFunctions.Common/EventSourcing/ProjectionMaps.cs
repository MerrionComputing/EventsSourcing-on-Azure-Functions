using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{

    public sealed class ProjectionMaps
        : IProjectionMaps
    {

        private Dictionary<string, ProjectionMap> AllMaps = new Dictionary<string, ProjectionMap>();

        /// <summary>
        /// Load the projection maps as stored in any configuration files
        /// </summary>
        public void LoadFromConfig(string basePath = null)
        {

            if (string.IsNullOrWhiteSpace(basePath))
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

            // Get the [ProjectionMaps] section
            IConfigurationSection projectionMapSection = config.GetSection("ProjectionMaps");
            if (null != projectionMapSection)
            {
                if (projectionMapSection.Exists())
                {
                    var configProjectionMaps = projectionMapSection.Get<List<ProjectionMap>>(c => c.BindNonPublicProperties = true);

                    if (null != configProjectionMaps)
                    {
                        foreach (ProjectionMap map in configProjectionMaps)
                        {
                            if (null != map)
                            {
                                if (!AllMaps.ContainsKey(map.ProjectionName ))
                                {
                                    Type eventType = null;
                                    if (EventMaps.TryFindType(map.ProjectionImplementationClassName , out eventType))
                                    {
                                        AllMaps.Add(map.ProjectionName ,
                                            new ProjectionMap(map.ProjectionName, eventType.FullName ));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load the projection maps by reading the attributes of any IProjection based classes in the existing
        /// application
        /// </summary>
        public void LoadByReflection()
        {
            foreach (Assembly loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // ignore system assemblies and known assemblies
                if (loadedAssembly.FullName.Contains("Newtonsoft.Json"))
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
                if (loadedAssembly.IsDynamic)
                {
                    // Dynamic assemblies cannot be scanned for exported types
                    continue;
                }
                foreach (Type eventType in loadedAssembly.GetExportedTypes())
                {
                    // does it have an ProjectionName() attribute
                    foreach (ProjectionNameAttribute item in eventType.GetCustomAttributes(typeof(ProjectionNameAttribute), true))
                    {
                        if (!AllMaps.ContainsKey(item.Name))
                        {
                            AllMaps.Add(item.Name, 
                                new ProjectionMap(item.Name, eventType.FullName ));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create the .NET class for a particular projection type from its name
        /// </summary>
        /// <param name="projectionName">
        /// The "business" name of the projection to map to a .NET class
        /// </param>
        public IProjection CreateProjectionClass(string projectionName)
        {

            if (AllMaps.ContainsKey(projectionName))
            {
                return AllMaps[projectionName].CreateProjectionClass();
            }
            else
            {
                // maybe the projection name already is the .NET class (bad practice)
                var type = Type.GetType(projectionName, false);
                if (null == type)
                {
                    if (!EventMaps.TryFindType(projectionName, out type))
                    {
                        // Unable to create an event class for the name
                        return null;
                    }
                }
                if (null != type)
                {
                    return (IProjection)Activator.CreateInstance(type);
                }
            }
            // Unable to create an event class for the name
            return null;

        }


        private static ProjectionMaps _defaultProjectionMaps;
        /// <summary>
        /// Create a projections map that is the default for this application
        /// </summary>
        /// <remarks>
        /// This uses both the configuration and reflection to build the maps - if you need faster
        /// spin-up you should create a hard-coded map and use dependency injection to load it
        /// </remarks>
        public static IProjectionMaps CreateDefaultProjectionMaps()
        {
            if (null == _defaultProjectionMaps)
            {
                _defaultProjectionMaps = new ProjectionMaps();
                _defaultProjectionMaps.LoadFromConfig();
                _defaultProjectionMaps.LoadByReflection();
            }
            return _defaultProjectionMaps;
        }
    }

    /// <summary>
    /// Mapping between a projection name and the class that implements it
    /// </summary>
    /// <remarks>
    /// Projection names can be domain qualified {domain}.{entity type}.{projection name} if
    /// needed for uniqueness
    /// </remarks>
    public sealed class ProjectionMap
    {

        /// <summary>
        /// The unique projection name as it is known to the application
        /// </summary>
        public string ProjectionName { get; internal set; }

        /// <summary>
        /// The name of the CLR class that implements that projection
        /// </summary>
        public string ProjectionImplementationClassName { get; internal set; }

        public IProjection CreateProjectionClass()
        {
            if (!string.IsNullOrWhiteSpace(ProjectionImplementationClassName))
            {
                Type typeRet;
                if (EventMaps.TryFindType(ProjectionImplementationClassName, out typeRet))
                {
                    return (IProjection)(Activator.CreateInstance(typeRet));
                }
            }

            // Could not create any instance
            return null;
        }

        public ProjectionMap(string projectionNameIn,
            string classImplementationIn)
        {
            ProjectionName = projectionNameIn;
            ProjectionImplementationClassName = classImplementationIn;
        }

        /// <summary>
        /// Parameter-less constructor for serialisation
        /// </summary>
        public ProjectionMap()
        {
        }

    }
}
