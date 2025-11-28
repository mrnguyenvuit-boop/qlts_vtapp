@echo off
echo Publishing ClientPrinterTray (net9.0-windows, win-x64)...
dotnet publish -c Release -r win-x64 --self-contained true --output publish
echo.
echo ===============================
echo  Build xong. File exe: publish\ClientPrinterTray.exe
echo ===============================
pause
