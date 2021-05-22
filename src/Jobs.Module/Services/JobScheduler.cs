using Jobs.Entities;
using Jobs.Models;
using Jobs.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.JobsModule.Services
{
    public class RelationalJobScheduler<TContext> : IJobScheduler
        where TContext : DbContext
    {
        private readonly TContext _dbContext;
        private readonly SequentialGuidGenerator<TContext> _guid;

        public RelationalJobScheduler(
            TContext context,
            SequentialGuidGenerator<TContext> guid)
        {
            _dbContext = context;
            _guid = guid;
        }

        public async Task<Job> DequeueAsync()
        {
            var job = await _dbContext.Set<Job>()
                .Where(j => j.Status == JobStatus.Pending)
                .OrderBy(j => j.JobId)
                .FirstOrDefaultAsync();

            if (job == null) return null;
            job.Status = JobStatus.Running;
            await _dbContext.Set<Job>()
                .Where(j => j.JobId == job.JobId)
                .BatchUpdateAsync(j => new Job { Status = JobStatus.Running });

            return job;
        }

        public async Task<Job> ScheduleAsync(JobDescription description)
        {
            var toCreate = new List<Job>();

            void Create(JobDescription description, Guid? parent)
            {
                bool isLeaf = description.Children == null
                    || description.Children.Count == 0;

                if (!isLeaf && parent != null)
                    throw new InvalidOperationException(
                        "Multiple nested job is not supported yet.");

                var newJob = new Job
                {
                    JobId = _guid.Create(),
                    OwnerId = description.OwnerId,
                    Status = isLeaf ? JobStatus.Pending : JobStatus.Composite,
                    SuggestedFileName = description.SuggestedFileName,
                    Arguments = description.Arguments,
                    CreationTime = DateTimeOffset.Now,
                    JobType = description.JobType,
                    ParentJobId = parent,
                };

                toCreate.Add(newJob);
                if (isLeaf) return;
                foreach (var child in description.Children)
                    Create(description, newJob.JobId);
            }

            Create(description, null);

            _dbContext.Add(toCreate[0]);
            await _dbContext.SaveChangesAsync();

            if (toCreate.Count > 1)
            {
                _dbContext.AddRange(toCreate.Skip(1));
                await _dbContext.SaveChangesAsync();
            }

            return toCreate[0];
        }

        public async Task MarkAsync(Job job, JobStatus status, JobStatus? prevStatus = null)
        {
            if (prevStatus.HasValue && job.Status != prevStatus)
                throw new InvalidOperationException("Previous status not match.");

            var affected = await _dbContext.Set<Job>()
                .Where(j => j.JobId == job.JobId)
                .WhereIf(prevStatus.HasValue, j => j.Status == prevStatus)
                .BatchUpdateAsync(j => new Job { Status = status });

            if (prevStatus.HasValue && affected == 0)
                throw new DbUpdateConcurrencyException("The previous status do not match.");

            job.Status = status;
        }
    }
}
