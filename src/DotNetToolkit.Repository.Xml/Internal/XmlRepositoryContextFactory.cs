﻿namespace DotNetToolkit.Repository.Xml.Internal
{
    using Configuration;
    using Factories;
    using System;

    /// <summary>
    /// An implementation of <see cref="IRepositoryContextFactory" />.
    /// </summary>
    /// <seealso cref="IRepositoryContextFactory" />
    internal class XmlRepositoryContextFactory : IRepositoryContextFactory
    {
        #region Fields

        private readonly string _path;
        private readonly bool _ignoreTransactionWarning;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRepositoryContextFactory" /> class.
        /// </summary>
        /// <param name="path">The database directory to create.</param>
        public XmlRepositoryContextFactory(string path) : this(path, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRepositoryContextFactory" /> class.
        /// </summary>
        /// <param name="path">The database directory to create.</param>
        /// <param name="ignoreTransactionWarning">If a transaction operation is requested, ignore any warnings since the context provider does not support transactions.</param>
        public XmlRepositoryContextFactory(string path, bool ignoreTransactionWarning)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            _path = path;
            _ignoreTransactionWarning = ignoreTransactionWarning;
        }

        #endregion

        #region Implementation of IRepositoryContextFactory

        /// <summary>
        /// Create a new repository context.
        /// </summary>
        /// <returns>The new repository context.</returns>
        public IRepositoryContext Create()
        {
            return new XmlRepositoryContext(_path, _ignoreTransactionWarning);
        }

        #endregion
    }
}
