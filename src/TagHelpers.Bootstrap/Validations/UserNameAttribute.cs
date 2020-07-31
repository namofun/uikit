using System.Collections.Generic;
using System.Linq;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Validate the UserName property.
    /// </summary>
    public sealed class UserNameAttribute : ValidationAttribute
    {
        /// <summary>
        /// The allowed characters.
        /// </summary>
        static readonly ISet<char> AllowedCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_@.".ToHashSet();

        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            if (value is string str)
            {
                foreach (char t in str)
                    if (!AllowedCharacters.Contains(t))
                        return false;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override string FormatErrorMessage(string name)
        {
            return string.Format("The {0} must consist of only 0-9, a-z and A-Z.", name);
        }
    }
}
