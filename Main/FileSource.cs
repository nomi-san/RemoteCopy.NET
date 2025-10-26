using System;
using System.IO;
using System.Threading.Tasks;

namespace RemoteCopy
{
    public class FileSource
    {
        public string Name { get; init; }
        public long Size { get; init; }
        public DateTime? Date { get; init; }
        public Func<Task<Stream>> Fetcher { get; init; }

        public FileSource(string name, long size, DateTime? date, Func<Task<Stream>> fetcher)
        {
            this.Name = name;
            this.Size = size;
            this.Date = date;
            this.Fetcher = fetcher;
        }
    }
}