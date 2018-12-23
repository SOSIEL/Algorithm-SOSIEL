; Extension infomation
#define ExtensionName "SOSIEL PLATFORM"
#define AppVersion "2.0"
#define AppPublisher "Garry Sotnik"

; Build directory
#define BuildDir "..\..\SOSIEL EX1\Demo\bin\Release\netcoreapp2.0\win-x86"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{C052621B-6EF7-439E-8DA4-AF7013A3CD85}
AppName={#ExtensionName}
AppVersion={#AppVersion}
; Name in "Programs and Features"
AppVerName={#ExtensionName}
AppPublisher={#AppPublisher}
;AppPublisherURL={#AppURL}
;AppSupportURL={#AppURL}
;AppUpdatesURL={#AppURL}
DefaultDirName={pf}\{#ExtensionName}
;DisableDirPage=yes
DefaultGroupName={#ExtensionName}
DisableProgramGroupPage=yes
LicenseFile=THE SOSIEL PLATFORM LICENSE AGREEMENT.rtf
OutputDir={#SourcePath}
OutputBaseFilename={#ExtensionName} {#AppVersion}-setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"


[Files]
Source: {#BuildDir}\*; DestDir: "{app}"; Flags: ignoreversion

[Registry]
Root: "HKCU"; Subkey: "SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"; \
    ValueType: String; ValueName: "{app}\Demo.exe"; ValueData: "RUNASADMIN"; \
    Flags: uninsdeletekeyifempty uninsdeletevalue; MinVersion: 0,6.1

;[Run]



;[UninstallRun]



