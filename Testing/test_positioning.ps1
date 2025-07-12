# Test script for ED Inara Overlay 2.0 positioning fixes
Write-Host "Testing ED Inara Overlay 2.0 window positioning..." -ForegroundColor Green

# Build the project first
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build "D:\Projects\ED_Inara_Overlay_2.0\ED_Inara_Overlay_2.0\ED_Inara_Overlay_2.0.csproj"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful!" -ForegroundColor Green
    
    # Check screen resolution
    $screen = [System.Windows.Forms.Screen]::PrimaryScreen.WorkingArea
    Write-Host "Screen working area: $($screen.Width) x $($screen.Height)" -ForegroundColor Cyan
    
    # Start the application
    Write-Host "Starting application..." -ForegroundColor Yellow
    Write-Host "Expected behavior:" -ForegroundColor Cyan
    Write-Host "1. Main window (toggle button) should appear at bottom-left of screen" -ForegroundColor White
    Write-Host "2. Click the toggle button to open TradeRouteWindow" -ForegroundColor White
    Write-Host "3. TradeRouteWindow should appear at center/right of screen (not off-screen)" -ForegroundColor White
    Write-Host "4. Both windows should be visible and within screen bounds" -ForegroundColor White
    Write-Host ""
    Write-Host "Press Ctrl+C to stop the application when done testing." -ForegroundColor Yellow
    Write-Host ""
    
    # Run the application
    & "D:\Projects\ED_Inara_Overlay_2.0\ED_Inara_Overlay_2.0\bin\Debug\net8.0-windows\ED_Inara_Overlay_2.0.exe"
    
} else {
    Write-Host "Build failed! Check the error messages above." -ForegroundColor Red
}
