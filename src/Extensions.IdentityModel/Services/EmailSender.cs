using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Services
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

        /// <summary>
        /// Send an email confirmation.
        /// </summary>
        /// <param name="email">The receiver.</param>
        /// <param name="link">The activate link.</param>
        /// <returns>A task for sending an email.</returns>
        public Task SendEmailConfirmationAsync(string email, string link)
        {
            return SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        }
    }
}
