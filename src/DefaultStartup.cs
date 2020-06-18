using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Default startup class to configure the services and request pipeline.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The modules to configure
        /// </summary>
        internal static ICollection<AbstractModule> Modules { get; } = new List<AbstractModule>();

        /// <summary>
        /// Create an instance of <see cref="Startup"/>
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/></param>
        /// <param name="env">The <see cref="IWebHostEnvironment"/></param>
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        /// <summary>
        /// The configuration of running application
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The environment with web host specified settings
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The dependency injection builder</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddApplicationInsightsTelemetry();

            services.AddMediatR(Modules.Select(a => a.GetType().Assembly).ToArray());


            /*
            services.AddDbContext<AppDbContext>(options => options
                .UseSqlServer(Configuration.GetConnectionString("UserDbConnection"))
                .UseBulkExtensions());
            */

            //services.AddScoped<IAuditlogger, Auditlogger>();


            /*
            services.AddIdentity<User, Role>(
                options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredUniqueChars = 2;

                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 10;
                    options.Lockout.AllowedForNewUsers = true;

                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.@";

                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddUserManager<UserManager>()
                .AddSignInManager<SignInManager>()
                .AddDefaultTokenProviders()
                .RegisterOtherStores()
                .UseClaimsPrincipalFactory<UserWithNickNameClaimsPrincipalFactory<AppDbContext>, User>();

            services.AddAuthentication()
                .SetCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.LoginPath = "/account/login";
                    options.LogoutPath = "/account/logout";
                    options.AccessDeniedPath = "/account/access-denied";
                    options.SlidingExpiration = true;
                    options.Events = new CookieAuthenticationValidator();
                })
                .AddBasic(options =>
                {
                    options.Realm = "JudgeWeb";
                    options.AllowInsecureProtocol = true;
                    options.Events = new BasicAuthenticationValidator<User, Role, int, AppDbContext>();
                });

            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("EmailVerified", b => b.RequireClaim("email_verified", "true"));
                });

            if (Configuration["IdentityServer:Enabled"] == "True")
                services.AddIdentityServer()
                    .AddAspNetIdentity<User>()
                    .AddInMemoryClients(Configuration.GetSection("IdentityServer:Clients"))
                    .AddInMemoryIdentityResources(Configuration.GetSection("IdentityServer:Scopes"))
                    .AddInMemoryApiResources(Configuration.GetSection("IdentityServer:Apis"))
                    .AddDeveloperSigningCredential();
            */

            services.AddSingleton(
                HtmlEncoder.Create(
                    UnicodeRanges.BasicLatin,
                    UnicodeRanges.CjkUnifiedIdeographs));

            Modules.ApplyServices(services);
            var (parts, razors) = Modules.GetParts();

            services.AddControllersWithViews()
                .AddTimeSpanJsonConverter()
                .UseSlugifyParameterTransformer()
                .ReplaceDefaultLinkGenerator()
                .AddSessionStateTempDataProvider()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .ConfigureApplicationPartManager(apm => parts.ForEach(a => apm.ApplicationParts.Add(a)));

            if (Environment.IsDevelopment())
                services.AddControllersWithViews()
                    .AddRazorRuntimeCompilation(options =>
                        options.FileProviders.Add(razors));

            services.AddSession(options => options.Cookie.IsEssential = true);

            /*
            services.AddApiExplorer()
                .AddHtmlTemplate(Environment.WebRootFileProvider.GetFileInfo("static/nelmioapidoc/index.html.src"))
                .AddDocument("DOMjudge", "DOMjudge compact API v4", "7.2.0")
                .AddSecurityScheme("basic", Microsoft.OpenApi.Models.SecuritySchemeType.Http)
                .IncludeXmlComments(EnabledAreas.Select(item => System.IO.Path.Combine(AppContext.BaseDirectory, $"{AssemblyPrefix}{item}.xml")))
                .IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, "JudgeWeb.Domains.Contests.CcsApi.xml"))
                .FilterByRouteArea();
            */
        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder</param>
        public void Configure(IApplicationBuilder app)
        {
            app.EnsureClaimTypes();

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMiddleware<AjaxExceptionMiddleware>();
                //app.UseDatabaseErrorPage();
                app.UseStatusCodePage();
            }
            else if (Environment.EnvironmentName == "Test")
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

                app.UseDeveloperExceptionPage();
                app.UseMiddleware<AjaxExceptionMiddleware>();
                //app.UseDatabaseErrorPage();
                app.UseStatusCodePage();
            }
            else
            {
                //app.UseMiddleware<RealIpMiddleware>();
                app.UseHttpsRedirection();
                app.UseExceptionHandler("/error");
                app.UseStatusCodePage();
                app.UseCatchException();
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();

            //if (Configuration["IdentityServer:Enabled"] == "True")
            //    app.UseIdentityServer();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                Modules.ApplyEndpoints(endpoints);
                //endpoints.MapControllers();

                //endpoints.MapSwaggerUI("/api/doc")
                //    .RequireRoles("Administrator,Problem");

                endpoints.MapFallbackNotFound("/api/{**slug}");

                endpoints.MapFallbackNotFound("/lib/{**slug}");

                //endpoints.MapFallbackToAreaController(
                //    pattern: "/dashboard/{**slug}",
                //    "NotFound2", "Root", "Dashboard");
            });
        }
    }
}
