#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    public abstract class UserInformationProviderBase<T> : IUserInformationProvider
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        protected UserInformationProviderBase(
            IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        protected virtual ValueTask ProduceAsync(
            TagBuilder tag,
            T evermore,
            string? username,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext)
        {
            var url = _urlHelperFactory.GetUrlHelper(actionContext);

            tag.MergeAttribute("href", url.RouteUrl("AccountProfile", new { username }));
            tag.InnerHtml.AppendHtml(username);
            return default;
        }

        protected virtual ValueTask ProduceAsync(
            TagBuilder tag,
            (T Evermore, string UserName)? information,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext)
        {
            if (!information.HasValue)
            {
                tag.InnerHtml.AppendHtml("UNKNOWN USER");
                return default;
            }
            else
            {
                var (e, u) = information.Value;
                return ProduceAsync(tag, e, u, attach, actionContext);
            }
        }

        protected abstract ValueTask<(T, string)?> GetUserAsync(
            int userId,
            string? userName,
            IReadOnlyDictionary<string, string> attach);

        protected abstract ValueTask<(T, string)?> GetUserAsync(
            string userName,
            IReadOnlyDictionary<string, string> attach);

        public virtual async ValueTask<TagBuilder> ProcessAsync(
            int userId,
            string? userName,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext)
        {
            var user = await GetUserAsync(userId, userName, attach);
            var tag = new TagBuilder("a");
            await ProduceAsync(tag, user, attach, actionContext);
            return tag;
        }

        public virtual async ValueTask<TagBuilder> ProcessAsync(
            string userName,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext)
        {
            var user = await GetUserAsync(userName, attach);
            var tag = new TagBuilder("a");
            await ProduceAsync(tag, user, attach, actionContext);
            return tag;
        }
    }

    public interface IUserInformationCache<T>
    {
        ValueTask<T> GetByUserNameAsync(string userName, Func<string, Task<T>> valueFactory);

        ValueTask<T> GetByUserIdAsync(int userId, Func<int, Task<T>> valueFactory);
    }

    public class MemoryUserInformationCache<T> : MemoryCache, IUserInformationCache<T>
    {
        private readonly TimeSpan _expireSpan = TimeSpan.FromMinutes(5);

        public MemoryUserInformationCache()
            : base(new MemoryCacheOptions
            {
                Clock = new Microsoft.Extensions.Internal.SystemClock()
            })
        {
        }

        public ValueTask<T> GetByUserNameAsync(string userName, Func<string, Task<T>> valueFactory)
        {
            return this.GetOrCreateAsync("UserName: " + userName, async entry =>
            {
                var value = await valueFactory(userName);
                entry.AbsoluteExpirationRelativeToNow = _expireSpan;
                return value;
            });
        }

        public ValueTask<T> GetByUserIdAsync(int userId, Func<int, Task<T>> valueFactory)
        {
            return GetOrCreateAsync("UserId: " + userId, async entry =>
            {
                var value = await valueFactory(userId);
                entry.AbsoluteExpirationRelativeToNow = _expireSpan;
                return value;
            });
        }

        public async ValueTask<T> GetOrCreateAsync(object key, Func<ICacheEntry, Task<T>> factory)
        {
            if (!TryGetValue(key, out object result))
            {
                using ICacheEntry entry = CreateEntry(key);

                result = (await factory(entry).ConfigureAwait(false))!;
                entry.Value = result;
            }

            return (T)result;
        }
    }

    public class DefaultUserInformationProvider : UserInformationProviderBase<MediatR.Unit>
    {
        private readonly IUserInformationCache<string?> _cache;
        private readonly IUserManager _userManager;

        public DefaultUserInformationProvider(
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
            userName ??= await _cache.GetByUserIdAsync(
                userId,
                async uid => (await _userManager.FindByIdAsync(uid))?.UserName);

            return userName == null ? default((MediatR.Unit, string)?) : (MediatR.Unit.Value, userName);
        }

        protected override ValueTask<(MediatR.Unit, string)?> GetUserAsync(string userName, IReadOnlyDictionary<string, string> attach)
        {
            return new ValueTask<(MediatR.Unit, string)?>((MediatR.Unit.Value, userName));
        }
    }
}
