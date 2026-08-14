Parallel dump download (Python)
================================

Your dump handler already supports HTTP Range requests, so parallel HTTP
download usually works better than FTP (no extra server setup).

Requirements
------------
- Python 3.7+
- Network access to the site

1) HTTP parallel (recommended - works now)
------------------------------------------
python tools\parallel_download.py --url "https://aucoeexam.in/dumpfile.ashx?file=college_full_schema.dmp" -c 8 -o D:\downloads

Use more connections if helpful:
  -c 16

2) FTP parallel (only after DumpFTP site is enabled on IIS)
-----------------------------------------------------------
python tools\parallel_download.py --ftp "ftp://anonymous@66.85.164.106/college_full_schema.dmp" -c 8 -o D:\downloads

Notes
-----
- Parallel connections help only if the limit is per-connection.
  If the server total upload is capped, speed may stay the same.
- Large .dmp files: prefer -c 8 or -c 16 and a fast local disk.
