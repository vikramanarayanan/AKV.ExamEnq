#!/usr/bin/env python3
"""
Parallel file downloader for aucoeexam dump files.

Supports:
  - HTTP  : multi-connection Range download via dumpfile.ashx (recommended)
  - FTP   : multi-connection REST+RETR download from DumpFTP site

Examples:
  python parallel_download.py --url https://aucoeexam.in/dumpfile.ashx?file=college_full_schema.dmp
  python parallel_download.py --ftp ftp://aucoeexam.in/college_full_schema.dmp --connections 8
  python parallel_download.py --ftp ftp://anonymous@66.85.164.106/college_full_schema.dmp -c 16 -o D:\\downloads
"""

from __future__ import print_function

import argparse
import os
import sys
import threading
import time
from concurrent.futures import ThreadPoolExecutor, as_completed

try:
    from urllib.parse import urlparse, parse_qs, unquote
    from urllib.request import Request, urlopen
except ImportError:
    from urlparse import urlparse, parse_qs  # type: ignore
    from urllib import unquote  # type: ignore
    from urllib2 import Request, urlopen  # type: ignore

try:
    from ftplib import FTP
except ImportError:
    FTP = None


def human(n):
    for unit in ("B", "KB", "MB", "GB", "TB"):
        if n < 1024.0:
            return "{:.2f} {}".format(n, unit)
        n /= 1024.0
    return "{:.2f} PB".format(n)


class Progress(object):
    def __init__(self, total):
        self.total = total
        self.done = 0
        self.lock = threading.Lock()
        self.t0 = time.time()

    def add(self, n):
        with self.lock:
            self.done += n
            elapsed = max(time.time() - self.t0, 0.001)
            speed = self.done / elapsed
            pct = (100.0 * self.done / self.total) if self.total else 0.0
            sys.stdout.write(
                "\r{:.1f}%  {} / {}  ({}/s)   ".format(
                    pct, human(self.done), human(self.total), human(speed)
                )
            )
            sys.stdout.flush()


# ---------------- HTTP parallel (Range) ----------------

def http_size(url):
    req = Request(url, method="HEAD")
    try:
        with urlopen(req, timeout=60) as resp:
            length = resp.headers.get("Content-Length")
            accept = resp.headers.get("Accept-Ranges", "")
            if length:
                return int(length), "bytes" in accept.lower()
    except TypeError:
        # Python 2 / older: Request has no method=
        req = Request(url)
        req.get_method = lambda: "HEAD"  # type: ignore
        resp = urlopen(req, timeout=60)
        length = resp.headers.get("Content-Length")
        accept = resp.headers.get("Accept-Ranges", "")
        resp.close()
        if length:
            return int(length), "bytes" in accept.lower()
    except Exception:
        pass

    # Fallback: GET first byte
    req = Request(url, headers={"Range": "bytes=0-0"})
    resp = urlopen(req, timeout=60)
    cr = resp.headers.get("Content-Range", "")
    resp.close()
    if "/" in cr:
        return int(cr.split("/")[-1]), True
    raise RuntimeError("Could not determine file size (server must support HTTP Range).")


def http_chunk(url, start, end, path, progress):
    req = Request(url, headers={"Range": "bytes={}-{}".format(start, end)})
    resp = urlopen(req, timeout=600)
    with open(path, "r+b") as f:
        f.seek(start)
        while True:
            buf = resp.read(1024 * 1024)
            if not buf:
                break
            f.write(buf)
            progress.add(len(buf))
    resp.close()


def download_http(url, dest, connections):
    size, ranged = http_size(url)
    print("HTTP size: {}  Range support: {}".format(human(size), ranged))
    if not ranged and connections > 1:
        print("WARNING: no Range support; falling back to 1 connection")
        connections = 1

    with open(dest, "wb") as f:
        f.truncate(size)

    if connections <= 1:
        progress = Progress(size)
        http_chunk(url, 0, size - 1, dest, progress)
        print()
        return

    part = (size + connections - 1) // connections
    progress = Progress(size)
    with ThreadPoolExecutor(max_workers=connections) as pool:
        futures = []
        for i in range(connections):
            start = i * part
            end = min(start + part - 1, size - 1)
            if start > end:
                break
            futures.append(pool.submit(http_chunk, url, start, end, dest, progress))
        for fut in as_completed(futures):
            fut.result()
    print()


# ---------------- FTP parallel (REST) ----------------

def ftp_connect(host, port, user, password):
    ftp = FTP()
    ftp.connect(host, port, timeout=60)
    ftp.login(user, password)
    ftp.set_pasv(True)
    return ftp


def ftp_size(host, port, user, password, remote):
    ftp = ftp_connect(host, port, user, password)
    try:
        ftp.sendcmd("TYPE I")
        return ftp.size(remote)
    finally:
        try:
            ftp.quit()
        except Exception:
            ftp.close()


def ftp_chunk(host, port, user, password, remote, start, end, path, progress):
    ftp = ftp_connect(host, port, user, password)
    try:
        ftp.sendcmd("TYPE I")
        sock = None
        try:
            ftp.sendcmd("REST {}".format(start))
            sock = ftp.transfercmd("RETR " + remote)
            remaining = end - start + 1
            with open(path, "r+b") as f:
                f.seek(start)
                while remaining > 0:
                    buf = sock.recv(min(1024 * 1024, remaining))
                    if not buf:
                        break
                    f.write(buf)
                    got = len(buf)
                    remaining -= got
                    progress.add(got)
        finally:
            if sock is not None:
                sock.close()
            try:
                ftp.voidresp()
            except Exception:
                pass
    finally:
        try:
            ftp.quit()
        except Exception:
            ftp.close()


def download_ftp(url, dest, connections):
    if FTP is None:
        raise RuntimeError("ftplib not available")

    parsed = urlparse(url)
    host = parsed.hostname
    port = parsed.port or 21
    user = unquote(parsed.username or "anonymous")
    password = unquote(parsed.password or "anonymous@")
    remote = unquote(parsed.path.lstrip("/"))
    if not remote:
        raise RuntimeError("FTP URL must include remote file path")

    size = ftp_size(host, port, user, password, remote)
    if not size or size <= 0:
        raise RuntimeError("Could not get FTP file size for " + remote)

    print("FTP size: {}  connections: {}".format(human(size), connections))

    with open(dest, "wb") as f:
        f.truncate(size)

    if connections <= 1:
        progress = Progress(size)
        ftp_chunk(host, port, user, password, remote, 0, size - 1, dest, progress)
        print()
        return

    part = (size + connections - 1) // connections
    progress = Progress(size)
    with ThreadPoolExecutor(max_workers=connections) as pool:
        futures = []
        for i in range(connections):
            start = i * part
            end = min(start + part - 1, size - 1)
            if start > end:
                break
            futures.append(
                pool.submit(
                    ftp_chunk, host, port, user, password, remote, start, end, dest, progress
                )
            )
        for fut in as_completed(futures):
            fut.result()
    print()


def guess_filename(url):
    parsed = urlparse(url)
    if parsed.scheme in ("http", "https"):
        qs = parse_qs(parsed.query)
        if "file" in qs and qs["file"]:
            return os.path.basename(qs["file"][0])
    name = os.path.basename(unquote(parsed.path.rstrip("/")))
    return name or "download.bin"


def main():
    p = argparse.ArgumentParser(description="Parallel dump file downloader (HTTP/FTP)")
    g = p.add_mutually_exclusive_group(required=True)
    g.add_argument("--url", help="HTTP(S) dump URL, e.g. https://aucoeexam.in/dumpfile.ashx?file=NAME")
    g.add_argument("--ftp", help="FTP URL, e.g. ftp://anonymous@aucoeexam.in/NAME")
    p.add_argument("-c", "--connections", type=int, default=8, help="Parallel connections (default 8)")
    p.add_argument("-o", "--outdir", default=".", help="Output directory")
    p.add_argument("-f", "--filename", default=None, help="Override output file name")
    args = p.parse_args()

    url = args.url or args.ftp
    name = args.filename or guess_filename(url)
    dest = os.path.join(args.outdir, name)
    os.makedirs(args.outdir, exist_ok=True)

    print("Source :", url)
    print("Output :", dest)
    print("Parts  :", args.connections)

    t0 = time.time()
    if args.ftp:
        download_ftp(url, dest, max(1, args.connections))
    else:
        download_http(url, dest, max(1, args.connections))
    elapsed = max(time.time() - t0, 0.001)
    size = os.path.getsize(dest)
    print(
        "Done in {:.1f}s  avg {}/s  saved {}".format(
            elapsed, human(size / elapsed), dest
        )
    )


if __name__ == "__main__":
    main()
