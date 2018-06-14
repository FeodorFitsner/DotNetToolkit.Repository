﻿namespace DotNetToolkit.Repository.Transactions
{
    using Traits;

    /// <summary>
    /// Maintains business objects in-memory which have been changed (inserted, updated, or deleted) in a single transaction.
    /// Once the transaction is completed, the changes will be sent to the database in one single asynchronous unit of work.
    /// </summary>
    /// <seealso cref="DotNetToolkit.Repository.Transactions.IUnitOfWork"/>
    /// <seealso cref="DotNetToolkit.Repository.Transactions.Traits.ICanAggregateAsync" />
    /// <seealso cref="DotNetToolkit.Repository.Transactions.Traits.ICanAddAsync" />
    /// <seealso cref="DotNetToolkit.Repository.Transactions.Traits.ICanUpdateAsync" />
    /// <seealso cref="DotNetToolkit.Repository.Transactions.Traits.ICanDeleteAsync" />
    /// <seealso cref="DotNetToolkit.Repository.Transactions.Traits.ICanGetAsync" />
    /// <seealso cref="DotNetToolkit.Repository.Transactions.Traits.ICanFindAsync" />
    public interface IUnitOfWorkAsync : IUnitOfWork, ICanAggregateAsync, ICanAddAsync, ICanUpdateAsync, ICanDeleteAsync, ICanGetAsync, ICanFindAsync
    {
    }
}
