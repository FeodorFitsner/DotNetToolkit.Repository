﻿namespace DotNetToolkit.Repository.EntityFrameworkCore.Internal
{
    using Configuration;
    using Configuration.Conventions;
    using Extensions;
    using Extensions.Internal;
    using Microsoft.EntityFrameworkCore;
    using Query;
    using Query.Strategies;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Transactions;
    using Utility;

    /// <summary>
    /// An implementation of <see cref="IEfCoreRepositoryContext" />.
    /// </summary>
    /// <seealso cref="IEfCoreRepositoryContext" />
    internal class EfCoreRepositoryContext : LinqRepositoryContextBaseAsync, IEfCoreRepositoryContext
    {
        #region Fields

        private readonly DbContext _context;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EfCoreRepositoryContext" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public EfCoreRepositoryContext(DbContext context)
        {
            _context = Guard.NotNull(context, nameof(context));
            _context.ConfigureLogging(s => Logger.Debug(s.TrimEnd(Environment.NewLine.ToCharArray())));

            ConfigureConventions(context);
        }

        #endregion

        #region Private Methods

        private void ConfigureConventions(DbContext context)
        {
            var helper = new EfCoreRepositoryConventionHelper(context);

            Conventions = new RepositoryConventions()
            {
                PrimaryKeysCallback = type => helper.GetPrimaryKeyPropertyInfos(type)
            };
        }

        #endregion

        #region Implementation of IEfCoreRepositoryContext

        /// <summary>
        /// Gets the underlying context.
        /// </summary>
        public DbContext UnderlyingContext { get { return _context; } }

        #endregion

        #region Implementation of IRepositoryContext

        /// <summary>
        /// Returns the entity's query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the of the entity.</typeparam>
        /// <returns>The entity's query.</returns>
        protected override IQueryable<TEntity> AsQueryable<TEntity>()
        {
            return _context.Set<TEntity>().AsQueryable();
        }

        /// <summary>
        /// Apply a fetching options to the specified entity's query.
        /// </summary>
        /// <returns>The entity's query with the applied options.</returns>
        protected override IQueryable<TEntity> ApplyFetchingOptions<TEntity>(IQueryable<TEntity> query, IQueryOptions<TEntity> options)
        {
            return query.ApplyFetchingOptions(Conventions, options);
        }

        /// <summary>
        /// Creates a raw SQL query that is executed directly in the database and returns a collection of entities.
        /// </summary>
        /// <param name="sql">The SQL query string.</param>
        /// <param name="cmdType">The command type.</param>
        /// <param name="parameters">The parameters to apply to the SQL query string.</param>
        /// <param name="projector">A function to project each entity into a new form.</param>
        /// <returns>A list which each entity has been projected into a new form.</returns>
        public override IEnumerable<TEntity> ExecuteSqlQuery<TEntity>(string sql, CommandType cmdType, Dictionary<string, object> parameters, Func<IDataReader, TEntity> projector)
        {
            Guard.NotEmpty(sql, nameof(sql));
            Guard.NotNull(projector, nameof(projector));

            var connection = _context.Database.GetDbConnection();
            var command = connection.CreateCommand();
            var shouldOpenConnection = connection.State != ConnectionState.Open;

            if (shouldOpenConnection)
                connection.Open();

            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Parameters.Clear();
            command.AddParameters(parameters);

            using (var reader = command.ExecuteReader(shouldOpenConnection ? CommandBehavior.CloseConnection : CommandBehavior.Default))
            {
                var list = new List<TEntity>();

                while (reader.Read())
                {
                    list.Add(projector(reader));
                }

                return list;
            }
        }

        /// <summary>
        /// Creates a raw SQL query that is executed directly in the database.
        /// </summary>
        /// <param name="sql">The SQL query string.</param>
        /// <param name="cmdType">The command type.</param>
        /// <param name="parameters">The parameters to apply to the SQL query string.</param>
        /// <returns>The number of rows affected.</returns>
        public override int ExecuteSqlCommand(string sql, CommandType cmdType, Dictionary<string, object> parameters)
        {
            Guard.NotEmpty(sql, nameof(sql));

            var connection = _context.Database.GetDbConnection();
            var shouldOpenConnection = connection.State != ConnectionState.Open;

            if (shouldOpenConnection)
                connection.Open();

            try
            {
                using (var command = connection.CreateCommand())
                {
                    if (shouldOpenConnection)
                        connection.Open();

                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    command.Parameters.Clear();
                    command.AddParameters(parameters);

                    return command.ExecuteNonQuery();
                }
            }
            finally
            {

                if (shouldOpenConnection)
                    connection.Close();
            }
        }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <returns>The transaction.</returns>
        public override ITransactionManager BeginTransaction()
        {
            CurrentTransaction = new EfCoreTransactionManager(_context.Database.BeginTransaction());

            return CurrentTransaction;
        }

        /// <summary>
        /// Tracks the specified entity in memory and will be inserted into the database when <see cref="M:DotNetToolkit.Repository.IContext.SaveChanges" /> is called.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        public override void Add<TEntity>(TEntity entity)
        {
            _context.Set<TEntity>().Add(Guard.NotNull(entity, nameof(entity)));
        }

        /// <summary>
        /// Tracks the specified entity in memory and will be updated in the database when <see cref="M:DotNetToolkit.Repository.IContext.SaveChanges" /> is called.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        public override void Update<TEntity>(TEntity entity)
        {
            Guard.NotNull(entity, nameof(entity));

            var entry = _context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var keyValues = Conventions.GetPrimaryKeyValues(entity);

                var entityInDb = _context.Set<TEntity>().Find(keyValues);

                if (entityInDb != null)
                {
                    _context.Entry(entityInDb).CurrentValues.SetValues(entity);
                }
            }
            else
            {
                entry.State = EntityState.Modified;
            }
        }

        /// <summary>
        /// Tracks the specified entity in memory and will be removed from the database when <see cref="M:DotNetToolkit.Repository.IContext.SaveChanges" /> is called.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        public override void Remove<TEntity>(TEntity entity)
        {
            Guard.NotNull(entity, nameof(entity));

            if (_context.Entry(entity).State == EntityState.Detached)
            {
                var keyValues = Conventions.GetPrimaryKeyValues(entity);

                var entityInDb = _context.Set<TEntity>().Find(keyValues);

                if (entityInDb != null)
                {
                    _context.Set<TEntity>().Remove(entityInDb);
                }
            }
            else
            {
                _context.Set<TEntity>().Remove(entity);
            }
        }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>
        /// The number of state entries written to the database.
        /// </returns>
        public override int SaveChanges()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// Finds an entity with the given primary key values in the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the of the entity.</typeparam>
        /// <param name="fetchStrategy">Defines the child objects that should be retrieved when loading the entity.</param>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found in the repository.</returns>
        public override TEntity Find<TEntity>(IFetchQueryStrategy<TEntity> fetchStrategy, params object[] keyValues)
        {
            Guard.NotEmpty(keyValues, nameof(keyValues));

            if (fetchStrategy == null)
            {
                var result = _context.Set<TEntity>().Find(keyValues);

                return result;
            }

            return base.Find(fetchStrategy, keyValues);
        }

        #endregion

        #region Implementation of IRepositoryContextAsync

        /// <summary>
        /// An overridable method to return the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        protected override Task<TSource> FirstOrDefaultAsync<TSource>(IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            return source.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// An overridable method to create a <see cref="T:System.Collections.Generic.List`1" /> from an <see cref="T:System.Linq.IQueryable`1" /> by enumerating it asynchronously.
        /// </summary>
        protected override Task<List<TSource>> ToListAsync<TSource>(IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            return source.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// An overridable method to return the number of elements in a sequence.
        /// </summary>
        protected override Task<int> CountAsync<TSource>(IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            return source.CountAsync(cancellationToken);
        }

        /// <summary>
        /// An overridable method to determine whether a sequence contains any elements.
        /// </summary>
        protected override Task<bool> AnyAsync<TSource>(IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            return source.AnyAsync(cancellationToken);
        }

        /// <summary>
        /// An overridable method to create a <see cref="T:System.Collections.Generic.Dictionary`2" /> from an <see cref="T:System.Linq.IQueryable`1" /> by enumerating it asynchronously  according to a specified key selector and an element selector function.
        /// </summary>
        protected override Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken)
        {
            return source.ToDictionaryAsync(keySelector, elementSelector, cancellationToken);
        }

        /// <summary>
        /// Asynchronously creates raw SQL query that is executed directly in the database and returns a collection of entities.
        /// </summary>
        /// <param name="sql">The SQL query string.</param>
        /// <param name="cmdType">The command type.</param>
        /// <param name="parameters">The parameters to apply to the SQL query string.</param>
        /// <param name="projector">A function to project each entity into a new form.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="System.Threading.Tasks.Task" /> that represents the asynchronous operation, containing a list which each entity has been projected into a new form.</returns> 
        public override async Task<IEnumerable<TEntity>> ExecuteSqlQueryAsync<TEntity>(string sql, CommandType cmdType, Dictionary<string, object> parameters, Func<IDataReader, TEntity> projector, CancellationToken cancellationToken = new CancellationToken())
        {
            Guard.NotEmpty(sql, nameof(sql));
            Guard.NotNull(projector, nameof(projector));

            var connection = _context.Database.GetDbConnection();
            var command = connection.CreateCommand();
            var shouldOpenConnection = connection.State != ConnectionState.Open;

            if (shouldOpenConnection)
                await connection.OpenAsync(cancellationToken);

            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Parameters.Clear();
            command.AddParameters(parameters);

            using (var reader = await command.ExecuteReaderAsync(shouldOpenConnection ? CommandBehavior.CloseConnection : CommandBehavior.Default, cancellationToken))
            {
                var list = new List<TEntity>();

                while (await reader.ReadAsync(cancellationToken))
                {
                    list.Add(projector(reader));
                }

                return list;
            }
        }

        /// <summary>
        /// Asynchronously creates raw SQL query that is executed directly in the database.
        /// </summary>
        /// <param name="sql">The SQL query string.</param>
        /// <param name="cmdType">The command type.</param>
        /// <param name="parameters">The parameters to apply to the SQL query string.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="System.Threading.Tasks.Task" /> that represents the asynchronous operation, containing the number of rows affected.</returns>
        public override async Task<int> ExecuteSqlCommandAsync(string sql, CommandType cmdType, Dictionary<string, object> parameters, CancellationToken cancellationToken = new CancellationToken())
        {
            Guard.NotEmpty(sql, nameof(sql));

            var connection = _context.Database.GetDbConnection();
            var shouldOpenConnection = connection.State != ConnectionState.Open;

            try
            {
                using (var command = connection.CreateCommand())
                {
                    if (shouldOpenConnection)
                        await connection.OpenAsync(cancellationToken);

                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    command.Parameters.Clear();
                    command.AddParameters(parameters);

                    return await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
            finally
            {

                if (shouldOpenConnection)
                    connection.Close();
            }
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation, containing the number of state entries written to the database.</returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously finds an entity with the given primary key values in the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the of the entity.</typeparam>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <param name="fetchStrategy">Defines the child objects that should be retrieved when loading the entity.</param>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation, containing the entity found in the repository.</returns>
        public override async Task<TEntity> FindAsync<TEntity>(CancellationToken cancellationToken, IFetchQueryStrategy<TEntity> fetchStrategy, params object[] keyValues)
        {
            Guard.NotEmpty(keyValues, nameof(keyValues));

            if (fetchStrategy == null)
            {
                var result = await _context.Set<TEntity>().FindAsync(keyValues, cancellationToken);

                return result;
            }

            return await base.FindAsync(cancellationToken, fetchStrategy, keyValues);
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _context.Dispose();

            base.Dispose();
        }

        #endregion

    }
}