#nullable enable
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Collections.Generic;
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

        protected override async ValueTask<(MediatR.Unit, string)?> GetUserAsync(int userId, string? userName, IReadOnlyDictionary<string, string> attach)
        {
            userName = await _cache.GetByUserIdAsync(
                userId,
                async uid => (await _userManager.FindByIdAsync(uid))?.UserName);

            return userName == null ? default((MediatR.Unit, string)?) : (MediatR.Unit.Value, userName);
        }

        protected override async ValueTask<(MediatR.Unit, string)?> GetUserAsync(string userName, IReadOnlyDictionary<string, string> attach)
        {
            var uname = await _cache.GetByUserNameAsync(
                userName,
                async uid => (await _userManager.FindByNameAsync(uid))?.UserName);

            return uname == null ? default((MediatR.Unit, string)?) : (MediatR.Unit.Value, uname);
        }

        protected override async ValueTask ProduceAsync(TagBuilder tag, Unit evermore, string? username, IReadOnlyDictionary<string, string> attach, ViewContext actionContext)
        {
            await base.ProduceAsync(tag, evermore, username, attach, actionContext);
            tag.Attributes.TryAdd("data-type", "2");
        }

        protected override async ValueTask ProduceAsync(TagBuilder tag, (Unit Evermore, string UserName)? information, IReadOnlyDictionary<string, string> attach, ViewContext actionContext)
        {
            await base.ProduceAsync(tag, information, attach, actionContext);
            tag.Attributes.TryAdd("data-type", "3");
        }
    }
}
