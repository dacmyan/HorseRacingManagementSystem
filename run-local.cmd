@echo off
title Horse Racing Management System Runner
set ASPNETCORE_ENVIRONMENT=Development
cls
echo =====================================================================
echo                HORSE RACING MANAGEMENT SYSTEM
echo =====================================================================
echo.
echo Dang khoi dong Backend API (cong 5000) va Frontend Dev Server (cong 5173)...
echo.
echo - Log cua Backend se co tien to [Backend] mau xanh duong.
echo - Log cua Frontend se co tien to [Frontend] mau xanh la.
echo - Nhan Ctrl+C (hoac dong terminal) de dung ca 2 dich vu cung luc.
echo =====================================================================
echo.

echo [*] Dang giai phong cong 5005 va 5173 neu con bi chiem...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":5005" ^| findstr "LISTENING" 2^>nul') do (
    taskkill /PID %%a /F >nul 2>&1
)
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":5173" ^| findstr "LISTENING" 2^>nul') do (
    taskkill /PID %%a /F >nul 2>&1
)
timeout /t 1 /nobreak >nul
echo [*] San sang khoi dong...
echo.

npx concurrently -k -p "[{name}]" -n "Backend,Frontend" -c "blue.bold,green.bold" "cd backend && dotnet run --project src/HorseRacing.API -- --urls http://localhost:5005" "cd frontend && npm run dev"

