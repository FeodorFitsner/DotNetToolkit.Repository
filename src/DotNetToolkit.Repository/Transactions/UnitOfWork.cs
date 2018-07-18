﻿namespace DotNetToolkit.Repository.Transactions
{
    using Configuration;
    using Factories;
    using Interceptors;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An implementation of <see cref="IUnitOfWork" />.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        #region Fields

        private IRepositoryContext _context;
        private ITransactionManager _transactionManager;
        private readonly IRepositoryFactory _factory;

        private bool _disposed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the repositories.
        /// </summary>
        internal Dictionary<Type, object> Repositories { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        /// <param name="context">The repository context.</param>
        public UnitOfWork(IRepositoryContext context) : this(context, (IEnumerable<IRepositoryInterceptor>)null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        /// <param name="context">The repository context.</param>
        /// <param name="interceptor">The interceptor.</param>
        public UnitOfWork(IRepositoryContext context, IRepositoryInterceptor interceptor) : this(context, new List<IRepositoryInterceptor> { interceptor }) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        /// <param name="context">The repository context.</param>
        /// <param name="interceptors">The interceptors.</param>
        public UnitOfWork(IRepositoryContext context, IEnumerable<IRepositoryInterceptor> interceptors)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _context = context;
            _transactionManager = _context.BeginTransaction();
            _factory = new RepositoryFactory(() => context, interceptors);
            Repositories = new Dictionary<Type, object>();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (_transactionManager != null)
                {
                    _transactionManager.Rollback();
                    _transactionManager.Dispose();
                    _transactionManager = null;
                }

                _context = null;
            }

            _disposed = true;
        }

        #endregion

        #region Implementation of IUnitOfWork

        /// <summary>
        /// Commits all changes made in this unit of work.
        /// </summary>
        public virtual void Commit()
        {
            if (_transactionManager == null)
                throw new InvalidOperationException("The transaction has already been committed or disposed.");

            _transactionManager.Commit();
            _transactionManager = null;
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Implementation of IRepositoryFactory

        /// <summary>
        /// Creates a new repository for the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>The new repository.</returns>
        public IRepository<TEntity> Create<TEntity>() where TEntity : class
        {
            return _factory.Create<TEntity>();
        }

        /// <summary>
        /// Creates a new repository for the specified entity and primary key type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key primary key value.</typeparam>
        /// <returns>The new repository.</returns>
        public IRepository<TEntity, TKey> Create<TEntity, TKey>() where TEntity : class
        {
            return _factory.Create<TEntity, TKey>();
        }

        /// <summary>
        /// Creates a new repository for the specified entity and a composite primary key type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TKey1">The type of the first part of the composite primary key.</typeparam>
        /// <typeparam name="TKey2">The type of the second part of the composite primary key.</typeparam>
        /// <returns>The new repository.</returns>
        public IRepository<TEntity, TKey1, TKey2> Create<TEntity, TKey1, TKey2>() where TEntity : class
        {
            return _factory.Create<TEntity, TKey1, TKey2>();
        }

        /// <summary>
        /// Creates a new repository for the specified entity and a composite primary key type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TKey1">The type of the first part of the composite primary key.</typeparam>
        /// <typeparam name="TKey2">The type of the second part of the composite primary key.</typeparam>
        /// <typeparam name="TKey3">The type of the third part of the composite primary key.</typeparam>
        /// <returns>The new repository.</returns>
        public IRepository<TEntity, TKey1, TKey2, TKey3> Create<TEntity, TKey1, TKey2, TKey3>() where TEntity : class
        {
            return _factory.Create<TEntity, TKey1, TKey2, TKey3>();
        }

        /// <summary>
        /// Creates a new repository for the specified type.
        /// </summary>
        /// <returns>The new repository.</returns>
        public T CreateInstance<T>() where T : class
        {
            return _factory.CreateInstance<T>();
        }

        #endregion
    }
}
