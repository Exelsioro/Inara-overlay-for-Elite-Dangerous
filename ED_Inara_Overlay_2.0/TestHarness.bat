@echo off
echo ED Inara Overlay 2.0 - Automated Test Harness
echo ==============================================
echo.
echo This harness will test the overlay focus behavior:
echo 1. Start app with no target - waiting window should be visible
echo 2. Launch mock target - waiting should hide, overlay should appear
echo 3. Change focus to Notepad - overlay should hide
echo 4. Return focus to mock target - overlay should reappear
echo.

set "APP_PATH=%~dp0bin\Debug\net8.0-windows\ED_Inara_Overlay_2.0.exe"
set "MOCK_TARGET_NAME=MockTargetApp"

echo Checking if required files exist...
if not exist "%APP_PATH%" (
    echo ERROR: Application executable not found at %APP_PATH%
    echo Please build the application first using: dotnet build
    pause
    exit /b 1
)

echo.
echo =====================================================
echo TEST 1: Starting overlay app with no target running
echo =====================================================
echo.
echo Starting overlay app targeting "MockTargetApp" (which doesn't exist yet)...
start "ED_Inara_Overlay" "%APP_PATH%" %MOCK_TARGET_NAME%
echo.
echo VERIFY: A waiting window should appear showing "Looking for: MockTargetApp.exe"
echo.
pause

echo.
echo ================================================
echo TEST 2: Launching mock target application
echo ================================================
echo.
echo Now we'll compile and launch the mock target application...
echo.

REM Compile the mock target app
echo Compiling MockTargetApp...
csc /target:winexe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /out:MockTargetApp.exe ../MockTargetApp.cs

if not exist "MockTargetApp.exe" (
    echo ERROR: Failed to compile MockTargetApp.exe
    echo Make sure you have .NET SDK installed
    pause
    exit /b 1
)

echo.
echo Starting MockTargetApp...
start "Mock Target" MockTargetApp.exe
echo.
echo VERIFY: 
echo - The waiting window should disappear
echo - The overlay should appear over the mock target window
echo - The overlay should be visible when the mock target has focus
echo.
pause

echo.
echo ================================================
echo TEST 3: Testing focus change behavior
echo ================================================
echo.
echo Now we'll test focus behavior by switching to Notepad...
echo.
echo Starting Notepad...
start notepad.exe
echo.
echo INSTRUCTIONS:
echo 1. Click on the Notepad window to give it focus
echo 2. VERIFY: The overlay should hide when Notepad has focus
echo 3. Then click back on the Mock Target Application window
echo 4. VERIFY: The overlay should reappear when Mock Target has focus
echo.
pause

echo.
echo ================================================
echo TEST 4: Cleanup and preparation for Elite Dangerous test
echo ================================================
echo.
echo Cleaning up test applications...
echo.
echo Closing test applications...
taskkill /f /im MockTargetApp.exe >nul 2>&1
taskkill /f /im notepad.exe >nul 2>&1
taskkill /f /im ED_Inara_Overlay_2.0.exe >nul 2>&1
echo.
echo Mock test completed successfully!
echo.
echo ================================================
echo TEST 5: Testing with Elite Dangerous (if available)
echo ================================================
echo.
echo Now we'll test with the real Elite Dangerous client...
echo.
echo Starting overlay for Elite Dangerous...
start "ED_Inara_Overlay_ED" "%APP_PATH%" EliteDangerous64
echo.
echo INSTRUCTIONS:
echo 1. If Elite Dangerous is not running, you should see a waiting window
echo 2. Start Elite Dangerous from Steam/Epic/Frontier
echo 3. VERIFY: The waiting window should disappear and overlay should appear
echo 4. Alt+Tab to switch to another application (like Notepad)
echo 5. VERIFY: The overlay should hide when Elite Dangerous doesn't have focus
echo 6. Alt+Tab back to Elite Dangerous
echo 7. VERIFY: The overlay should reappear when Elite Dangerous has focus
echo.
echo NOTE: If Elite Dangerous is already running, the overlay should appear immediately
echo.
pause

echo.
echo ================================================
echo Test Harness Complete
echo ================================================
echo.
echo All tests have been run. The overlay should demonstrate:
echo - Waiting window when target is not running
echo - Overlay appears when target is detected
echo - Overlay hides when target loses focus
echo - Overlay reappears when target regains focus
echo.
echo Press any key to close any remaining test applications...
pause

echo.
echo Cleaning up...
taskkill /f /im MockTargetApp.exe >nul 2>&1
taskkill /f /im notepad.exe >nul 2>&1
taskkill /f /im ED_Inara_Overlay_2.0.exe >nul 2>&1

if exist "MockTargetApp.exe" del "MockTargetApp.exe"

echo.
echo Test harness completed successfully!
echo.
pause
