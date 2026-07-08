@echo off
title Horse Racing Management System Runner
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

npx concurrently -k -p "[{name}]" -n "Backend,Frontend" -c "blue.bold,green.bold" "cd backend && dotnet run --project src/HorseRacing.API -- --urls http://localhost:5000" "cd frontend && npm run dev"

