using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.Tests
{
    /// <summary>
    /// Simple <see cref="IHostBuilder"/> for testing only database related.
    /// </summary>
    internal class SimpleHostBuilder : IHostBuilder, IHost
    {
        private readonly Dictionary<object, object> properties = new();
        private ServiceCollection serviceCollection = new();
        private ServiceProvider serviceProvider;

        public IDictionary<object, object> Properties => properties;

        IServiceProvider IHost.Services => serviceProvider ?? throw new InvalidOperationException();

        public IHostBuilder ConfigureServices(
            Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            configureDelegate.Invoke(null, serviceCollection);
            return this;
        }

        public IHost Build()
        {
            this.serviceProvider = serviceCollection.BuildServiceProvider();
            this.serviceCollection = null;
            return this;
        }

        public IHostBuilder ConfigureAppConfiguration(
            Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
            => throw new NotSupportedException();

        public IHostBuilder ConfigureContainer<TContainerBuilder>(
            Action<HostBuilderContext, TContainerBuilder> configureDelegate)
            => throw new NotSupportedException();

        public IHostBuilder ConfigureHostConfiguration(
            Action<IConfigurationBuilder> configureDelegate)
            => throw new NotSupportedException();

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
            IServiceProviderFactory<TContainerBuilder> factory)
            => throw new NotSupportedException();

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
            Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
            => throw new NotSupportedException();

        void IDisposable.Dispose()
            => this.serviceProvider.Dispose();

        Task IHost.StartAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();

        Task IHost.StopAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
