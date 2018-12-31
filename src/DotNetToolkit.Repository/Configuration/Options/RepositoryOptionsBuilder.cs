﻿namespace DotNetToolkit.Repository.Configuration.Options
{
    using Caching;
    using Factories;
    using Interceptors;
    using Logging;
    using System;

    /// <summary>
    /// Represents a builder used to create or modify options for a repository.
    /// </summary>
    public class RepositoryOptionsBuilder
    {
        #region Fields

        private readonly RepositoryOptions _options;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether any options have been configured.
        /// </summary>
        public virtual bool IsConfigured { get { return Options.IsConfigured; } }

        /// <summary>
        /// Gets the options being configured.
        /// </summary>
        public virtual RepositoryOptions Options { get { return _options; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryOptionsBuilder"/> class.
        /// </summary>
        public RepositoryOptionsBuilder()
        {
            _options = new RepositoryOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryOptionsBuilder"/> class.
        /// </summary>
        /// <param name="options">The repository options.</param>
        public RepositoryOptionsBuilder(IRepositoryOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _options = new RepositoryOptions(options);
        }

        #endregion

        #region Public Methods

#if !NETSTANDARD1_3
        /// <summary>
        /// Configures the repository options with the data from the App.config.
        /// </summary>
        /// <returns>The same builder instance.</returns>
        public virtual RepositoryOptionsBuilder UseConfiguration()
        {
            var config = (Internal.ConfigFile.ConfigurationSection)
                System.Configuration.ConfigurationManager.GetSection(Internal.ConfigFile.ConfigurationSection.SectionName);

            var defaultContextFactory = config.DefaultContextFactory.GetTypedValue();
            if (defaultContextFactory != null)
            {
                UseInternalContextFactory(defaultContextFactory);
            }

            foreach (var item in config.Interceptors.GetTypedValues())
            {
                UseInterceptor(item.Key, item.Value);
            }

            return this;
        }
#endif

#if NETSTANDARD2_0
        /// <summary>
        /// Configures the repository options using the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The same builder instance.</returns>
        public virtual RepositoryOptionsBuilder UseConfiguration(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var config = new Internal.ConfigFile.ConfigurationHandler(configuration);

            var defaultContextFactory = config.GetDefaultContextFactory();
            if (defaultContextFactory != null)
            {
                UseInternalContextFactory(defaultContextFactory);
            }

            foreach (var item in config.GetInterceptors())
            {
                UseInterceptor(item.Key, item.Value);
            }

            return this;
        }
#endif

        /// <summary>
        /// Configures the repository options with an interceptor that intercepts any activity within the repository.
        /// </summary>
        /// <param name="underlyingType">The type of interceptor.</param>
        /// <param name="interceptorFactory">The interceptor factory.</param>
        /// <returns>The same builder instance.</returns>
        public virtual RepositoryOptionsBuilder UseInterceptor(Type underlyingType, Func<IRepositoryInterceptor> interceptorFactory)
        {
            if (underlyingType == null)
                throw new ArgumentNullException(nameof(underlyingType));

            if (interceptorFactory == null)
                throw new ArgumentNullException(nameof(interceptorFactory));

            _options.With(underlyingType, interceptorFactory);

            return this;
        }

        /// <summary>
        /// Configures the repository options with an interceptor that intercepts any activity within the repository.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
        /// <param name="interceptorFactory">The interceptor factory.</param>
        /// <returns>The same builder instance.</returns>
        public RepositoryOptionsBuilder UseInterceptor<TInterceptor>(Func<TInterceptor> interceptorFactory) where TInterceptor : class, IRepositoryInterceptor
        {
            return UseInterceptor(typeof(TInterceptor), interceptorFactory);
        }

        /// <summary>
        /// Configures the repository options with an interceptor that intercepts any activity within the repository.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns>The same builder instance.</returns>
        public RepositoryOptionsBuilder UseInterceptor<TInterceptor>(TInterceptor interceptor) where TInterceptor : class, IRepositoryInterceptor
        {
            return UseInterceptor<TInterceptor>(() => interceptor);
        }

        /// <summary>
        /// Configures the repository options with a logger provider for logging messages within the repository.
        /// </summary>
        /// <param name="loggerProvider">The logger provider.</param>
        /// <returns>The same builder instance.</returns>
        public RepositoryOptionsBuilder UseLoggerProvider(ILoggerProvider loggerProvider)
        {
            if (loggerProvider == null)
                throw new ArgumentNullException(nameof(loggerProvider));

            _options.With(loggerProvider);

            return this;
        }

        /// <summary>
        /// Configures the repository options with a caching provider for caching queries within the repository.
        /// </summary>
        /// <param name="cacheProvider">The caching provider.</param>
        /// <returns>The same builder instance.</returns>
        public RepositoryOptionsBuilder UseCachingProvider(ICacheProvider cacheProvider)
        {
            if (cacheProvider == null)
                throw new ArgumentNullException(nameof(cacheProvider));

            _options.With(cacheProvider);

            return this;
        }

        /// <summary>
        /// Configures the repository options with an internal context factory.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// <returns>The same builder instance.</returns>
        public virtual RepositoryOptionsBuilder UseInternalContextFactory(IRepositoryContextFactory contextFactory)
        {
            if (contextFactory == null)
                throw new ArgumentNullException(nameof(contextFactory));

            _options.With(contextFactory);

            return this;
        }

        #endregion
    }
}
