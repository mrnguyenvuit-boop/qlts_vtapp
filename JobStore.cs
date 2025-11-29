using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClientPrinterTray
{
    public class JobStore
    {
        private readonly string _path = "job_log.json";

        public List<PrintJob> GetAll()
        {
            if (!File.Exists(_path)) File.WriteAllText(_path, "[]");
            return JsonConvert.DeserializeObject<List<PrintJob>>(File.ReadAllText(_path)) ?? new();
        }

        public void Add(PrintJob j)
        {
            var list = GetAll();
            list.Add(j);
            File.WriteAllText(_path, JsonConvert.SerializeObject(list, Formatting.Indented));
        }

        public void SaveAll(List<PrintJob> list)
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(list, Formatting.Indented));
        }

        public void Clear()
        {
            File.WriteAllText(_path, "[]");
        }
    }
    public class PrintJob
    {
        public string JobId { get; set; }
        public List<string> Files { get; set; }   // ⬅ đổi tên - hỗ trợ multi file
        public string Printer { get; set; }
        public JobState State { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Completed { get; set; }
        public string? Error { get; set; }

        public static PrintJob From(JobItem j) => new PrintJob
        {
            JobId = j.JobId,
            Files = j.Files,
            Printer = j.Printer,
            State = j.State,
            Created = j.Created,
            Completed = j.Completed,
            Error = j.Error
        };
    }

}
