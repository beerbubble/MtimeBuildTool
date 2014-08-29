@echo off
Set nowdate=%date:~0,4%%date:~5,2%%date:~8,2%

Rd /s/q "\\192.168.0.25\ftproot\mtime\upversion\MtimeCSearch\%nowdate%" & Md "\\192.168.0.25\ftproot\mtime\upversion\MtimeCSearch\%nowdate%"
xcopy "E:\Package\Service\MtimeCSearch\%nowdate%" "\\192.168.0.25\ftproot\mtime\upversion\MtimeCSearch\%nowdate%\" /Y /I /S