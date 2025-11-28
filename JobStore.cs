using System.Collections.Generic;
using System.Linq;

namespace ClientPrinterTray
{
    public class JobStore
    {
        private readonly List<JobItem> _jobs = new();

        public void Add(JobItem job)
        {
            _jobs.Add(job);
        }

        public List<JobItem> GetAll()
        {
            return _jobs.ToList();
        }
    }
}
