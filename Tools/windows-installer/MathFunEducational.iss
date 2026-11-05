; Compile with Inno Setup 6 after building the game into Publish/Windows.
#define AppName "MathFunEducational"
#define AppVersion "1.0.0"

[Setup]
AppId={{A3F5715A-153F-44C0-8D03-A381E092D9A1}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\..\Publish
OutputBaseFilename=MathFunEducational-Windows-Setup
Compression=lzma2
SolidCompression=yes

[Files]
Source: "..\..\Publish\Windows\*"; DestDir: "{app}"; Excludes: "MathFunEducational_BackUpThisFolder_ButDontShipItWithYourGame\*"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\MathFunEducational.exe"

[Run]
Filename: "{app}\MathFunEducational.exe"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent
