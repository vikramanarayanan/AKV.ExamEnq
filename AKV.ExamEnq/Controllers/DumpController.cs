using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace AKV.ExamEnq.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    [AllowAnonymous]
    public class DumpController : Controller
    {
        private static readonly string DumpFolder = @"C:\dump";

        public class DumpFileInfo
        {
            public string FileName { get; set; }
            public long SizeBytes { get; set; }
            public DateTime LastModified { get; set; }
        }

        // GET: Dump
        public ActionResult Index()
        {
            var files = new List<DumpFileInfo>();

            if (Directory.Exists(DumpFolder))
            {
                files = Directory.GetFiles(DumpFolder)
                    .Select(path => new FileInfo(path))
                    .OrderByDescending(f => f.LastWriteTime)
                    .Select(f => new DumpFileInfo
                    {
                        FileName = f.Name,
                        SizeBytes = f.Length,
                        LastModified = f.LastWriteTime
                    })
                    .ToList();
            }

            ViewBag.DumpFolder = DumpFolder;
            return View(files);
        }

        // GET: Dump/Download?fileName=example.zip
        // Redirects to lightweight ashx handler (bypasses MVC) for max speed
        public ActionResult Download(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return new HttpStatusCodeResult(400, "File name is required.");
            }

            var safeName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(safeName) || safeName != fileName)
            {
                return new HttpStatusCodeResult(400, "Invalid file name.");
            }

            var fullPath = Path.Combine(DumpFolder, safeName);
            var dumpRoot = Path.GetFullPath(DumpFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var resolvedPath = Path.GetFullPath(fullPath);

            if (!resolvedPath.StartsWith(dumpRoot, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(resolvedPath))
            {
                return HttpNotFound("File not found.");
            }

            // Redirect to lightweight handler – avoids MVC overhead
            var url = VirtualPathUtility.ToAbsolute("~/dumpfile.ashx") + "?file=" + Uri.EscapeDataString(safeName);
            return Redirect(url);
        }
    }
}
