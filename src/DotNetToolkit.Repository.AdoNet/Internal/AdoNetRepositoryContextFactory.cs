﻿namespace DotNetToolkit.Repository.AdoNet.Internal
{
    using Configuration;
    using Factories;
    using System;
    using System.Data;
    using System.Data.Common;

    /// <summary>
    /// An implementation of <see cref="IRepositoryContextFactory" />.
    /// </summary>
    /// <seealso cref="IRepositoryContextFactory" />
    internal class AdoNetRepositoryContextFactory : IRepositoryContextFactory
    {
        #region Fields

        private readonly string _nameOrConnectionString;
        private readonly string _providerName;
        private readonly DbConnection _existingConnection;
        private readonly bool _ensureDatabaseCreated;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoNetRepositoryContextFactory"/> class.
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        /// <param name="ensureDatabaseCreated">
        /// Ensures that the database for the context exists. If it exists, no action is taken.
        /// If it does not exist then the database and all its schema are created.
        /// If the database exists, then no effort is made to ensure it is compatible with the model for this context.
        /// </param>
        public AdoNetRepositoryContextFactory(string nameOrConnectionString, bool ensureDatabaseCreated = false)
        {
            if (nameOrConnectionString == null)
                throw new ArgumentNullException(nameof(nameOrConnectionString));

            _nameOrConnectionString = nameOrConnectionString;
            _ensureDatabaseCreated = ensureDatabaseCreated;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoNetRepositoryContextFactory"/> class.
        /// </summary>
        /// <param name="providerName">The name of the provider.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="ensureDatabaseCreated">
        /// Ensures that the database for the context exists. If it exists, no action is taken.
        /// If it does not exist then the database and all its schema are created.
        /// If the database exists, then no effort is made to ensure it is compatible with the model for this context.
        /// </param>
        public AdoNetRepositoryContextFactory(string providerName, string connectionString, bool ensureDatabaseCreated = false)
        {
            if (providerName == null)
                throw new ArgumentNullException(nameof(providerName));

            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));

            _providerName = providerName;
            _nameOrConnectionString = connectionString;
            _ensureDatabaseCreated = ensureDatabaseCreated;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoNetRepositoryContextFactory" /> class.
        /// </summary>
        /// <param name="existingConnection">The existing connection.</param>
        /// <param name="ensureDatabaseCreated">
        /// Ensures that the database for the context exists. If it exists, no action is taken.
        /// If it does not exist then the database and all its schema are created.
        /// If the database exists, then no effort is made to ensure it is compatible with the model for this context.
        /// </param>
        public AdoNetRepositoryContextFactory(DbConnection existingConnection, bool ensureDatabaseCreated = false)
        {
            if (existingConnection == null)
                throw new ArgumentNullException(nameof(existingConnection));

            if (existingConnection.State == ConnectionState.Closed)
                existingConnection.Open();

            _existingConnection = existingConnection;
            _ensureDatabaseCreated = ensureDatabaseCreated;
        }

        #endregion

        #region Implementation of IRepositoryContextFactory

        /// <summary>
        /// Create a new repository context.
        /// </summary>
        /// <returns>The new repository context.</returns>
        public IRepositoryContext Create()
        {
            if (_existingConnection != null)
                return new AdoNetRepositoryContext(_existingConnection, _ensureDatabaseCreated);

            if (!string.IsNullOrEmpty(_providerName))
                return new AdoNetRepositoryContext(_providerName, _nameOrConnectionString, _ensureDatabaseCreated);

            return new AdoNetRepositoryContext(_nameOrConnectionString, _ensureDatabaseCreated);
        }

        #endregion
    }
}
