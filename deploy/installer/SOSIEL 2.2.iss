; Extension infomation
#define ExtensionName "SOSIEL PLATFORM"
#define AppVersion "2.2"
#define AppPublisher "Garry Sotnik"

; Build directory
#define BuildDir "..\..\SOSIEL EX1\Demo\bin\Release\netcoreapp2.0\publish"

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
DefaultDirName=D:\{#ExtensionName}
DisableDirPage=no
DefaultGroupName={#ExtensionName}
DisableProgramGroupPage=yes
LicenseFile=THE SOSIEL PLATFORM LICENSE AGREEMENT.rtf
OutputDir={#SourcePath}
OutputBaseFilename={#ExtensionName} {#AppVersion}-setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: {app}; Permissions: users-modify

[Files]
Source: {#BuildDir}\*; DestDir: "{app}"; Flags: ignoreversion

;[Run]



;[UninstallRun]



