using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.AzureBlob;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;

namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// Options class for setting up Azure Storage Blob as a web root file provider.
    /// </summary>
    public class AzureBlobWwwrootOptions
    {
        /// <summary>
        /// Gets or sets whether automatically caches the files being requested.
        /// </summary>
        public bool AutoCache { get; set; } = true;

        /// <summary>
        /// Gets or sets allowed folders for access.
        /// </summary>
        public string[]? AllowedFolders { get; set; }

        /// <summary>
        /// Gets or sets the access tier uploaded by this file provider.
        /// </summary>
        public AccessTier? AccessTier { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the local cache path.
        /// </summary>
        public string LocalCachePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the blob service client.
        /// </summary>
        public BlobServiceClient? BlobServiceClient { get; set; }

        /// <summary>
        /// Gets or sets the name of blob container.
        /// </summary>
        public string? BlobContainerName { get; set; }

        /// <summary>
        /// Gets or sets the blob container client.
        /// </summary>
        public BlobContainerClient? BlobContainerClient { get; set; }

        /// <summary>
        /// Configure the options with the parameters.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="containerName">Container name.</param>
        /// <param name="localFileCachePath">Path of file local cache.</param>
        /// <param name="allowedRanges">Allowed ranges for provider to proceed.</param>
        /// <returns>The options to chain calls.</returns>
        public AzureBlobWwwrootOptions With(
            string connectionString,
            string containerName,
            string localFileCachePath,
            string[]? allowedRanges = null)
        {
            ConnectionString = connectionString;
            BlobContainerName = containerName;
            LocalCachePath = localFileCachePath;
            AllowedFolders = allowedRanges;
            return this;
        }

        internal BlobContainerClient GetContainer()
        {
            if (BlobContainerClient != null)
            {
                return BlobContainerClient;
            }

            if (BlobServiceClient == null
                && string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new ArgumentNullException(
                    nameof(BlobServiceClient),
                    "Please specify BlobServiceClient or ConnectionString in AzureBlobWwwrootOptions!");
            }

            if (string.IsNullOrWhiteSpace(BlobContainerName))
            {
                throw new ArgumentNullException(
                    nameof(BlobContainerName),
                    "Please specify BlobContainerName in AzureBlobWwwrootOptions!");
            }

            var service = BlobServiceClient ?? new(ConnectionString);
            return service.GetBlobContainerClient(BlobContainerName);
        }

        internal string EnsureLocalCachePathCreated()
        {
            if (!Directory.Exists(LocalCachePath))
            {
                Directory.CreateDirectory(LocalCachePath);
            }

            return LocalCachePath;
        }
    }

    public class AzureBlobWwwrootProvider : AzureBlobProvider, IWwwrootFileProvider
    {
        public AzureBlobWwwrootProvider(IOptions<AzureBlobWwwrootOptions> options)
            : base(
                  options.Value.GetContainer(),
                  options.Value.EnsureLocalCachePathCreated(),
                  options.Value.AccessTier,
                  options.Value.AutoCache,
                  options.Value.AllowedFolders)
        {
        }
    }

    internal class ConfigureWebEnvironmentForAzureBlob : IConfigureOptions<SubstrateOptions>
    {
        private readonly IWebHostEnvironment _environment;
        private readonly AzureBlobWwwrootProvider _wwwrootProvider;

        public ConfigureWebEnvironmentForAzureBlob(
            IWebHostEnvironment hostEnvironment,
            AzureBlobWwwrootProvider wwwrootProvider)
        {
            _environment = hostEnvironment;
            _wwwrootProvider = wwwrootProvider;
        }

        public void Configure(SubstrateOptions options)
        {
            IFileProvider wwwroot = _environment.WebRootFileProvider;

            if (wwwroot is NullFileProvider)
            {
                _environment.WebRootFileProvider = _wwwrootProvider;
            }
            else
            {
                if (wwwroot is CompositeFileProvider composite)
                {
                    composite = new(composite.FileProviders.Append(_wwwrootProvider));
                }
                else
                {
                    composite = new(wwwroot, _wwwrootProvider);
                }

                _environment.WebRootFileProvider = composite;
            }
        }
    }

    public static class AzureBlobWwwrootExtensions
    {
        /// <summary>
        /// Adds Azure Storage Blob as wwwroot provider.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="configureOptions">The configure options action.</param>
        /// <returns>The host builder to chain configure calls.</returns>
        public static IHostBuilder AddAzureBlobWebRoot(
            this IHostBuilder hostBuilder,
            Action<HostBuilderContext, AzureBlobWwwrootOptions> configureOptions)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<AzureBlobWwwrootProvider>();
                services.AddSingletonUpcast<IWwwrootFileProvider, AzureBlobWwwrootProvider>();
                services.ConfigureOptions<ConfigureWebEnvironmentForAzureBlob>();
                services.Configure<AzureBlobWwwrootOptions>(options => configureOptions(context, options));
            });
        }

        /// <summary>
        /// Adds Azure Storage Blob as wwwroot provider.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="containerName">Container name.</param>
        /// <param name="localFileCachePath">Path of file local cache.</param>
        /// <param name="allowedRanges">Allowed ranges for provider to proceed.</param>
        /// <returns>The host builder to chain configure calls.</returns>
        public static IHostBuilder AddAzureBlobWebRoot(
            this IHostBuilder hostBuilder,
            string connectionString,
            string containerName,
            string localFileCachePath,
            string[]? allowedRanges = null)
        {
            return AddAzureBlobWebRoot(hostBuilder, (context, options) =>
            {
                options.ConnectionString = connectionString;
                options.BlobContainerName = containerName;
                options.LocalCachePath = localFileCachePath;
                options.AllowedFolders = allowedRanges;
            });
        }
    }
}
