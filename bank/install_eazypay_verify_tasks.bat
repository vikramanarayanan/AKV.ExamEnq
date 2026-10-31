@echo off
REM Registers both verify jobs. Run as Administrator once.
set BAT5=%~dp0run_eazypay_verify_once.bat
set BAT24=%~dp0run_eazypay_verify_daily.bat

schtasks /Create /TN "AKV-EazypayVerify-5Min" /TR "\"%BAT5%\"" /SC MINUTE /MO 5 /F /RL LIMITED
if errorlevel 1 echo Failed to create 5-minute task.

schtasks /Create /TN "AKV-EazypayVerify-Daily24h" /TR "\"%BAT24%\"" /SC DAILY /ST 23:00 /F /RL LIMITED
if errorlevel 1 echo Failed to create daily 23:00 (11:00 PM) task.

echo.
schtasks /Query /TN "AKV-EazypayVerify-5Min" /FO LIST /V
echo.
schtasks /Query /TN "AKV-EazypayVerify-Daily24h" /FO LIST /V
