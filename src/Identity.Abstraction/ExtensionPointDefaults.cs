namespace SatelliteSite.IdentityModule
{
    /// <summary>
    /// Identity module extension points.
    /// </summary>
    public static class ExtensionPointDefaults
    {
        /// <summary>
        /// The user detail page <c>/profile/{username}</c> component.
        /// </summary>
        public const string UserDetail = "Component_UserDetail";

        /// <summary>
        /// The dashboard user detail page <c>/dashboard/users/{username}</c> component.
        /// </summary>
        public const string DashboardUserDetail = "Component_DashboardUserDetail";
    }
}
