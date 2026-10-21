;defining variables
#define Repository     "..\."
#define MyAppName      "BCFier"
#define MyAppVersion GetFileVersion("..\Bcfier.Win\bin\Release\Bcfier.Win.dll")
#define MyAppPublisher "Matteo Cominetti"
#define MyAppURL       "http://www.bcfier.com/"
#define MyAppExeName   "Bcfier.Win.exe"

#define WinAppName    "Bcfier.Win"


[Setup]
AppId={{0d553633-80f8-490b-84d6-9d3d6ad4196d}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={userpf}\{#MyAppName}
DisableDirPage=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
OutputDir={#Repository}
OutputBaseFilename=BCFier
SetupIconFile={#Repository}\Assets\icon.ico
Compression=lzma
SolidCompression=yes
WizardImageFile={#Repository}\Assets\bcfier-banner.bmp
ChangesAssociations=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Dirs]
Name: "{app}"; Permissions: everyone-full

[Files]
Source: "{#Repository}\{#WinAppName}\bin\Release\{#WinAppName}.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Repository}\{#WinAppName}\bin\Release\{#WinAppName}.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Repository}\{#WinAppName}\bin\Release\{#WinAppName}.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Repository}\{#WinAppName}\bin\Release\{#WinAppName}.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Repository}\{#WinAppName}\bin\Release\Bcfier.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Repository}\{#WinAppName}\bin\Release\Renga.WPF.Styles.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Repository}\{#WinAppName}\bin\Release\en-US\*"; DestDir: "{app}\en-US"; Flags: ignoreversion
Source: "{#Repository}\{#WinAppName}\bin\Release\ru-RU\*"; DestDir: "{app}\ru-RU"; Flags: ignoreversion
Source: "{#Repository}\Assets\BCF.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{userpf}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Classes\.bcfzip"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\{#MyAppName}"; ValueType: string; ValueName: ""; ValueData: "BCF File"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\BCF.ico"
Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}""""%1"""

;checks if the .NET 8 Desktop Runtime is installed
[Code]
function GetInstalledRuntimeVersion(): string;
var
  subkey: string;
  version: string;
begin
  subkey := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
  if RegQueryStringValue(HKLM, subkey, 'Version', version) then
    Result := version;
end;

function InitializeSetup(): Boolean;
var
  installed: string;
  errCode: integer;
begin
  installed := GetInstalledRuntimeVersion();
  if (installed = '') or (CompareVersion(installed, '8.0.0') < 0) then begin
    if MsgBox('{#MyAppName} requires Microsoft .NET 8 Desktop Runtime.'#13#13
          'Do you want me to open https://dotnet.microsoft.com/download/dotnet/8.0'#13
          'so you can download it?', mbConfirmation, MB_YESNO) = IDYES then begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/8.0',
        '', '', SW_SHOW, ewNoWait, errCode);
    end;
    Result := false;
  end else
    Result := true;
end;