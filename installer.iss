; Inno Setup Script for Elite Dangerous Inara Overlay
; Generated for application version 1.0.0

#define MyAppName "Elite Dangerous Inara Overlay"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ED Inara Overlay Team"
#define MyAppURL "https://github.com/your-repo/ED_Inara_Overlay"
#define MyAppExeName "ED_Inara_Overlay.exe"
#define MyAppAssocName "Elite Dangerous Inara Overlay"
#define MyAppAssocExt ".edovl"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{A3E8B5C7-1F4A-4B9D-8E2C-6F5A9B3D7E8F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
; The [Icons] "quicklaunchicon" entry uses {userappdata} but its [Tasks] entry has a proper IsAdminInstallMode Check.
UsedUserAreasWarning=no
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=dist
OutputBaseFilename=ED_Inara_Overlay_Setup_{#MyAppVersion}
SetupIconFile=ED_Inara_Overlay\Resources\app.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
; Minimum Windows version (Windows 10 1607 for .NET 8)
MinVersion=10.0.14393
; Architecture
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Main application executable
Source: "ED_Inara_Overlay\bin\Release\net8.0-windows\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; Application dependencies
Source: "ED_Inara_Overlay\bin\Release\net8.0-windows\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "ED_Inara_Overlay\bin\Release\net8.0-windows\*.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "ED_Inara_Overlay\bin\Release\net8.0-windows\*.pdb"; DestDir: "{app}"; Flags: ignoreversion
; Theme files
Source: "ED_Inara_Overlay\bin\Release\net8.0-windows\Themes\*"; DestDir: "{app}\Themes"; Flags: ignoreversion
; Application icon
Source: "ED_Inara_Overlay\Resources\app.ico"; DestDir: "{app}"; Flags: ignoreversion
; Application manifest
Source: "app.manifest"; DestDir: "{app}"; Flags: ignoreversion
; Documentation
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "Documentation\*"; DestDir: "{app}\Documentation"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Registry]
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocExt}\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppAssocKey}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes"; ValueType: string; ValueName: ".myp"; ValueData: ""

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[Code]
function GetDotNetVersion(): string;
var
  Version: string;
begin
  if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedframework\Microsoft.NETCore.App', '8.0.0', Version) then
    Result := Version
  else if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedframework\Microsoft.NETCore.App', '8.0.0', Version) then
    Result := Version
  else
    Result := '';
end;

function InitializeSetup(): Boolean;
var
  NetVersion: string;
  ErrorCode: Integer;
begin
  Result := True;
  
  // Check for .NET 8 installation
  NetVersion := GetDotNetVersion();
  if NetVersion = '' then
  begin
    if MsgBox('This application requires .NET 8.0 Desktop Runtime to be installed. Would you like to download it now?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/en-us/download/dotnet/8.0', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
    end;
    Result := False;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Create application data directory for settings
    CreateDir(ExpandConstant('{userappdata}\ED_Inara_Overlay'));
  end;
end;

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\ED_Inara_Overlay"

[InstallDelete]
; Clean up any old installation files
Type: filesandordirs; Name: "{app}\*.old"
