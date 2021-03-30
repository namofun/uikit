#nullable enable
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace System
{
    /// <summary>
    /// The basic extensions that is used frequently.
    /// </summary>
    public static class BasicExtensions
    {
        /// <summary>
        /// Get the source string from Base64-encoded string.
        /// </summary>
        /// <param name="value">The base64 encoded string.</param>
        /// <returns>The original string.</returns>
        public static string UnBase64(this string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Get the base64 encoded string from the source string.
        /// </summary>
        /// <param name="value">The source string.</param>
        /// <returns>The base64 encoded string.</returns>
        public static string ToBase64(this string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Convert the json object string to an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="jsonString">The source json string.</param>
        /// <returns>The deserialized object.</returns>
        /// <exception cref="JsonException" />
        public static T AsJson<T>(this string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString)) return default!;
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Convert the instance of <typeparamref name="T"/> to the json object.
        /// </summary>
        /// <typeparam name="T">The source type.</typeparam>
        /// <param name="value">The source object.</param>
        /// <returns>The serialized string.</returns>
        public static string ToJson<T>(this T value)
        {
            return JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Write the source to destination span.
        /// </summary>
        /// <param name="toWrite">The span to write.</param>
        /// <param name="source">The source to read.</param>
        /// <param name="chars">The character set to use.</param>
        /// <returns>The result string.</returns>
        private static string ToHexDigest(Span<char> toWrite, byte[] source, char[] chars)
        {
            if (toWrite.Length != source.Length * 2) throw new InvalidOperationException();
            if (chars.Length != 16) throw new InvalidOperationException();

            for (int i = 0; i < source.Length; i++)
            {
                var ch = source[i];
                var bit = (ch & 0x0f0) >> 4;
                toWrite[i << 1] = chars[bit];
                bit = ch & 0x0f;
                toWrite[(i << 1) | 1] = chars[bit];
            }

            return toWrite.ToString();
        }

        /// <summary>
        /// Convert the byte[] to corresponding hexademical string.
        /// </summary>
        /// <param name="source">The source byte array.</param>
        /// <param name="lower">Whether the result is lowercase.</param>
        /// <returns>The hexadecimal string.</returns>
        public static string ToHexDigest(this byte[] source, bool lower = false)
        {
            var chars = (lower ? "0123456789abcdef" : "0123456789ABCDEF").ToCharArray();
            if (source.Length <= 4096)
            {
                return ToHexDigest(stackalloc char[source.Length * 2], source, chars);
            }
            else
            {
                var buffer = ArrayPool<char>.Shared.Rent(source.Length * 2);
                var result = ToHexDigest(new ArraySegment<char>(buffer, 0, source.Length * 2), source, chars);
                ArrayPool<char>.Shared.Return(buffer);
                return result;
            }
        }

        /// <summary>
        /// Gets the MD5 result for byte[].
        /// </summary>
        /// <param name="source">The source to calculate.</param>
        /// <returns>The calculated MD5 result.</returns>
        public static byte[] ToMD5(this byte[] source)
        {
            using var MD5p = new MD5CryptoServiceProvider();
            return MD5p.ComputeHash(source);
        }

        /// <summary>
        /// Gets the MD5 result for <see cref="Stream" />.
        /// </summary>
        /// <param name="source">The source to calculate.</param>
        /// <returns>The calculated MD5 result.</returns>
        public static byte[] ToMD5(this Stream source)
        {
            using var MD5p = new MD5CryptoServiceProvider();
            return MD5p.ComputeHash(source);
        }

        /// <summary>
        /// Gets the MD5 result for <see cref="string" />.
        /// </summary>
        /// <param name="source">The source to calculate.</param>
        /// <param name="encoding">The encoding, defaults UTF-8.</param>
        /// <returns>The calculated MD5 string.</returns>
        [Obsolete]
        public static string ToMD5(this string source, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return encoding.GetBytes(source).ToMD5().ToHexDigest(true);
        }
    }
}
