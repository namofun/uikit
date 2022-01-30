using Microsoft.Extensions.Mailing;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Mailing
{
    internal static class EmailSenderExtensions
    {
        /// <summary>
        /// Send an email confirmation.
        /// </summary>
        /// <param name="emailSender">The email sender.</param>
        /// <param name="email">The receiver.</param>
        /// <param name="link">The activate link.</param>
        /// <returns>A task for sending an email.</returns>
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        }
    }
}
