using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace SatelliteSite.Tests
{
    /// <summary>
    /// <para>
    /// Factory for bootstrapping an application in memory for functional end to end tests.
    /// </para>
    /// <para>
    /// Instance of <see cref="SubstrateApplicationBase"/> can be used to
    /// create a <see cref="TestServer"/> instance using the MVC application
    /// defined by <see cref="CreateHostBuilder"/> and one or more <see cref="HttpClient"/>
    /// instances used to send <see cref="HttpRequestMessage"/> to the <see cref="TestServer"/>.
    /// </para>
    /// </summary>
    public abstract class SubstrateApplicationBase : IDisposable
    {
        private bool _disposed;
        private TestServer? _server;
        private IHost? _host;
        private readonly IList<HttpClient> _clients = new List<HttpClient>();

        /// <summary>
        /// Create the host builder.
        /// </summary>
        /// <returns>The host builder.</returns>
        protected abstract IHostBuilder CreateHostBuilder();

        /// <summary>
        /// Prepare the host.
        /// </summary>
        /// <param name="host">The host.</param>
        protected virtual void PrepareHost(IHost host)
        {
        }

        /// <summary>
        /// Cleanup the host.
        /// </summary>
        /// <param name="host">The host.</param>
        protected virtual void CleanupHost(IHost host)
        {
        }

        /// <summary>
        /// Gets the <see cref="TestServer"/> created by this <see cref="SubstrateApplicationBase"/>.
        /// </summary>
        public TestServer Server
        {
            get
            {
                EnsureServer();
                return _server!;
            }
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> created by the server associated with this <see cref="SubstrateApplicationBase"/>.
        /// </summary>
        public virtual IServiceProvider Services
        {
            get
            {
                EnsureServer();
                return _host?.Services ?? _server!.Host.Services;
            }
        }

        /// <summary>
        /// Gets the entry point assembly.
        /// </summary>
        protected abstract Assembly EntryPointAssembly { get; }

        /// <summary>
        /// Gets the <see cref="WebApplicationFactoryClientOptions"/> used by <see cref="CreateClient()"/>.
        /// </summary>
        public WebApplicationFactoryClientOptions ClientOptions { get; private set; } = new WebApplicationFactoryClientOptions();

        /// <summary>
        /// Ensure the server is built.
        /// </summary>
        private void EnsureServer()
        {
            if (_server != null)
            {
                return;
            }

            EnsureDepsFile();

            var localDebugPath = EntryPointAssembly
                .GetCustomAttribute<LocalDebugPathAttribute>();
            if (localDebugPath == null)
                throw new InvalidOperationException("Invalid assembly information.");
            if (!Directory.Exists(localDebugPath.Path))
                throw new InvalidOperationException($"Invalid assembly debug path \"{localDebugPath.Path}\".");

            _host = CreateHostBuilder()
                .UseEnvironment("Testing")
                .ConfigureWebHost(builder =>
                {
                    builder.UseContentRoot(localDebugPath.Path);
                    builder.UseTestServer();
                })
                .Build();

            PrepareHost(_host);
            _host.Start();
            _server = (TestServer)_host.Services.GetRequiredService<IServer>();
        }

        /// <summary>
        /// Ensure the deps file is present.
        /// </summary>
        private void EnsureDepsFile()
        {
            if (EntryPointAssembly?.EntryPoint == null)
                throw new InvalidOperationException("Invalid assembly entry point.");
            var depsFileName = $"{EntryPointAssembly.GetName().Name}.deps.json";
            var depsFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, depsFileName));
            if (!depsFile.Exists)
                throw new InvalidOperationException($"Missing deps file {depsFile.FullName}. Please make sure your test project made package-reference to Microsoft.AspNetCore.Mvc.Testing.");
        }

        /// <summary>
        /// Creates an instance of <see cref="HttpClient"/> that automatically follows
        /// redirects and handles cookies.
        /// </summary>
        /// <returns>The <see cref="HttpClient"/>.</returns>
        public HttpClient CreateClient() =>
            CreateClient(ClientOptions);

        /// <summary>
        /// Creates an instance of <see cref="HttpClient"/> that automatically follows
        /// redirects and handles cookies.
        /// </summary>
        /// <returns>The <see cref="HttpClient"/>.</returns>
        public HttpClient CreateClient(Action<WebApplicationFactoryClientOptions> optionsAction)
        {
            var options = new WebApplicationFactoryClientOptions();
            optionsAction?.Invoke(options);
            return CreateClient(options);
        }

        /// <summary>
        /// Creates an instance of <see cref="HttpClient"/> that automatically follows
        /// redirects and handles cookies.
        /// </summary>
        /// <returns>The <see cref="HttpClient"/>.</returns>
        public HttpClient CreateClient(WebApplicationFactoryClientOptions options) =>
            CreateDefaultClient(options.BaseAddress, options.CreateHandlers());

        /// <summary>
        /// Creates a new instance of an <see cref="HttpClient"/> that can be used to
        /// send <see cref="HttpRequestMessage"/> to the server. The base address of the <see cref="HttpClient"/>
        /// instance will be set to <c>http://localhost</c>.
        /// </summary>
        /// <param name="handlers">A list of <see cref="DelegatingHandler"/> instances to set up on the
        /// <see cref="HttpClient"/>.</param>
        /// <returns>The <see cref="HttpClient"/>.</returns>
        public HttpClient CreateDefaultClient(params DelegatingHandler[] handlers)
        {
            EnsureServer();

            HttpClient client;
            if (handlers == null || handlers.Length == 0)
            {
                client = _server!.CreateClient();
            }
            else
            {
                for (var i = handlers.Length - 1; i > 0; i--)
                {
                    handlers[i - 1].InnerHandler = handlers[i];
                }

                var serverHandler = _server!.CreateHandler();
                handlers[^1].InnerHandler = serverHandler;

                client = new HttpClient(handlers[0]);
            }

            _clients.Add(client);

            ConfigureClient(client);

            return client;
        }

        /// <summary>
        /// Configures <see cref="HttpClient"/> instances created by this <see cref="WebApplicationFactory{TEntryPoint}"/>.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> instance getting configured.</param>
        protected virtual void ConfigureClient(HttpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            client.BaseAddress = new Uri("http://localhost");
        }

        /// <summary>
        /// Creates a new instance of an <see cref="HttpClient"/> that can be used to
        /// send <see cref="HttpRequestMessage"/> to the server.
        /// </summary>
        /// <param name="baseAddress">The base address of the <see cref="HttpClient"/> instance.</param>
        /// <param name="handlers">A list of <see cref="DelegatingHandler"/> instances to set up on the
        /// <see cref="HttpClient"/>.</param>
        /// <returns>The <see cref="HttpClient"/>.</returns>
        public HttpClient CreateDefaultClient(Uri baseAddress, params DelegatingHandler[] handlers)
        {
            var client = CreateDefaultClient(handlers);
            client.BaseAddress = baseAddress;

            return client;
        }

        /// <summary>
        /// Process the dispose action.
        /// </summary>
        /// <param name="disposing">Whether we are in Dispose() or Finalize().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var client in _clients)
                    {
                        client.Dispose();
                    }

                    _server?.Dispose();
                    if (_host != null) CleanupHost(_host);
                    _host?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <inheritdoc />
        ~SubstrateApplicationBase()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
