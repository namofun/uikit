namespace Microsoft.AspNetCore.Identity
{
    /// <summary>
    /// The notification to send when a registration in front-end is required.
    /// </summary>
    public class RegisterNotification : MediatR.INotification
    {
        /// <summary>
        /// Whether the user name is invalid
        /// </summary>
        public bool Failed { get; private set; }

        /// <summary>
        /// The username to register
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Marks the user name as invalid.
        /// </summary>
        public void Fail()
        {
            Failed = true;
        }

        /// <summary>
        /// Constructs a notification for register check.
        /// </summary>
        /// <param name="toValidate">The username to validate.</param>
        public RegisterNotification(string toValidate)
        {
            Username = toValidate;
        }
    }
}
