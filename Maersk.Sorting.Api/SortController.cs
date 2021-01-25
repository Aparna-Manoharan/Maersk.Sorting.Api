using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maersk.Sorting.Api.ExceptionHandler;

namespace Maersk.Sorting.Api.Controllers
{
    [ApiController]
    [Route("sort")]
    public class SortController : ControllerBase
    {
        private readonly ISortJobProcessor _sortJobProcessor;
        private IMemoryCache _cacheprovider;
        public SortController(ISortJobProcessor sortJobProcessor, IMemoryCache cacheprovider)
        {
            _sortJobProcessor = sortJobProcessor;
            _cacheprovider = cacheprovider;
        }

        [HttpPost("run")]
        [Obsolete("This executes the sort job asynchronously. Use the asynchronous 'EnqueueJob' instead.")]
        public async Task<ActionResult<SortJob>> EnqueueAndRunJob(int[] values)
        {
            var pendingJob = new SortJob(
                id: Guid.NewGuid(),
                status: SortJobStatus.Pending,
                duration: null,
                input: values,
                output: null);

            var completedJob = await _sortJobProcessor.Process(pendingJob);

            return Ok(completedJob);
        }

        [HttpPost]
        public ActionResult<SortJob> EnqueueJob(int[] values)
        {
            var pendingJob = new SortJob(
             id: Guid.NewGuid(),
             status: SortJobStatus.Pending,
             duration: null,
             input: values,
             output: null);
            List<SortJob> jobs;
            if (!_cacheprovider.TryGetValue("SortJobCache", out jobs)) {            
                jobs = new List<SortJob>();
            }
            jobs.Add(pendingJob);
            _cacheprovider.Set("SortJobCache", jobs);
            return Ok(pendingJob);
        }

        [HttpGet]
        public async Task<ActionResult<SortJob[]>> GetJobs()
        {            
            List<SortJob> jobs;           
            if (_cacheprovider.TryGetValue("SortJobCache", out jobs))
            {
                return await Task.FromResult(jobs.ToArray());
            }
            return NotFound(new ApiResponse((int)HttpStatusCode.NotFound, "No Jobs found"));
        }

        [HttpGet("{jobId}")]
        public async Task<ActionResult<SortJob>> GetJob(Guid jobId)
        {
            SortJob job;
            if (_cacheprovider.Get("SortJobCache") !=null)
            {
                job =  _cacheprovider.Get<IEnumerable<SortJob>>("SortJobCache").Where( x => x.Id == jobId).FirstOrDefault();
                return await Task.FromResult(job);
            }
             return NotFound(new ApiResponse((int)HttpStatusCode.NotFound, "Job with provided id doesnot exist"));
        }
    }
}
