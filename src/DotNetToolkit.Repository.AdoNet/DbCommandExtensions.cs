﻿namespace DotNetToolkit.Repository.AdoNet
{
    using Helpers;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains various extensions for <see cref="DbCommand" />.
    /// </summary>
    public static class DbCommandExtensions
    {
        /// <summary>
        /// Adds a new parameter with the specified name and value to the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public static void AddParmeter(this DbCommand command, string name, object value)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var parameter = command.CreateParameter();

            parameter.Value = value ?? DBNull.Value;
            parameter.ParameterName = name;

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Adds a new collection of parameters to the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <exception cref="System.ArgumentNullException">command</exception>
        public static void AddParmeters(this DbCommand command, Dictionary<string, object> parameters)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (parameters == null || parameters.Count == 0)
                return;

            command.Parameters.Clear();

            foreach (var item in parameters)
            {
                command.AddParmeter(item.Key, item.Value);
            }
        }

        internal static bool ExecuteObjectExist(this DbCommand command, object obj)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var entityType = obj.GetType();
            var tableName = ConventionHelper.GetTableName(entityType);
            var primeryKeyPropertyInfo = ConventionHelper.GetPrimaryKeyPropertyInfos(entityType).First();
            var primeryKeyColumnName = ConventionHelper.GetColumnName(primeryKeyPropertyInfo);

            command.CommandText = $"SELECT * FROM [{tableName}]\nWHERE {primeryKeyColumnName} = @{primeryKeyColumnName}";
            command.CommandType = CommandType.Text;
            command.Parameters.Clear();
            command.AddParmeter($"@{primeryKeyColumnName}", primeryKeyPropertyInfo.GetValue(obj, null));

            var existInDb = false;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    existInDb = true;

                    break;
                }
            }

            return existInDb;
        }

        internal static async Task<bool> ExecuteObjectExistAsync(this DbCommand command, object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var entityType = obj.GetType();
            var tableName = ConventionHelper.GetTableName(entityType);
            var primeryKeyPropertyInfo = ConventionHelper.GetPrimaryKeyPropertyInfos(entityType).First();
            var primeryKeyColumnName = ConventionHelper.GetColumnName(primeryKeyPropertyInfo);

            command.CommandText = $"SELECT * FROM [{tableName}]\nWHERE {primeryKeyColumnName} = @{primeryKeyColumnName}";
            command.CommandType = CommandType.Text;
            command.Parameters.Clear();
            command.AddParmeter($"@{primeryKeyColumnName}", primeryKeyPropertyInfo.GetValue(obj, null));

            var existInDb = false;

            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (reader.Read())
                {
                    existInDb = true;

                    break;
                }
            }

            return existInDb;
        }
    }
}
