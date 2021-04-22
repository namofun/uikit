#nullable enable
using Microsoft.AspNetCore.Mvc.Routing;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    public class UserInformationProviderV2 : UserInformationProviderBase<MediatR.Unit>
    {
        private readonly IUserInformationCache<string?> _cache;
        private readonly IUserManager _userManager;

        public UserInformationProviderV2(
            IUrlHelperFactory urlHelperFactory,
            IUserInformationCache<string?> cache,
            IUserManager userManager)
            : base(urlHelperFactory)
        {
            _cache = cache;
            _userManager = userManager;
        }

        protected override async Task<(MediatR.Unit, string)?> GetUserAsync(int userId, string? userName)
        {
            userName = await _cache.GetByUserIdAsync(
                userId,
                async uid => (await _userManager.FindByIdAsync(uid))?.UserName);

            return userName == null ? default((MediatR.Unit, string)?) : (MediatR.Unit.Value, userName);
        }

        protected override async Task<(MediatR.Unit, string)?> GetUserAsync(string userName)
        {
            var uname = await _cache.GetByUserNameAsync(
                userName,
                async uid => (await _userManager.FindByNameAsync(uid))?.UserName);

            return uname == null ? default((MediatR.Unit, string)?) : (MediatR.Unit.Value, uname);
        }
    }
}
