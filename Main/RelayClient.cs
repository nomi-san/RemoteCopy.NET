using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteCopy
{
    static class RelayClient
    {
        static string MakeUrl(Session session, int fileIndex)
            => string.Format("{0}/{1}/{2}", session.Host, session.Id, fileIndex);

        public static async Task<bool> UploadFile(HttpClient http, Session session, int fileIndex, CancellationToken cancellation)
        {
            var url = MakeUrl(session, fileIndex);
            var file = session.FileList[fileIndex];
            var path = Path.Combine(session.FromDir, file.Path);

            if (!File.Exists(path))
                return false;

            try
            {
                using var fileStream = File.OpenRead(path);
                using var content = new StreamContent(fileStream, 65536);
                using var response = await http.PostAsync(url, content, cancellation);

                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch
            {
            }

            return false;
        }

        public static async Task<Stream> DownloadFile(HttpClient http, Session session, int fileIndex, CancellationToken cancellation)
        {
            var url = MakeUrl(session, fileIndex);

            var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellation);
            //var length = response.Content.Headers.ContentLength!.Value;
            var stream = await response.Content.ReadAsStreamAsync(cancellation);

            return stream;
        }
    }
}