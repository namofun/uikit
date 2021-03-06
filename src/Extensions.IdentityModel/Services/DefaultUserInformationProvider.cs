﻿#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
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

        protected virtual Task ProduceAsync(
            T evermore,
            string username,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext,
            TagHelperContext context,
            TagHelperOutput output)
        {
            var url = _urlHelperFactory.GetUrlHelper(actionContext);

            output.TagName = "a";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("href", url.RouteUrl("AccountProfile", new { username }));
            output.Content.SetContent(username);
            return Task.CompletedTask;
        }

        protected virtual Task ProduceAsync(
            (T Evermore, string UserName)? information,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext,
            TagHelperContext context,
            TagHelperOutput output)
        {
            if (!information.HasValue)
            {
                output.TagName = "a";
                output.TagMode = TagMode.StartTagAndEndTag;
                output.Content.SetContent("UNKNOWN USER");
                return Task.CompletedTask;
            }
            else
            {
                var (e, u) = information.Value;
                return ProduceAsync(e, u, attach, actionContext, context, output);
            }
        }

        protected abstract Task<(T, string)?> GetUserAsync(int userId, string? userName);

        protected abstract Task<(T, string)?> GetUserAsync(string userName);

        public async Task ProcessAsync(
            int userId,
            string? userName,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext,
            TagHelperContext context,
            TagHelperOutput output)
        {
            var t = await GetUserAsync(userId, userName);
            await ProduceAsync(t, attach, actionContext, context, output);
        }

        public async Task ProcessAsync(
            string userName,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext,
            TagHelperContext context,
            TagHelperOutput output)
        {
            var t = await GetUserAsync(userName);
            await ProduceAsync(t, attach, actionContext, context, output);
        }
    }

    public interface IUserInformationCache<T>
    {
        Task<T> GetByUserNameAsync(string userName, Func<string, Task<T>> valueFactory);

        Task<T> GetByUserIdAsync(int userId, Func<int, Task<T>> valueFactory);
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

        public Task<T> GetByUserNameAsync(string userName, Func<string, Task<T>> valueFactory)
        {
            return this.GetOrCreateAsync("UserName: " + userName, async entry =>
            {
                var value = await valueFactory(userName);
                entry.AbsoluteExpirationRelativeToNow = _expireSpan;
                return value;
            });
        }

        public Task<T> GetByUserIdAsync(int userId, Func<int, Task<T>> valueFactory)
        {
            return this.GetOrCreateAsync("UserId: " + userId, async entry =>
            {
                var value = await valueFactory(userId);
                entry.AbsoluteExpirationRelativeToNow = _expireSpan;
                return value;
            });
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

        protected override async Task<(MediatR.Unit, string)?> GetUserAsync(int userId, string? userName)
        {
            userName ??= await _cache.GetByUserIdAsync(
                userId,
                async uid => (await _userManager.FindByIdAsync(uid))?.UserName);

            return userName == null ? default((MediatR.Unit, string)?) : (MediatR.Unit.Value, userName);
        }

        protected override Task<(MediatR.Unit, string)?> GetUserAsync(string userName)
        {
            return Task.FromResult<(MediatR.Unit, string)?>((MediatR.Unit.Value, userName));
        }
    }
}
