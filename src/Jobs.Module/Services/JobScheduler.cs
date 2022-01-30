using Jobs.Entities;
using Jobs.Models;
using Jobs.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.JobsModule.Services
{
    public partial class RelationalJobStorage<TContext> : IJobScheduler
        where TContext : DbContext
    {
        private readonly TContext _dbContext;
        private readonly SequentialGuidGenerator<TContext> _guid;
        private readonly IJobFileProvider _fileProvider;
        private readonly IResettableSignal<JobHostedService> _signal;

        public RelationalJobStorage(
            TContext context,
            SequentialGuidGenerator<TContext> guid,
            IResettableSignal<JobHostedService> signal,
            IJobFileProvider fileProvider)
        {
            _dbContext = context;
            _guid = guid;
            _fileProvider = fileProvider;
            _signal = signal;
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
            var invalidChars = Path.GetInvalidFileNameChars();

            void Create(JobDescription description, Guid? parent)
            {
                bool isLeaf = description.Children == null
                    || description.Children.Count == 0;

                if (!isLeaf && parent != null)
                    throw new InvalidOperationException(
                        "Multiple nested job is not supported yet.");

                if (invalidChars.Any(description.SuggestedFileName.Contains))
                    throw new InvalidOperationException(
                        "The suggested file name is invalid.");

                var newJob = new Job
                {
                    JobId = _guid.Create(),
                    OwnerId = description.OwnerId,
                    Status = isLeaf ? JobStatus.Pending : JobStatus.Composite,
                    Composite = !isLeaf,
                    SuggestedFileName = description.SuggestedFileName,
                    Arguments = description.Arguments ?? "{}",
                    CreationTime = DateTimeOffset.Now,
                    JobType = description.JobType,
                    ParentJobId = parent,
                };

                toCreate.Add(newJob);
                if (isLeaf) return;
                foreach (var child in description.Children)
                    Create(child, newJob.JobId);
            }

            Create(description, null);

            _dbContext.Add(toCreate[0]);
            await _dbContext.SaveChangesAsync();

            if (toCreate.Count > 1)
            {
                _dbContext.AddRange(toCreate.Skip(1));
                await _dbContext.SaveChangesAsync();
            }

            _signal.Notify();
            return toCreate[0];
        }

        public async Task MarkAsync(Job job, JobStatus status, DateTimeOffset? completeTime = null, JobStatus? prevStatus = null)
        {
            if (prevStatus.HasValue && job.Status != prevStatus)
                throw new InvalidOperationException("Previous status not match.");

            var affected = await _dbContext.Set<Job>()
                .Where(j => j.JobId == job.JobId)
                .WhereIf(prevStatus.HasValue, j => j.Status == prevStatus)
                .BatchUpdateAsync(j => new Job { Status = status, CompleteTime = completeTime });

            if (prevStatus.HasValue && affected == 0)
                throw new DbUpdateConcurrencyException("The previous status do not match.");

            job.Status = status;

            if (!job.ParentJobId.HasValue)
            {
                return;
            }
            else if (status == JobStatus.Failed)
            {
                await _dbContext.Set<Job>()
                    .Where(j => j.JobId == job.ParentJobId && j.Status == JobStatus.Composite)
                    .BatchUpdateAsync(j => new Job { Status = JobStatus.Failed });

                await _dbContext.Set<Job>()
                    .Where(j => j.ParentJobId == job.ParentJobId && j.Status == JobStatus.Pending)
                    .BatchUpdateAsync(j => new Job { Status = JobStatus.Cancelled });
            }
            else
            {
                await _dbContext.Set<Job>()
                    .Where(j => j.JobId == job.ParentJobId && j.Status == JobStatus.Composite)
                    .Where(j => _dbContext.Set<Job>().Where(i => i.ParentJobId == j.JobId).All(i => i.Status == JobStatus.Finished))
                    .BatchUpdateAsync(j => new Job { Status = JobStatus.Pending });
            }
        }
    }
}
