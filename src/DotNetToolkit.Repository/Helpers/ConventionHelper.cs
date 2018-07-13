﻿namespace DotNetToolkit.Repository.Helpers
{
    using Properties;
    using Specifications;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Represents a convention helper for the repositories.
    /// </summary>
    internal static class ConventionHelper
    {
        /// <summary>
        /// Gets the composite primary key property information for the specified type.
        /// </summary>
        /// <param name="entityType">The entity type to get the primary key from.</param>
        /// <returns>The composite primary key property infos.</returns>
        public static IEnumerable<PropertyInfo> GetPrimaryKeyPropertyInfos(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            if (InMemoryCache.Instance.PrimaryKeyMapping.ContainsKey(entityType))
                return InMemoryCache.Instance.PrimaryKeyMapping[entityType];

            // Gets by checking the annotations
            var propertyInfos = entityType
                .GetRuntimeProperties()
                .Where(x => IsMapped(x) && x.GetCustomAttribute<KeyAttribute>() != null)
                .ToList();

            // Gets by naming convention
            if (!propertyInfos.Any())
            {
                foreach (var propertyName in GetDefaultPrimaryKeyNameChecks(entityType))
                {
                    var propertyInfo = entityType.GetTypeInfo().GetDeclaredProperty(propertyName);

                    if (propertyInfo != null && IsMapped(propertyInfo))
                    {
                        propertyInfos.Add(propertyInfo);

                        break;
                    }
                }
            }

            if (propertyInfos.Any())
            {
                InMemoryCache.Instance.PrimaryKeyMapping[entityType] = propertyInfos;

                return propertyInfos;
            }

            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.EntityRequiresPrimaryKey, entityType));
        }

        /// <summary>
        /// Determines whether the specified entity type has a composite primary key defined.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        public static bool HasCompositePrimaryKey(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            return GetPrimaryKeyPropertyInfos(entityType).Count() > 1;
        }

        /// <summary>
        /// Gets the composite primary key property information for the specified type.
        /// </summary>
        /// <returns>The primary key property infos.</returns>
        public static IEnumerable<PropertyInfo> GetPrimaryKeyPropertyInfos<T>()
        {
            return GetPrimaryKeyPropertyInfos(typeof(T));
        }

        /// <summary>
        /// Returns a specification for getting an entity by it's primary key.
        /// </summary>
        /// <returns>The new specification.</returns>
        public static ISpecification<TEntity> GetByPrimaryKeySpecification<TEntity>(params object[] keyValues) where TEntity : class
        {
            if (keyValues == null)
                throw new ArgumentNullException(nameof(keyValues));

            var propInfos = GetPrimaryKeyPropertyInfos<TEntity>().ToList();

            if (keyValues.Length != propInfos.Count)
                throw new ArgumentException(Resources.EntityPrimaryKeyValuesLengthMismatch, nameof(keyValues));

            var parameter = Expression.Parameter(typeof(TEntity), "x");

            BinaryExpression exp = null;

            for (var i = 0; i < propInfos.Count; i++)
            {
                var propInfo = propInfos[i];
                var keyValue = keyValues[i];

                var x = Expression.Equal(
                    Expression.PropertyOrField(parameter, propInfo.Name),
                    Expression.Constant(keyValue));

                exp = exp == null ? x : Expression.AndAlso(x, exp);
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(exp, parameter);

            return new Specification<TEntity>(lambda);
        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>The name of the table.</returns>
        public static string GetTableName(Type entityType)
        {
            if (InMemoryCache.Instance.TableNameMapping.ContainsKey(entityType))
                return InMemoryCache.Instance.TableNameMapping[entityType];

            var tableName = entityType.GetTypeInfo().GetCustomAttribute<TableAttribute>()?.Name;

            if (string.IsNullOrEmpty(tableName))
                tableName = PluralizationHelper.Pluralize(entityType.Name);

            InMemoryCache.Instance.TableNameMapping[entityType] = tableName;

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
        public static bool IsMapped(PropertyInfo pi)
        {
            if (pi == null)
                throw new ArgumentNullException(nameof(pi));

            return pi.GetCustomAttribute<NotMappedAttribute>() == null;
        }

        /// <summary>
        /// Gets the column order for the specified property.
        /// </summary>
        /// <param name="pi">The property info.</param>
        /// <returns>The column order.</returns>
        public static int GetColumnOrder(PropertyInfo pi)
        {
            if (pi == null)
                throw new ArgumentNullException(nameof(pi));

            var columnAttribute = pi.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute == null)
                return -1;

            return columnAttribute.Order;
        }

        /// <summary>
        /// Gets the collection of foreign key properties that matches the specified foreign type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="foreignType">The foreign type to match.</param>
        /// <returns>The collection of foreign key properties.</returns>
        public static IEnumerable<PropertyInfo> GetForeignKeyPropertyInfos(Type sourceType, Type foreignType)
        {
            var tupleKey = Tuple.Create(sourceType, foreignType);

            if (InMemoryCache.Instance.ForeignKeyMapping.ContainsKey(tupleKey))
                return InMemoryCache.Instance.ForeignKeyMapping[tupleKey];

            var properties = sourceType.GetRuntimeProperties().Where(IsMapped);
            var foreignNavigationPropertyInfo = properties.SingleOrDefault(x => x.PropertyType == foreignType);
            var propertyInfos = new List<PropertyInfo>();

            if (foreignNavigationPropertyInfo != null)
            {
                var propertyInfosWithForeignKeys = properties.Where(x => x.GetCustomAttribute<ForeignKeyAttribute>() != null);
                if (propertyInfosWithForeignKeys.Any())
                {
                    // Try to find by checking on the foreign key property
                    propertyInfos = propertyInfosWithForeignKeys
                        .Where(IsPrimitive)
                        .Where(x => x.GetCustomAttribute<ForeignKeyAttribute>().Name.Equals(foreignNavigationPropertyInfo.Name))
                        .ToList();

                    // Try to find by checking on the navigation property
                    if (!propertyInfos.Any())
                    {
                        propertyInfos = properties
                            .Where(IsPrimitive)
                            .Where(x => foreignNavigationPropertyInfo.GetCustomAttribute<ForeignKeyAttribute>().Name.Equals(GetColumnName(x)))
                            .ToList();
                    }
                }

                // Try to find by naming convention
                if (!propertyInfos.Any() && !HasCompositePrimaryKey(foreignType))
                {
                    var primaryKeyPropertyInfo = GetPrimaryKeyPropertyInfos(foreignType).FirstOrDefault();
                    if (primaryKeyPropertyInfo != null)
                    {
                        var foreignPrimaryKeyName = GetColumnName(primaryKeyPropertyInfo);
                        var propertyInfo = properties.SingleOrDefault(x => x.Name == $"{foreignNavigationPropertyInfo.Name}{foreignPrimaryKeyName}");

                        if (propertyInfo != null)
                        {
                            propertyInfos.Add(propertyInfo);
                        }
                    }
                }
            }

            InMemoryCache.Instance.ForeignKeyMapping[tupleKey] = propertyInfos;

            return propertyInfos;
        }

        /// <summary>
        /// Gets a dictionary of foreign properties with the key as the foreign key column name for the navigation property, and the value as the navigation property.
        /// </summary>
        /// <param name="entityType">The type of the entity that contains all the foreign keys.</param>
        /// <returns>A dictionary of foreign properties with the key as the foreign key column name for the navigation property, and the value as the navigation property.</returns>
        public static Dictionary<string, PropertyInfo> GetForeignKeyPropertiesMapping(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            var foreignNavigationPropertyInfos = entityType.GetRuntimeProperties().Where(IsComplex);
            var foreignKeyPropertyInfosMapping = new Dictionary<string, PropertyInfo>();

            foreach (var foreignNavigationPropertyInfo in foreignNavigationPropertyInfos)
            {
                var dict = GetForeignKeyPropertyInfos(entityType, foreignNavigationPropertyInfo.PropertyType)
                    .ToDictionary(GetColumnName, x => foreignNavigationPropertyInfo);

                foreach (var item in dict)
                {
                    foreignKeyPropertyInfosMapping.Add(item.Key, item.Value);
                }
            }

            return foreignKeyPropertyInfosMapping;
        }

        /// <summary>
        /// Determines if the specified property is a complex type.
        /// </summary>
        /// <param name="pi">The property info.</param>
        /// <returns><c>true</c> if the specified type is a complex type; otherwise, <c>false</c>.</returns>
        public static bool IsComplex(PropertyInfo pi)
        {
            return pi.PropertyType.Namespace != "System";
        }

        /// <summary>
        /// Determines if the specified property is a primitive type.
        /// </summary>
        /// <param name="pi">The property info.</param>
        /// <returns><c>true</c> if the specified type is a primitive type; otherwise, <c>false</c>.</returns>
        public static bool IsPrimitive(PropertyInfo pi)
        {
            return !IsComplex(pi);
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="pi">The property info.</param>
        /// <returns>The name of the column.</returns>
        public static string GetColumnName(PropertyInfo pi)
        {
            if (pi == null)
                throw new ArgumentNullException(nameof(pi));

            // If this is a complex object then don't worry about finding a column attribute for it
            if (IsComplex(pi))
                return pi.Name;

            var columnName = pi.GetCustomAttribute<ColumnAttribute>()?.Name;

            if (string.IsNullOrEmpty(columnName))
                columnName = pi.Name;

            return columnName;
        }

        /// <summary>
        /// Determines whether the specified property is defined as identity.
        /// </summary>
        /// <param name="pi">The property info.</param>
        /// <returns><c>true</c> if the specified property is defined as identity; otherwise, <c>false</c>.</returns>
        /// <remarks>If no the property does not have a <see cref="DatabaseGeneratedOption.Identity" /> option defined, this function will returned <c>true</c> as default.</remarks>
        public static bool IsIdentity(PropertyInfo pi)
        {
            if (pi == null)
                throw new ArgumentNullException(nameof(pi));

            var databaseGeneratedAttribute = pi.GetCustomAttribute<DatabaseGeneratedAttribute>();
            if (databaseGeneratedAttribute == null)
            {
                var declaringType = pi.DeclaringType;

                if (HasCompositePrimaryKey(declaringType))
                    return false;

                var primaryKeyPropertyInfo = GetPrimaryKeyPropertyInfos(declaringType).First();

                return primaryKeyPropertyInfo.Name.Equals(pi.Name);
            }

            return databaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
        }

        /// <summary>
        /// Gets the primary key name checks.
        /// </summary>
        /// <param name="entityType">The entity type to get the primary key from.</param>
        /// <remarks>Assumes the entity has either an 'Id' property or 'EntityName' + 'Id'.</remarks>
        /// <returns>The list of primary key names to check.</returns>
        private static IEnumerable<string> GetDefaultPrimaryKeyNameChecks(Type entityType)
        {
            const string suffix = "Id";

            return new[] { suffix, entityType.Name + suffix };
        }
    }
}