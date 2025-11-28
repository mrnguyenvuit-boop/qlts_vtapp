using System;
using System.Threading.Tasks;

namespace ClientPrinterTray
{
    public class PrintQueue
    {
        private readonly JobStore _store;
        private readonly DbLogger _logger;

        public event Action<JobItem>? JobUpdated;

        public PrintQueue(JobStore store, DbLogger logger)
        {
            _store = store;
            _logger = logger;
        }

        // thêm job vào queue
        public async Task EnqueueAsync(JobItem job)
        {
            _store.Add(job);
            JobUpdated?.Invoke(job);
            await _logger.InsertAsync(job.JobId, "Đã thêm vào hàng đợi", true);
        }

        // xử lý in PDF
        public async Task ProcessAsync(JobItem job)
        {
            try
            {
                Printer.PrintSilent(job.FilePath, job.Printer);
                job.MarkSuccess();
                JobUpdated?.Invoke(job);

                await _logger.InsertAsync(job.JobId, "In thành công", true);
            }
            catch (Exception ex)
            {
                job.MarkFail(ex.Message);
                JobUpdated?.Invoke(job);

                await _logger.InsertAsync(job.JobId, ex.Message, false);
            }
        }
    }
}
