using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Maersk.Sorting.Api
{
    public class SortJobHostedService : BackgroundService
    {
        protected IMemoryCache _cacheprovider;
        protected ISortJobProcessor _sortJobProcessor;      
        public SortJobHostedService(IMemoryCache cacheprovider, ISortJobProcessor sortJobProcessor)
        {
            _cacheprovider = cacheprovider;
            _sortJobProcessor = sortJobProcessor;           
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<SortJob> jobs;
                if (_cacheprovider.TryGetValue("SortJobCache", out jobs))
                {
                    jobs.ForEach(async job =>
                   {
                       if (job.Status == SortJobStatus.Pending)
                       {
                           var res  = await _sortJobProcessor.Process(job);                           
                           var index = jobs.ToList().IndexOf(job);
                           if (index != -1)
                               jobs[index] = res;
                           _cacheprovider.Set("SortJobCache", jobs);
                       }
                   });
                }
                
                
                await Task.Delay(new TimeSpan(0, 1, 0));
            }

        }

    }
}
