using System;

namespace ClientPrinterTray
{

    public class JobItem
    {
        public string JobId { get; set; } = Guid.NewGuid().ToString("N");

        // 🔥 Thêm nhiều file
        public List<string> Files { get; set; } = new();

        public string Printer { get; set; } = "";
        public JobState State { get; private set; } = JobState.Pending;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? Completed { get; set; }
        public string? Error { get; set; }

        public void MarkSuccess()
        {
            State = JobState.Success;
            Completed = DateTime.Now;
        }

        public void MarkFail(string error)
        {
            State = JobState.Fail;
            Completed = DateTime.Now;
            Error = error;
        }
    }

    public enum JobState
    {
        Pending,
        Success,
        Fail
    }
}
