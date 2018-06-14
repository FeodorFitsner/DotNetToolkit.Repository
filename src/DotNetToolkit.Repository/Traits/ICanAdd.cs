﻿namespace DotNetToolkit.Repository.Traits
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a trait for adding items to a repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface ICanAdd<in TEntity> where TEntity : class
    {
        /// <summary>
        /// Adds the specified <paramref name="entity" /> into the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        void Add(TEntity entity);

        /// <summary>
        /// Adds the specified <paramref name="entities" /> collection into the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to add.</param>
        void Add(IEnumerable<TEntity> entities);
    }
}

namespace DotNetToolkit.Repository.Transactions.Traits
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a trait for adding items to a unit of work repository.
    /// </summary>
    public interface ICanAdd
    {
        /// <summary>
        /// Adds the specified <paramref name="entity" /> into the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to add.</param>
        void Add<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Adds the specified <paramref name="entities" /> collection into the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The collection of entities to add.</param>
        void Add<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    }
}
