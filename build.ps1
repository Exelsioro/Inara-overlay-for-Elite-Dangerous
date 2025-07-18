Set-Location "D:\Projects\ED_Inara_Overlay_2.0\ED_Inara_Overlay_2.0"
dotnet build ED_Inara_Overlay_2.0.sln
Write-Host "Build completed. Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
