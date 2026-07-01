@echo off
title Horse Racing Management System Runner
cls
echo =====================================================================
echo                HORSE RACING MANAGEMENT SYSTEM
echo =====================================================================
echo.
echo [1/2] Khoi dong Backend API (cong 5000)...
start "Backend API" /D "%~dp0backend" cmd /k dotnet run --project src/HorseRacing.API -- --urls http://localhost:5000

echo [2/2] Khoi dong Frontend (cong 5173)...
start "Frontend Dev" /D "%~dp0frontend" cmd /k npm run dev

echo.
echo =====================================================================
echo  Khoi dong hoan tat! Backend va Frontend dang chay o 2 cua so rieng.
echo  - Swagger: http://localhost:5000/swagger
echo  - Frontend: http://localhost:5173
echo =====================================================================
echo.
pause
