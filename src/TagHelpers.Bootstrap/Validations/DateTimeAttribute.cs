namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Validate the <see cref="DateTimeOffset"/> property.
    /// </summary>
    public sealed class DateTimeAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        public override bool IsValid(object? value)
        {
            if (value == null) return true;
            if (value is not string realValue) return false;
            if (string.IsNullOrWhiteSpace(realValue)) return true;
            return DateTimeOffset.TryParse(realValue, out _);
        }

        /// <inheritdoc />
        public override string FormatErrorMessage(string name)
        {
            return $"Error parsing the format of datetime {name}.";
        }
    }
}
