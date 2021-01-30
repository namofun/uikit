using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    /// <summary>
    /// Provides data from cookie or session state to the current <see cref="ITempDataDictionary"/> object.
    /// </summary>
    public class CompositeTempDataProvider : ITempDataProvider
    {
        private static readonly string TempDataSessionStateKey = "__ControllerTempData";
        private static readonly string Purpose = "Microsoft.AspNetCore.Mvc.CookieTempDataProviderToken.v1";

        private readonly IDataProtector _dataProtector;
        private readonly TempDataSerializer _tempDataSerializer;
        private readonly ChunkingCookieManager _chunkingCookieManager;
        private readonly CookieTempDataProviderOptions _options;
        private readonly ILogger _logger;
        private readonly string _cookieName;
        private readonly Action<ILogger, string, Exception> _tempDataCookieLoadFailure;

        public CompositeTempDataProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<CookieTempDataProviderOptions> options,
            TempDataSerializer tempDataSerializer,
            ILoggerFactory loggerFactory)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(Purpose);
            _tempDataSerializer = tempDataSerializer;
            _chunkingCookieManager = new ChunkingCookieManager();
            _logger = loggerFactory.CreateLogger<CookieTempDataProvider>();
            _options = options.Value;
            _cookieName = options.Value.Cookie.Name;

            _tempDataCookieLoadFailure = LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(3, "TempDataCookieLoadFailure"),
                "The temp data cookie {CookieName} could not be loaded.");
        }

        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var session = LoadSessionTempData(context);
            var cookie = LoadCookieTempData(context);

            if (session != null && cookie != null)
            {
                foreach (var (key, value) in cookie)
                {
                    session.TryAdd(key, value);
                }
            }

            return session ?? cookie ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        private IDictionary<string, object>? LoadSessionTempData(HttpContext context)
        {
            // Accessing Session property will throw if the session middleware is not enabled.
            var session = context.Session;

            if (session.TryGetValue(TempDataSessionStateKey, out var value))
            {
                // If we got it from Session, remove it so that no other request gets it
                session.Remove(TempDataSessionStateKey);

                return _tempDataSerializer.Deserialize(value);
            }

            return null;
        }

        private IDictionary<string, object>? LoadCookieTempData(HttpContext context)
        {
            if (context.Request.Cookies.ContainsKey(_cookieName))
            {
                // The cookie we use for temp data is user input, and might be invalid in many ways.
                //
                // Since TempData is a best-effort system, we don't want to throw and get a 500 if the cookie is
                // bad, we will just clear it and ignore the exception. The common case that we've identified for
                // this is misconfigured data protection settings, which can cause the key used to create the 
                // cookie to no longer be available.
                try
                {
                    var encodedValue = _chunkingCookieManager.GetRequestCookie(context, _cookieName);
                    if (!string.IsNullOrEmpty(encodedValue))
                    {
                        var protectedData = WebEncoders.Base64UrlDecode(encodedValue);
                        var unprotectedData = _dataProtector.Unprotect(protectedData);
                        var tempData = _tempDataSerializer.Deserialize(unprotectedData);
                        return tempData;
                    }
                }
                catch (Exception ex)
                {
                    _tempDataCookieLoadFailure(_logger, _cookieName, ex);

                    // If we've failed, we want to try and clear the cookie so that this won't keep happening
                    // over and over.
                    if (!context.Response.HasStarted)
                    {
                        _chunkingCookieManager.DeleteCookie(context, _cookieName, _options.Cookie.Build(context));
                    }
                }
            }

            return null;
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Accessing Session property will throw if the session middleware is not enabled.
            var session = context.Session;
            var cookieOptions = _options.Cookie.Build(context);
            SetCookiePath(context, cookieOptions);

            var hasValues = (values != null && values.Count > 0);
            if (hasValues)
            {
                var bytes = _tempDataSerializer.Serialize(values);
                if (bytes.Length > 4096)
                {
                    session.Set(TempDataSessionStateKey, bytes);
                }
                else
                {
                    bytes = _dataProtector.Protect(bytes);
                    var encodedValue = WebEncoders.Base64UrlEncode(bytes);
                    _chunkingCookieManager.AppendResponseCookie(context, _cookieName, encodedValue, cookieOptions);
                }
            }
            else
            {
                session.Remove(TempDataSessionStateKey);
                _chunkingCookieManager.DeleteCookie(context, _cookieName, cookieOptions);
            }
        }

        private void SetCookiePath(HttpContext httpContext, CookieOptions cookieOptions)
        {
            if (!string.IsNullOrEmpty(_options.Cookie.Path))
            {
                cookieOptions.Path = _options.Cookie.Path;
            }
            else
            {
                var pathBase = httpContext.Request.PathBase.ToString();
                if (!string.IsNullOrEmpty(pathBase))
                {
                    cookieOptions.Path = pathBase;
                }
            }
        }
    }
}
