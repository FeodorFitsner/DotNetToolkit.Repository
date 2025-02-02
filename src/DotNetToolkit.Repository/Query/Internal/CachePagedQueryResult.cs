﻿namespace DotNetToolkit.Repository.Query.Internal
{
    using JetBrains.Annotations;

    /// <summary>
    /// An implementation of <see cref="ICachePagedQueryResult{TResult}" />.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    internal class CachePagedQueryResult<TResult> : CacheQueryResult<IPagedQueryResult<TResult>>, ICachePagedQueryResult<TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachePagedQueryResult{TResult}"/> class.
        /// </summary>
        /// <param name="hashedKey">The hashed key.</param>
        /// <param name="result">The result.</param>
        public CachePagedQueryResult([NotNull] string hashedKey, [CanBeNull] IPagedQueryResult<TResult> result) : base(hashedKey, result) { }
    }
}
