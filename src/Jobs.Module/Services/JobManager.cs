﻿using Jobs.Entities;
using Jobs.Models;
using Jobs.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SatelliteSite.JobsModule.Services
{
    public partial class RelationalJobStorage<TContext> : IJobManager
        where TContext : DbContext
    {
        private Expression<Func<Job, JobEntry>> GetSelector()
        {
            return j => new JobEntry
            {
                Arguments = j.Arguments,
                CompleteTime = j.CompleteTime,
                Composite = j.Composite,
                SuggestedFileName = j.SuggestedFileName,
                Status = j.Status,
                CreationTime = j.CreationTime,
                JobId = j.JobId,
                JobType = j.JobType,
                ParentJobId = j.ParentJobId,
                OwnerId = j.OwnerId,
            };
        }

        public Task<IPagedList<JobEntry>> GetJobsAsync(int ownerId, int page = 1, int count = 20)
        {
            return _dbContext.Set<Job>()
                .Where(j => j.OwnerId == ownerId && j.Composite)
                .OrderByDescending(j => j.JobId)
                .Select(GetSelector())
                .ToPagedListAsync(page, count);
        }

        public async Task<JobEntry> FindJobAsync(Guid id, int? ownerId = null)
        {
            var e = await _dbContext.Set<Job>()
                .Where(j => j.JobId == id && j.OwnerId == ownerId)
                .Select(GetSelector())
                .SingleOrDefaultAsync();

            if (e.Composite)
                e.Children = await _dbContext.Set<Job>()
                    .Where(j => j.ParentJobId == id && j.OwnerId == ownerId)
                    .Select(GetSelector())
                    .ToListAsync();

            return e;
        }

        public Task<IFileInfo> GetLogsAsync(Guid guid)
        {
            return _fileProvider.GetFileInfoAsync(guid + "/log");
        }

        public Task<IFileInfo> GetDownloadAsync(Guid guid)
        {
            return _fileProvider.GetFileInfoAsync(guid + "/main");
        }
    }
}
