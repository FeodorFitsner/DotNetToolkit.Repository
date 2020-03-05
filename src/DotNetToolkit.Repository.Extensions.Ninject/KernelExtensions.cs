﻿namespace DotNetToolkit.Repository.Extensions.Ninject
{
    using Configuration.Interceptors;
    using Configuration.Options;
    using Configuration.Options.Internal;
    using global::Ninject;
    using JetBrains.Annotations;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Transactions;
    using Utility;

    /// <summary>
    /// Contains various extension methods for <see cref="IKernel" />
    /// </summary>
    public static class KernelExtensions
    {
        /// <summary>
        /// Binds all the repositories services using the specified options builder.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        /// <param name="optionsAction">A builder action used to create or modify options for the repositories.</param>
        /// <remarks>
        /// This method will scan for repositories and interceptors from the assemblies that have been loaded into the
        /// execution context of this application domain, and will bind them to the container.
        /// </remarks>
        public static void RegisterRepositories([NotNull] this IKernel kernel, [NotNull] Action<RepositoryOptionsBuilder> optionsAction)
        {
            BindRepositories(kernel, optionsAction, AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Binds all the repositories services using the specified options builder.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        /// <param name="optionsAction">A builder action used to create or modify options for the repositories.</param>
        /// <param name="assembliesToScan">The assemblies to scan.</param>
        /// <remarks>
        /// This method will scan for repositories and interceptors from the specified assemblies collection, and will bind them to the container.
        /// </remarks>
        public static void BindRepositories([NotNull] this IKernel kernel, [NotNull] Action<RepositoryOptionsBuilder> optionsAction, [NotNull] params Assembly[] assembliesToScan)
        {
            Guard.NotNull(kernel, nameof(kernel));
            Guard.NotNull(optionsAction, nameof(optionsAction));
            Guard.NotEmpty(assembliesToScan, nameof(assembliesToScan));

            var optionsBuilder = new RepositoryOptionsBuilder();

            optionsAction(optionsBuilder);

            var registeredInterceptorTypes = new List<Type>();

            // Scan assemblies for repositories, services, and interceptors
            AssemblyScanner
                .FindRepositoriesFromAssemblies(assembliesToScan)
                .ForEach(scanResult =>
                {
                    foreach (var implementationType in scanResult.ImplementationTypes)
                    {
                        if (scanResult.InterfaceType == typeof(IRepositoryInterceptor))
                        {
                            kernel.Bind(implementationType).ToSelf();
                            kernel.Bind(scanResult.InterfaceType).To(implementationType);
                            registeredInterceptorTypes.Add(implementationType);
                        }
                        else
                        {
                            kernel.Bind(scanResult.InterfaceType).To(implementationType);
                        }
                    }
                });

            // Binds other services
            kernel.Bind<IRepositoryFactory>().ToMethod(c => new RepositoryFactory(c.Kernel.Get<IRepositoryOptions>()));
            kernel.Bind<IUnitOfWork>().ToMethod(c => new UnitOfWork(c.Kernel.Get<IRepositoryOptions>()));
            kernel.Bind<IUnitOfWorkFactory>().ToMethod(c => new UnitOfWorkFactory(c.Kernel.Get<IRepositoryOptions>()));
            kernel.Bind<IServiceFactory>().ToMethod(c => new ServiceFactory(c.Kernel.Get<IUnitOfWorkFactory>()));
            kernel.Bind<IRepositoryOptions>().ToMethod(c =>
            {
                var options = new RepositoryOptions(optionsBuilder.Options);

                foreach (var interceptorType in registeredInterceptorTypes)
                {
                    if (!optionsBuilder.Options.Interceptors.ContainsKey(interceptorType))
                    {
                        options = options.With(interceptorType, () => (IRepositoryInterceptor)c.Kernel.Get(interceptorType));
                    }
                }

                return options;
            });

            // Binds resolver
            RepositoryDependencyResolver.SetResolver(type => kernel.Get(type));

            kernel.Bind<IRepositoryDependencyResolver>().ToMethod(c => RepositoryDependencyResolver.Current);
        }
    }
}
