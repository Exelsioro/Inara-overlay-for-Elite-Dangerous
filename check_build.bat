@echo off
echo Checking build for Elite Dangerous Overlay...
cd /d "D:\Projects\ED_Inara_Overlay_2.0\ED_Inara_Overlay_2.0"

echo.
echo === Building Solution ===
dotnet build ED_Inara_Overlay_2.0.sln --verbosity normal

echo.
echo === Build Results ===
if %errorlevel% equ 0 (
    echo BUILD SUCCESSFUL!
) else (
    echo BUILD FAILED! Error Level: %errorlevel%
)

echo.
pause
