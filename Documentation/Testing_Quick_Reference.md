# Testing Quick Reference

## Build

```powershell
dotnet build ED_Inara_Overlay/ED_Inara_Overlay.sln
```

## Run Tests

```powershell
.\Testing\QuickRegressionTest.ps1
.\Testing\RegressionTest.ps1
.\Testing\RunTests.ps1
```

## Process Checks

```powershell
Get-Process ED_Inara_Overlay -ErrorAction SilentlyContinue |
  Select-Object ProcessName, CPU, @{n='MemoryMB';e={[math]::Round($_.WorkingSet64 / 1MB, 2)}}
```

## Force Cleanup

```powershell
Get-Process ED_Inara_Overlay -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process MockTargetApp -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process notepad -ErrorAction SilentlyContinue | Stop-Process -Force
```

## Logs

- `ED_Inara_Overlay/bin/<Configuration>/net8.0-windows/logs/`
