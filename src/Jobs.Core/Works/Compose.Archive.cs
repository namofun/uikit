﻿using Jobs.Entities;
using Jobs.Models;
using Jobs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Jobs.Works
{
    public class ComposeArchive : IJobExecutorProvider
    {
        public string Type => "Compose.Archive";

        public IJobExecutor Create(IServiceProvider serviceProvider)
        {
            return new Executor(
                serviceProvider.GetRequiredService<IJobFileProvider>(),
                serviceProvider.GetRequiredService<IJobManager>());
        }

        public static JobDescription ForChildren(
            int ownerId,
            string fileName,
            List<JobDescription> children)
        {
            if (children.Select(j => j.SuggestedFileName).Where(j => j != null).Distinct().Count() < children.Count)
                throw new InvalidOperationException(
                    "For zip archives, two sub jobs cannot share the same file name.");

            return new JobDescription
            {
                JobType = "Compose.Archive",
                SuggestedFileName = fileName,
                Children = children,
                OwnerId = ownerId,
                Arguments = children.Select(j => j.SuggestedFileName).ToJson(),
            };
        }

        private class Executor : IJobExecutor
        {
            private readonly IJobFileProvider _fileProvider;
            private readonly IJobManager _manager;

            public Executor(IJobFileProvider fileProvider, IJobManager manager)
            {
                _fileProvider = fileProvider;
                _manager = manager;
            }

            public async Task<JobStatus> ExecuteAsync(string arguments, Guid guid, ILogger logger)
            {
                try
                {
                    var children = await _manager.GetChildrenAsync(guid);
                    var toAdd = new List<(string, IFileInfo)>();

                    foreach (var j in children)
                    {
                        if (j.Status != JobStatus.Finished || j.SuggestedFileName == null)
                        {
                            logger.LogError("The job {id} is not finished.", j.JobId);
                            return JobStatus.Failed;
                        }

                        var file = await _fileProvider.GetFileInfoAsync(j.JobId + "/main");
                        if (file == null || !file.Exists || file.PhysicalPath == null)
                        {
                            logger.LogError("The file job {id} is not found.", j.JobId);
                            return JobStatus.Failed;
                        }

                        toAdd.Add((j.SuggestedFileName, file));
                    }

                    var tmpFileName = Path.GetTempFileName();
                    logger.LogInformation("Use tmpfile {fileName}", tmpFileName);

                    using (var zipArchive = new ZipArchive(File.OpenWrite(tmpFileName), ZipArchiveMode.Create, false))
                    {
                        foreach (var item in toAdd)
                        {
                            logger.LogInformation("Add {fileName} from {physical}...", item.Item1, item.Item2.PhysicalPath);
                            zipArchive.CreateEntryFromFile(item.Item2.PhysicalPath, item.Item1);
                        }
                    }

                    using (var tmpFile = File.OpenRead(tmpFileName))
                    {
                        await _fileProvider.WriteStreamAsync(guid + "/main", tmpFile);
                    }

                    File.Delete(tmpFileName);
                    logger.LogInformation("Delete tmpfile {fileName}", tmpFileName);
                    return JobStatus.Finished;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unknown exception.");
                    return JobStatus.Failed;
                }
            }
        }
    }
}
