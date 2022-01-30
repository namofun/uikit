using System;

namespace SatelliteSite
{
    /// <summary>
    /// Attribute definition for roles in this module.
    /// </summary>
    /// <remarks>
    /// You should keep name, short name and description the same in different modules.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RoleDefinitionAttribute : Attribute
    {
        /// <summary>
        /// The name of this role
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The short name of this role
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// The description of this role
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The normalized name of this role
        /// </summary>
        public string NormalizedName => string.Intern(Name).ToUpperInvariant();

        /// <summary>
        /// The id of this role
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Generate the value of field ConcurrencyStamp.
        /// </summary>
        /// <returns>The generated concurrency stamp.</returns>
        public string GenerateDefaultConcurrencyStamp()
        {
            var guidValue = new byte[16];
            int seedBase = GetMyHashCode(Name) ^ GetMyHashCode(ShortName) ^ GetMyHashCode(Description) ^ Id;

            ulong k1 = (ulong)seedBase * (ulong)seedBase;
            ulong k2 = k1 ^ (k1 << 23);
            ulong k3 = k2 ^ (k1 >> 17) ^ (k2 << 26);

            for (int i = 0; i < 8; i++) guidValue[i] = (byte)((k2 >> (i << 3)) & 255);
            for (int i = 0; i < 8; i++) guidValue[i + 8] = (byte)((k3 >> (i << 3)) & 255);

            return new Guid(guidValue).ToString("D");

            static int GetMyHashCode(string content)
            {
                int result = 0;
                for (int i = 0; i < content.Length; i++)
                    result = result * 61357 + content[i];
                return result;
            }
        }

        /// <summary>
        /// Create an instance of role definition.
        /// </summary>
        /// <param name="id">The positive ID</param>
        /// <param name="name">The name</param>
        /// <param name="shortName">The short name</param>
        /// <param name="description">The description</param>
        public RoleDefinitionAttribute(int id, string name, string shortName, string description)
        {
            Id = id;
            Name = name;
            ShortName = shortName;
            Description = description;
        }
    }
}
