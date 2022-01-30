using System.Threading.Tasks;

namespace Microsoft.Extensions.Mailing
{
    /// <summary>
    /// The interface to provide mail sending functions.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Send an email to the corresponding email address.
        /// </summary>
        /// <param name="email">The receiver.</param>
        /// <param name="subject">The mail title.</param>
        /// <param name="message">The mail body with HTML support.</param>
        /// <returns>A task for sending an email.</returns>
        Task SendEmailAsync(string email, string subject, string message);
    }
}
