using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    public sealed class ClassificationMaps
        : IClassificationMaps
    {

        private Dictionary<string, ClassificationMap> AllMaps = new Dictionary<string, ClassificationMap>();

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
            IConfigurationSection classificationMapSection = config.GetSection("ClassificationMaps");
            if (null != classificationMapSection)
            {
                if (classificationMapSection.Exists())
                {
                    var configClassificationMaps = classificationMapSection.Get<List<ClassificationMap>>(c => c.BindNonPublicProperties = true);

                    if (null != configClassificationMaps)
                    {
                        foreach (ClassificationMap map in configClassificationMaps)
                        {
                            if (null != map)
                            {
                                if (!AllMaps.ContainsKey(map.ClassificationName ))
                                {
                                    Type eventType = null;
                                    if (EventMaps.TryFindType(map.ClassificationImplementationClassName , out eventType))
                                    {
                                        AllMaps.Add(map.ClassificationName ,
                                            new ClassificationMap(map.ClassificationName , eventType.FullName));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        private static ClassificationMaps _defaultClassificationMaps;
        internal static IClassificationMaps CreateDefaultClassificationMaps()
        {
            if (null == _defaultClassificationMaps)
            {
                _defaultClassificationMaps = new ClassificationMaps();
                _defaultClassificationMaps.LoadFromConfig();
                _defaultClassificationMaps.LoadByReflection();
            }
            return _defaultClassificationMaps;
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
                    // does it have an ClassificationName() attribute
                    foreach (ClassificationNameAttribute item in eventType.GetCustomAttributes(typeof(ClassificationNameAttribute), true))
                    {
                        if (!AllMaps.ContainsKey(item.Name))
                        {
                            AllMaps.Add(item.Name,
                                new ClassificationMap(item.Name, eventType.FullName));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create the .NET class for a particular classification type from its name
        /// </summary>
        /// <param name="classificationName">
        /// The "business" name of the classification to map to a .NET class
        /// </param>
        public IClassification CreateClassificationClass(string classificationName)
        {

            if (string.IsNullOrWhiteSpace(classificationName))
            {
                // No idea what classification is being sought
                return null;
            }

            if (AllMaps.ContainsKey(classificationName))
            {
                return AllMaps[classificationName].CreateClassificationClass();
            }
            else
            {
                // maybe the projection name already is the .NET class (bad practice)
                var type = Type.GetType(classificationName, false);
                if (null == type)
                {
                    if (!EventMaps.TryFindType(classificationName, out type))
                    {
                        // Unable to create an event class for the name
                        return null;
                    }
                }
                if (null != type)
                {
                    return (IClassification)Activator.CreateInstance(type);
                }
            }
            // Unable to create an event class for the name
            return null;
        }
    }

    /// <summary>
    /// Mapping between a classification name and the class that implements it
    /// </summary>
    /// <remarks>
    /// Classification names can be domain qualified {domain}.{entity type}.{projection name} if
    /// needed for uniqueness
    /// </remarks>
    public sealed class ClassificationMap
    {

        /// <summary>
        /// The unique classification name as it is known to the application
        /// </summary>
        public string ClassificationName { get; internal set; }

        /// <summary>
        /// The name of the CLR class that implements that projection
        /// </summary>
        public string ClassificationImplementationClassName { get; internal set; }

        public IClassification  CreateClassificationClass()
        {
            if (!string.IsNullOrWhiteSpace(ClassificationImplementationClassName))
            {
                Type typeRet;
                if (EventMaps.TryFindType(ClassificationImplementationClassName, out typeRet))
                {
                    return (IClassification)(Activator.CreateInstance(typeRet));
                }
            }

            // Could not create any instance
            return null;
        }


        public ClassificationMap(string classificationNameIn,
           string classImplementationIn)
        {
            ClassificationName = classificationNameIn;
            ClassificationImplementationClassName = classImplementationIn;
        }


        /// <summary>
        /// Parameter-less constructor for serialisation
        /// </summary>
        public ClassificationMap()
        {
        }
    }
}
