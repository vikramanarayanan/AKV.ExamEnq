@echo off
REM Every 5 minutes: last 5 minutes of unpaid rows.
cd /d "%~dp0"
"C:\Program Files\Python312\python.exe" eazypay_verify_job.py --once >> eazypay_verify_5min.log 2>&1
