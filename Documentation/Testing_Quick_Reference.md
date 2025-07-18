# ED Inara Overlay - Quick Testing Reference

## Quick Start

### Prerequisites Check
`powershell
# Check .NET installation
dotnet --version

# Build the application
dotnet build
`

### Fast Test (5 minutes)
`powershell
cd Testing
.\QuickRegressionTest.ps1
`

### Full Test Suite (15-20 minutes)
`powershell
cd Testing
.\TestHarness.ps1
`

## Test Commands Quick Reference

### Manual Testing
`cmd
# Basic test with mock target
ED_Inara_Overlay_2.0.exe MockTargetApp

# Test with Elite Dangerous
ED_Inara_Overlay_2.0.exe EliteDangerous64

# Test with Notepad (for focus testing)
ED_Inara_Overlay_2.0.exe notepad
`

### Automated Testing
`powershell
# Quick smoke test
.\QuickRegressionTest.ps1

# Basic functionality test
.\BasicRegressionTest.ps1

# Full regression suite
.\AutomatedRegressionTest.ps1

# Run all tests
.\RunTests.ps1
`

## Expected Behaviors

### ✅ Correct Behavior
- Waiting window appears when target not found
- Overlay appears over target window when target is running
- Overlay hides when target loses focus
- Overlay reappears when target regains focus
- Application exits cleanly when target process ends

### ❌ Issues to Report
- Overlay appears in wrong position
- Overlay doesn't hide when focus changes
- Memory leaks (increasing memory usage over time)
- Crashes or hangs
- Performance degradation

## Test Checklist

### Before Release
- [ ] All automated tests pass
- [ ] Manual focus testing with Elite Dangerous
- [ ] Multi-monitor testing (if applicable)
- [ ] Performance testing (run for 30+ minutes)
- [ ] Edge case testing (process termination, network issues)
- [ ] Installation/deployment testing

### Critical Test Areas
1. **Window Focus Detection** - Most important feature
2. **Process Detection** - Core functionality
3. **Overlay Positioning** - User experience
4. **Memory/Performance** - Long-term stability
5. **Error Handling** - Robustness

## Common Issues & Solutions

|           Issue               |        Cause        |               Solution            |
|-------------------------------|---------------------|-----------------------------------|
| TestHarness fails to find app | Build not current   | Run dotnet build                  |
| MockTargetApp won't compile   | Missing C# compiler | Install .NET SDK                  |
| Overlay doesn't appear        | Wrong process name  | Check Task Manager for exact name |
| Focus detection broken        | Windows API issue   | Restart application               |

## Performance Baselines

### Acceptable Performance
- **Memory Usage**: < 50MB steady state
- **CPU Usage**: < 2% when overlay hidden, < 5% when visible
- **Startup Time**: < 3 seconds
- **Focus Detection**: < 500ms response time

### Performance Testing Commands
`powershell
# Monitor memory usage
Get-Process "ED_Inara_Overlay_2.0" | Select-Object ProcessName, WorkingSet64, CPU

# Continuous monitoring
while (True) { 
    Get-Process "ED_Inara_Overlay_2.0" -ErrorAction SilentlyContinue | 
    Select-Object ProcessName, @{n='Memory(MB)';e={[math]::Round(.WorkingSet64/1MB,2)}}, CPU
    Start-Sleep 5 
}
`

## Debug Information

### Enable Debug Logging
`cmd
set ED_OVERLAY_DEBUG=1
ED_Inara_Overlay_2.0.exe EliteDangerous64
`

### Log Locations
- Application logs: %TEMP%\ED_Inara_Overlay_2.0\
- Windows Event Viewer: Applications and Services Logs
- PowerShell test logs: Testing\TestResults\

## Emergency Procedures

### If Application Hangs
`powershell
# Force kill all overlay processes
Get-Process "ED_Inara_Overlay_2.0" | Stop-Process -Force

# Clean up test processes
Get-Process "MockTargetApp" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process "notepad" -ErrorAction SilentlyContinue | Stop-Process -Force
`

### If System Becomes Unresponsive
1. Ctrl+Alt+Del → Task Manager
2. Find ED_Inara_Overlay_2.0 process
3. End process tree
4. Report bug with system configuration details

---

*Keep this reference handy during development and testing!*
