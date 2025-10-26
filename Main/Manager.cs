using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteCopy
{
    public static class Manager
    {
        static HttpClient _http = new();
        static ConcurrentDictionary<string,
            (Session session, CancellationTokenSource cancellation)> _map = new();

        const string _defaultHost = "https://ppng.io";

        public static string? GetSessionJson(string sessionId)
        {
            if (_map.TryGetValue(sessionId, out var local))
                return local.session.ToJson();

            return null;
        }

        public static void CancelSession(string sessionId)
        {
            if (_map.TryRemove(sessionId, out var session))
            {
                session.cancellation.Cancel();
            }
        }

        public static bool CreateSession(IList<string> paths, int maxCount, long maxTotalSize, string? host, out string? sessionId)
        {
            sessionId = null;

            if (maxCount <= 0)
                maxCount = int.MaxValue;
            if (maxTotalSize <= 0)
                maxTotalSize = int.MaxValue;
            if (string.IsNullOrEmpty(host))
                host = _defaultHost;

            if (paths.Count == 0 || paths.Count > maxCount)
                return false;

            long totalSize = 0;
            var fileList = new List<Session.File>();
            var fromDir = Directory.GetParent(paths[0])!.FullName;

            void pushFile(FileInfo info)
            {
                fileList.Add(new Session.File
                {
                    Path = info.FullName.Replace(fromDir, string.Empty).Trim('/', '\\'),
                    Size = info.Length,
                    Date = info.LastWriteTime.Ticks
                });

                totalSize += info.Length;
            }

            foreach (var path in paths)
            {
                if (fileList.Count >= maxCount || totalSize >= maxTotalSize)
                    return false;

                var attr = File.GetAttributes(path);
                if (attr.HasFlag(FileAttributes.Hidden))
                    continue;

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    var di = new DirectoryInfo(path);
                    var infoList = di.EnumerateFiles("*", SearchOption.AllDirectories);

                    if (infoList.Count() + fileList.Count >= maxCount)
                        return false;

                    foreach (var info2 in infoList)
                    {
                        if (!info2.Attributes.HasFlag(FileAttributes.Hidden))
                        {
                            if (fileList.Count >= maxCount || totalSize >= maxTotalSize)
                                return false;

                            pushFile(info2);
                        }
                    }
                }
                else
                {
                    pushFile(new FileInfo(path));
                }
            }

            sessionId = string.Format("{0}/{1:x}",
                Guid.NewGuid(),
                DateTime.UtcNow.Ticks);

            var session = new Session
            {
                Host = host,
                Id = sessionId,
                FileList = fileList,
                FileCount = fileList.Count,
                TotalSize = totalSize,
                FromDir = fromDir
            };

            _map.TryAdd(session.Id, (session, new CancellationTokenSource()));

            return true;
        }

        public static async Task<bool> UploadSession(string sessionId)
        {
            if (_map.TryGetValue(sessionId, out var local))
            {
                var (session, cancellation) = local;

                for (int i = 0; i < session.FileList.Count; i++)
                {
                    if (cancellation.IsCancellationRequested)
                        return false;

                    if (!await RelayClient.UploadFile(_http, session, i, cancellation.Token))
                        return false;
                }

                return true;
            }

            return false;
        }

        public static IEnumerable<FileSource>? DownloadSession(string sessionJson, CancellationToken cancellation, out string? sessionId)
        {
            sessionId = null;

            var session = Session.FromJson(sessionJson);
            if (session == null) return null;

            var files = new List<FileSource>();
            for (int i = 0; i < session.FileList.Count; i++)
            {
                var index = i;
                var file = session.FileList[i];
                var fetcher = async () =>
                {
                    if (file.Size == 0)
                        return new MemoryStream(1);

                    return await RelayClient
                        .DownloadFile(_http, session, index, cancellation)
                        .ConfigureAwait(false);
                };

                files.Add(new(file.Path, file.Size, new DateTime(file.Date), fetcher));
            }

            sessionId = session.Id;
            return files;
        }
    }
}