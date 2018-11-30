﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DotNetToolkit.Repository.Properties {
    using System.Reflection;


    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DotNetToolkit.Repository.Properties.Resources", typeof(Resources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No entity found in the repository with the &apos;{0}&apos; key..
        /// </summary>
        internal static string EntityKeyNotFound {
            get {
                return ResourceManager.GetString("EntityKeyNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The repository primary key type(s) constraint must match the number of primary key type(s) and ordering defined on the entity..
        /// </summary>
        internal static string EntityPrimaryKeyTypesMismatch {
            get {
                return ResourceManager.GetString("EntityPrimaryKeyTypesMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The number of primary key values passed must match number of primary key values defined on the entity..
        /// </summary>
        internal static string EntityPrimaryKeyValuesLengthMismatch {
            get {
                return ResourceManager.GetString("EntityPrimaryKeyValuesLengthMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The instance of entity type &apos;{0}&apos; requires a primary key to be defined..
        /// </summary>
        internal static string EntityRequiresPrimaryKey {
            get {
                return ResourceManager.GetString("EntityRequiresPrimaryKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The ForeignKeyAttribute on property &apos;{0}&apos; on type &apos;{1}&apos; is not valid. The foreign key name &apos;{2}&apos; was not found on the dependent type &apos;{1}&apos;. The Name value should be a comma separated list of foreign key property names..
        /// </summary>
        internal static string ForeignKeyAttributeOnPropertyNotFoundOnDependentType {
            get {
                return ResourceManager.GetString("ForeignKeyAttributeOnPropertyNotFoundOnDependentType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to determine composite primary key ordering for type &apos;{0}&apos;. Use the ColumnAttribute to specify an order for composite primary keys..
        /// </summary>
        internal static string UnableToDetermineCompositePrimaryKeyOrdering {
            get {
                return ResourceManager.GetString("UnableToDetermineCompositePrimaryKeyOrdering", resourceCulture);
            }
        }
    }
}
