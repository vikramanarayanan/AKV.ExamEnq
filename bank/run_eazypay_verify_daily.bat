@echo off
REM Daily at 23:00 (11:00 PM): last 24 hours of unpaid rows.
cd /d "%~dp0"
"C:\Program Files\Python312\python.exe" eazypay_verify_job.py --daily >> eazypay_verify_daily.log 2>&1
