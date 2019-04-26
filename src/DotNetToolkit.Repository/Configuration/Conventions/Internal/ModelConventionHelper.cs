﻿namespace DotNetToolkit.Repository.Configuration.Conventions.Internal
{
    using Extensions;
    using JetBrains.Annotations;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Reflection;
    using Utility;

    internal static class ModelConventionHelper
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The name of the table.</returns>
        public static string GetTableName([NotNull] this Type type)
        {
            Guard.NotNull(type);

            if (InMemoryCache.Instance.TableNameMapping.ContainsKey(type))
                return InMemoryCache.Instance.TableNameMapping[type];

            var tableName = type.GetTypeInfo().GetCustomAttribute<TableAttribute>()?.Name;

            if (string.IsNullOrEmpty(tableName))
                tableName = PluralizationService.Pluralize(type.Name);

            InMemoryCache.Instance.TableNameMapping[type] = tableName;

            return tableName;
        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <returns>The name of the table.</returns>
        public static string GetTableName<T>()
        {
            return GetTableName(typeof(T));
        }

        /// <summary>
        /// Determines whether the specified property is mapped (does not have a <see cref="NotMappedAttribute" /> defined).
        /// </summary>
        /// <param name="pi">The property info.</param>
        /// <returns>
        ///   <c>true</c> if the property is mapped; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsColumnMapped([NotNull] this PropertyInfo pi)
        {
            Guard.NotNull(pi);

            if (pi.GetCustomAttribute<NotMappedAttribute>() != null)
                return false;

            // Ensures the property has public setter
            return pi.CanWrite && pi.GetSetMethod(nonPublic: true).IsPublic;
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="pi">The property info.</param>
        /// <returns>The name of the column.</returns>
        public static string GetColumnName([NotNull] this PropertyInfo pi)
        {
            Guard.NotNull(pi);

            // If this is a complex object then don't worry about finding a column attribute for it
            if (pi.IsComplex())
                return pi.Name;

            var columnName = pi.GetCustomAttribute<ColumnAttribute>()?.Name;

            if (string.IsNullOrEmpty(columnName))
                columnName = pi.Name;

            return columnName;
        }

        /// <summary>
        /// Gets the column order for the specified property.
        /// </summary>
        /// <param name="pi">The property info.</param>
        /// <returns>The column order.</returns>
        public static int GetColumnOrder([NotNull] this PropertyInfo pi)
        {
            Guard.NotNull(pi);

            var columnAttribute = pi.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute == null)
            {
                // Checks to see if the property is a primary key, and if so, try to give it the lowest ordering number
                var declaringTypePrimaryKeyPropertyInfo = PrimaryKeyConventionHelper.GetPrimaryKeyPropertyInfos(pi.DeclaringType).First();
                if (declaringTypePrimaryKeyPropertyInfo.Name.Equals(pi.Name))
                    return -1;
            }
            else if (columnAttribute.Order > 0)
            {
                return columnAttribute.Order;
            }

            return Int32.MaxValue;
        }

        /// <summary>
        /// Determines whether the specified property is defined as identity.
        /// </summary>
        /// <param name="pi">The property info.</param>
        /// <returns><c>true</c> if the specified property is defined as identity; otherwise, <c>false</c>.</returns>
        /// <remarks>If no the property does not have a <see cref="DatabaseGeneratedOption.Identity" /> option defined, this function will returned <c>true</c> as default.</remarks>
        public static bool IsColumnIdentity([NotNull] this PropertyInfo pi)
        {
            Guard.NotNull(pi);

            var databaseGeneratedAttribute = pi.GetCustomAttribute<DatabaseGeneratedAttribute>();
            if (databaseGeneratedAttribute == null)
            {
                var declaringType = pi.DeclaringType;
                var primaryKeyPropertyInfo = PrimaryKeyConventionHelper.GetPrimaryKeyPropertyInfos(declaringType).First();

                return primaryKeyPropertyInfo.Name.Equals(pi.Name) && !PrimaryKeyConventionHelper.HasCompositePrimaryKey(declaringType);
            }

            return databaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
        }
    }
}
