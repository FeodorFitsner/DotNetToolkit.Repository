﻿namespace DotNetToolkit.Repository.Internal.ConfigFile
{
    using Configuration.Caching;
    using Configuration.Interceptors;
    using Configuration.Logging;
    using Configuration.Mapper;
    using Extensions;
    using Factories;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ConfigurationHandler
    {
        #region Fields

        private readonly IConfigurationSection _root;

        private const string RepositorySectionKey = "repository";
        private const string DefaultContextFactorySectionKey = "defaultContextFactory";
        private const string LoggingProviderSectionKey = "loggingProvider";
        private const string CachingProviderSectionKey = "cachingProvider";
        private const string MappingProviderSectionKey = "mappingProvider";
        private const string ExpiryKey = "expiry";
        private const string InterceptorCollectionSectionKey = "interceptors";
        private const string ParameterCollectionSectionKey = "parameters";
        private const string ValueKey = "value";
        private const string TypeKey = "type";

        #endregion

        #region Constructors

        public ConfigurationHandler(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _root = config.GetSection(RepositorySectionKey);

            if (_root == null)
                throw new InvalidOperationException("Unable to find a configuration for the repositories.");
        }

        #endregion

        #region Public Methods

        public IRepositoryContextFactory GetDefaultContextFactory()
        {
            var section = _root.GetSection(DefaultContextFactorySectionKey);

            if (section != null)
            {
                return GetTypedValue<IRepositoryContextFactory>(section);
            }

            return null;
        }

        public ILoggerProvider GetLoggerProvider()
        {
            var section = _root.GetSection(LoggingProviderSectionKey);

            if (section != null)
            {
                return GetTypedValue<ILoggerProvider>(section);
            }

            return null;
        }

        public ICacheProvider GetCachingProvider()
        {
            var section = _root.GetSection(CachingProviderSectionKey);

            if (section != null)
            {
                var value = GetTypedValue<ICacheProvider>(section);
                var expiry = ExtractExpiry(section);

                if (expiry != null)
                    value.CacheExpiration = expiry;

                return value;
            }

            return null;
        }

        public IMapperProvider GetMappingProvider()
        {
            var section = _root.GetSection(MappingProviderSectionKey);

            if (section != null)
            {
                return GetTypedValue<IMapperProvider>(section);
            }

            return null;
        }

        public Dictionary<Type, Func<IRepositoryInterceptor>> GetInterceptors()
        {
            var interceptorsDict = new Dictionary<Type, Func<IRepositoryInterceptor>>();
            var section = _root.GetSection(InterceptorCollectionSectionKey);

            if (section != null)
            {
                foreach (var subSection in section.GetChildren())
                {
                    if (subSection != null)
                    {
                        var type = ExtractType(subSection, isRequired: true);

                        interceptorsDict.Add(type, () => GetTypedValue<IRepositoryInterceptor>(section, type));
                    }
                }
            }

            return interceptorsDict;
        }

        #endregion

        #region Private Methods

        private static TimeSpan? ExtractExpiry(IConfigurationSection section)
        {
            var value = Extract(section, ExpiryKey, isRequired: false);

            if (string.IsNullOrEmpty(value))
                return null;

            return TimeSpan.Parse(value);
        }

        private static object[] ExtractParameters(IConfigurationSection section)
        {
            var parameterCollectionSection = section.GetSection(ParameterCollectionSectionKey);

            var args = new List<object>();

            if (parameterCollectionSection != null)
            {
                args.AddRange(parameterCollectionSection.GetChildren().Select(ExtractParameter));
            }

            return args.ToArray();
        }

        private static Type ExtractType(IConfigurationSection section, bool isRequired)
        {
            var value = Extract(section, TypeKey, isRequired);

            if (string.IsNullOrEmpty(value))
                value = "System.String";

            return Type.GetType(value, throwOnError: true);
        }

        private static object ExtractParameter(IConfigurationSection section)
        {
            var type = ExtractType(section, isRequired: false);
            var value = Extract(section, ValueKey);

            return type.ConvertTo(value);
        }

        private static string Extract(IConfigurationSection section, string key, bool isRequired = true)
        {
            if (section[key] == null && isRequired)
                throw new InvalidOperationException($"The '{key}' key is missing for the '{section.Path}' section.");

            return section[key];
        }

        private T GetTypedValue<T>(IConfigurationSection section, Type type = null)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section));

            if (type == null)
                type = ExtractType(section, isRequired: true);

            var args = ExtractParameters(section);

            var defaultFactory = ConfigurationProvider.GetDefaultFactory();

            if (defaultFactory != null)
                return (T)defaultFactory(type);

            if (args.Any())
                return (T)Activator.CreateInstance(type, args);

            return (T)Activator.CreateInstance(type);
        }

        #endregion
    }
}
