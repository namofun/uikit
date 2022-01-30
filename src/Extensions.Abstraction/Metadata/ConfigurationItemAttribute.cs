using SatelliteSite.Entities;
using System;

namespace SatelliteSite
{
    /// <summary>
    /// An attribute attached to an assembly indicating that
    /// the module requires this configuration item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ConfigurationItemAttribute : Attribute
    {
        /// <summary>
        /// The display priority of this configuration item
        /// </summary>
        public int DisplayPriority { get; }

        /// <summary>
        /// The name of this configuration item
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The category of this configuration item
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// The type of this configuration item
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The default value of this configuration item
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// The description of this configuration item
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Whether this configuration item is public
        /// </summary>
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// Construct instance of <see cref="ConfigurationItemAttribute"/>.
        /// </summary>
        /// <param name="priority">The display priority</param>
        /// <param name="category">The category</param>
        /// <param name="name">The name</param>
        /// <param name="type">The type</param>
        /// <param name="value">The default value</param>
        /// <param name="description">The description</param>
        public ConfigurationItemAttribute(int priority, string category, string name, Type type, object value, string description)
        {
            DisplayPriority = priority;
            Category = category;
            Name = name;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            DefaultValue = value;
            Description = description;
        }

        (string, string)? ConvertValueType<T>(string typeName) where T : struct
        {
            if (Type != typeof(T)) return null;
            if (DefaultValue is T value) return (typeName, value.ToJson());
            return null;
        }

        (string, string)? ConvertNullable<T>(string typeName) where T : struct
        {
            if (Type == typeof(T)) return ConvertValueType<T>(typeName);
            if (Type != typeof(T?)) return null;
            if (DefaultValue == null) return (typeName, "null");
            if (DefaultValue is T value) return (typeName, value.ToJson());
            return null;
        }

        (string, string)? ConvertRefType<T>(string typeName) where T : class
        {
            if (Type != typeof(T)) return null;
            if (DefaultValue == null) return (typeName, "null");
            if (DefaultValue is T value) return (typeName, value.ToJson());
            return null;
        }

        /// <summary>
        /// Convert this attribute to an entity.
        /// </summary>
        /// <returns>The converted entity.</returns>
        public Configuration ToEntity()
        {
            string typeString, value;

            (typeString, value) =
                ConvertValueType<int>("int") ??
                ConvertValueType<bool>("bool") ??
                ConvertNullable<DateTimeOffset>("datetime") ??
                ConvertRefType<string>("string") ??
                throw new ArgumentException($"Type {Type.Name} is not supported yet.");

            return new Configuration
            {
                Description = Description,
                Public = IsPublic,
                DisplayPriority = DisplayPriority,
                Category = Category,
                Name = Name,
                Type = typeString,
                Value = value,
            };
        }
    }

    /// <summary>
    /// An attribute attached to an assembly indicating that
    /// the module requires this configuration item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ConfigurationStringAttribute : ConfigurationItemAttribute
    {
        /// <summary>
        /// Construct instance of <see cref="ConfigurationItemAttribute"/>.
        /// </summary>
        /// <param name="priority">The display priority</param>
        /// <param name="category">The category</param>
        /// <param name="name">The name</param>
        /// <param name="value">The default value</param>
        /// <param name="description">The description</param>
        public ConfigurationStringAttribute(int priority, string category, string name, string value, string description)
            : base(priority, category, name, typeof(string), value, description)
        {
        }
    }

    /// <summary>
    /// An attribute attached to an assembly indicating that
    /// the module requires this configuration item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ConfigurationIntegerAttribute : ConfigurationItemAttribute
    {
        /// <summary>
        /// Construct instance of <see cref="ConfigurationItemAttribute"/>.
        /// </summary>
        /// <param name="priority">The display priority</param>
        /// <param name="category">The category</param>
        /// <param name="name">The name</param>
        /// <param name="value">The default value</param>
        /// <param name="description">The description</param>
        public ConfigurationIntegerAttribute(int priority, string category, string name, int value, string description)
            : base(priority, category, name, typeof(int), value, description)
        {
        }
    }

    /// <summary>
    /// An attribute attached to an assembly indicating that
    /// the module requires this configuration item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ConfigurationBooleanAttribute : ConfigurationItemAttribute
    {
        /// <summary>
        /// Construct instance of <see cref="ConfigurationItemAttribute"/>.
        /// </summary>
        /// <param name="priority">The display priority</param>
        /// <param name="category">The category</param>
        /// <param name="name">The name</param>
        /// <param name="value">The default value</param>
        /// <param name="description">The description</param>
        public ConfigurationBooleanAttribute(int priority, string category, string name, bool value, string description)
            : base(priority, category, name, typeof(bool), value, description)
        {
        }
    }
}
