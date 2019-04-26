﻿namespace DotNetToolkit.Repository.Queries.Strategies
{
    using Configuration.Conventions.Internal;
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// An implementation of <see cref="IFetchQueryStrategy{T}" />.
    /// </summary>
    public class FetchQueryStrategy<T> : IFetchQueryStrategy<T>
    {
        #region Fields

        private readonly IList<string> _properties;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchQueryStrategy{T}" /> class.
        /// </summary>
        public FetchQueryStrategy()
        {
            _properties = new List<string>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Defaults this instance.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <returns>The default fetch strategy.</returns>
        public static IFetchQueryStrategy<T> Default()
        {
            var mainTableProperties = typeof(T).GetRuntimeProperties().ToList();

            // Assumes we want to perform a join when the navigation property from the primary table has also a navigation property of
            // the same type as the primary table
            // Only do a join when the primary table has a foreign key property for the join table
            var paths = mainTableProperties
                .Where(x => x.IsComplex() && PrimaryKeyConventionHelper.GetPrimaryKeyPropertyInfos(x.PropertyType).Any())
                .Select(x => x.Name)
                .ToList();

            var fetchStrategy = new FetchQueryStrategy<T>();

            if (paths.Count > 0)
            {
                foreach (var path in paths)
                {
                    fetchStrategy.Fetch(path);
                }
            }

            return fetchStrategy;
        }

        #endregion

        #region Implementation of IFetchQueryStrategy<T>

        /// <summary>
        /// Gets the collection of related objects to include in the query results.
        /// </summary>
        IEnumerable<string> IFetchQueryStrategy<T>.PropertyPaths
        {
            get { return _properties; }
        }

        /// <summary>
        /// Specifies the related objects to include in the query results.
        /// </summary>
        /// <param name="path">A lambda expression representing the path to include.</param>
        /// <returns>A new <see cref="IFetchQueryStrategy{T}" /> with the defined query path.</returns>
        public IFetchQueryStrategy<T> Fetch(Expression<Func<T, object>> path)
        {
            return Fetch(path.ToIncludeString());
        }

        /// <summary>
        /// Specifies the related objects to include in the query results.
        /// </summary>
        /// <param name="path">The dot-separated list of related objects to return in the query results.</param>
        /// <returns>A new <see cref="IFetchQueryStrategy{T}" /> with the defined query path.</returns>
        public IFetchQueryStrategy<T> Fetch(string path)
        {
            if (!_properties.Contains(path))
                _properties.Add(path);

            return this;
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"FetchQueryStrategy<{typeof(T).Name}>: [ Paths = {string.Join(", ", _properties.ToArray())} ]";
        }

        #endregion
    }
}
