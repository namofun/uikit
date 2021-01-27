namespace Microsoft.AspNetCore.Identity
{
    /// <inheritdoc />
    public interface ILightweightUserClaimsPrincipalFactory<TUser> :
        IUserClaimsPrincipalFactory<TUser>
        where TUser : class
    {
    }
}
