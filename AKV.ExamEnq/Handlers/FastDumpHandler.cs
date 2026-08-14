using System;
using System.IO;
using System.Web;

namespace AKV.ExamEnq.Handlers
{
    /// <summary>
    /// Minimal handler outside MVC pipeline for high-speed dump downloads.
    /// </summary>
    public class FastDumpHandler : IHttpHandler
    {
        private static readonly string DumpFolder = @"C:\dump";
        private const int BufferSize = 1024 * 1024; // 1 MB

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var fileName = request.QueryString["file"];
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // Also support /dumpfile.ashx/MyFile.zip style path info
                fileName = request.PathInfo;
                if (!string.IsNullOrEmpty(fileName))
                {
                    fileName = fileName.TrimStart('/');
                }
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                response.StatusCode = 400;
                response.Write("File name is required.");
                return;
            }

            var safeName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(safeName) || safeName != Path.GetFileName(fileName.Replace('/', '\\')))
            {
                response.StatusCode = 400;
                response.Write("Invalid file name.");
                return;
            }

            // Normalize: only allow bare file name
            safeName = Path.GetFileName(safeName);
            var resolvedPath = Path.GetFullPath(Path.Combine(DumpFolder, safeName));
            var dumpRoot = Path.GetFullPath(DumpFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!resolvedPath.StartsWith(dumpRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(resolvedPath))
            {
                response.StatusCode = 404;
                response.Write("File not found.");
                return;
            }

            var fileInfo = new FileInfo(resolvedPath);
            var fileLength = fileInfo.Length;
            long start = 0;
            long end = fileLength - 1;
            var isPartial = false;

            var rangeHeader = request.Headers["Range"];
            if (!string.IsNullOrEmpty(rangeHeader) && rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
            {
                var range = rangeHeader.Substring(6).Trim();
                var parts = range.Split('-');
                if (parts.Length == 2)
                {
                    if (!string.IsNullOrEmpty(parts[0]))
                    {
                        long.TryParse(parts[0], out start);
                    }
                    if (!string.IsNullOrEmpty(parts[1]))
                    {
                        long.TryParse(parts[1], out end);
                    }
                    else
                    {
                        end = fileLength - 1;
                    }

                    if (start < 0 || end < start || start >= fileLength)
                    {
                        response.StatusCode = 416;
                        response.AddHeader("Content-Range", "bytes */" + fileLength);
                        return;
                    }

                    if (end >= fileLength)
                    {
                        end = fileLength - 1;
                    }

                    isPartial = true;
                }
            }

            var contentLength = end - start + 1;

            response.Clear();
            response.Buffer = false;
            response.BufferOutput = false;
            response.StatusCode = isPartial ? 206 : 200;
            response.ContentType = "application/octet-stream";
            response.AddHeader("Accept-Ranges", "bytes");
            response.AddHeader("Content-Length", contentLength.ToString());
            response.AddHeader("Content-Disposition",
                "attachment; filename=\"" + safeName.Replace("\"", string.Empty) + "\"");

            if (isPartial)
            {
                response.AddHeader("Content-Range", "bytes " + start + "-" + end + "/" + fileLength);
            }

            // Prefer kernel TransmitFile (fastest on IIS)
            if (!isPartial && start == 0 && contentLength == fileLength)
            {
                response.TransmitFile(resolvedPath);
                response.Flush();
                context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (isPartial)
            {
                response.TransmitFile(resolvedPath, start, contentLength);
                response.Flush();
                context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Fallback: large-buffer sequential stream
            using (var fs = new FileStream(resolvedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
                BufferSize, FileOptions.SequentialScan))
            {
                fs.Seek(start, SeekOrigin.Begin);
                var buffer = new byte[BufferSize];
                long remaining = contentLength;
                while (remaining > 0)
                {
                    var toRead = (int)Math.Min(buffer.Length, remaining);
                    var read = fs.Read(buffer, 0, toRead);
                    if (read <= 0)
                    {
                        break;
                    }

                    response.OutputStream.Write(buffer, 0, read);
                    remaining -= read;
                }
            }

            response.Flush();
            context.ApplicationInstance.CompleteRequest();
        }
    }
}
