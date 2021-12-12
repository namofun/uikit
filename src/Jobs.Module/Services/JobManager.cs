using Jobs.Entities;
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
        private Expression<Func<Job, JobEntry>> GetSelector(bool useArgs = false)
        {
            if (useArgs)
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
            else
            {
                return j => new JobEntry
                {
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
        }

        public Task<IPagedList<JobEntry>> GetJobsAsync(int? ownerId, int page = 1, int count = 20)
        {
            return _dbContext.Set<Job>()
                .WhereIf(ownerId.HasValue, j => j.OwnerId == ownerId)
                .Where(j => j.ParentJobId == null)
                .OrderByDescending(j => j.JobId)
                .Select(GetSelector())
                .ToPagedListAsync(page, count);
        }

        public async Task<JobEntry> FindJobAsync(Guid id, int? ownerId = null)
        {
            var e = await _dbContext.Set<Job>()
                .Where(j => j.JobId == id)
                .WhereIf(ownerId.HasValue, j => j.OwnerId == ownerId)
                .Select(GetSelector(true))
                .SingleOrDefaultAsync();

            if (e == null) return null;

            if (e.Composite)
                e.Children = await GetChildrenAsync(id);

            return e;
        }

        public Task<string> GetLogsAsync(Job jobEntry)
        {
            return _fileProvider.GetLogsAsync(jobEntry);
        }

        public Task<IFileInfo> GetDownloadAsync(Job jobEntry)
        {
            return _fileProvider.GetOutputAsync(jobEntry);
        }

        public Task<List<JobEntry>> GetChildrenAsync(Guid id, int? ownerId = null)
        {
            return _dbContext.Set<Job>()
                .Where(j => j.ParentJobId == id)
                .WhereIf(ownerId.HasValue, j => j.OwnerId == ownerId)
                .Select(GetSelector())
                .ToListAsync();
        }
    }
}
