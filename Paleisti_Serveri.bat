@echo off
setlocal
where dotnet >nul 2>&1
if errorlevel 1 (
    echo .NET SDK not found. The C# server requires .NET 9 SDK.
    pause
    exit /b 1
)
cd /d "%~dp0Server"
echo Starting Bomberman server on http://localhost:5000 ...
dotnet run
if errorlevel 1 (
    echo Server failed to start; check the error above.
    pause
    exit /b 1
)
exit /b 0
