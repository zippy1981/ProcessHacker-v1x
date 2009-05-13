;ISTool Version 5.2.1 and Inno Setup 5.2.4 dev, Script by XhmikosR
;
;Requirements:
;*Inno Setup QuickStart Pack:
;   http://www.jrsoftware.org/isdl.php#qsp

#define installer_build_number "32"
#define app_version	GetFileVersion("..\..\bin\Release\ProcessHacker.exe")
#define installer_build_date GetDateTimeString('dd/mm/yyyy', '.', '')
#define app_updates_url "http://processhacker.sourceforge.net/"
#define app_support_url "http://processhacker.sourceforge.net/"
#define app_contact "http://processhacker.sourceforge.net/"
#define app_publisher_url "http://processhacker.sourceforge.net/"

; From now on you'll probably won't have to change anything, so be carefull
[Setup]
AppID=Process_Hacker
AppCopyright=Copyright � 2008-2009, Process Hacker Team. Licensed under the GNU GPL, v3.
AppContact={#= app_contact}
AppName=Process Hacker
AppVerName=Process Hacker {#= app_version}
AppPublisher=wj32
AppPublisherURL={#= app_publisher_url}
AppSupportURL={#= app_support_url}
AppUpdatesURL={#= app_updates_url}
UninstallDisplayName=Process Hacker {#= app_version}
DefaultDirName={pf}\Process Hacker
DefaultGroupName=Process Hacker
VersionInfoCompany=wj32
VersionInfoCopyright=wj32
VersionInfoDescription=Process Hacker {#= app_version} Setup
VersionInfoTextVersion={#= app_version}
VersionInfoVersion={#= app_version}
VersionInfoProductName=Process Hacker {#= app_version}
VersionInfoProductVersion={#= app_version}
AppVersion={#= app_version}
MinVersion=0,5.01.2600
AppReadmeFile={app}\README.txt
LicenseFile=..\..\..\LICENSE.txt
InfoAfterFile=..\..\..\CHANGELOG.txt
SetupIconFile=Icons\ProcessHacker.ico
UninstallDisplayIcon={app}\ProcessHacker.exe
WizardImageFile=Icons\ProcessHackerLarge.bmp
WizardSmallImageFile=Icons\ProcessHackerSmall.bmp
OutputDir=.
OutputBaseFilename=processhacker-{#= app_version}-setup
AllowNoIcons=True
Compression=lzma/ultra64
SolidCompression=True
InternalCompressLevel=ultra64
EnableDirDoesntExistWarning=False
DirExistsWarning=No
ShowTasksTreeLines=True
AlwaysShowDirOnReadyPage=True
AlwaysShowGroupOnReadyPage=True
WizardImageStretch=False
PrivilegesRequired=Admin
ShowLanguageDialog=Auto
LanguageDetectionMethod=uilanguage
AppMutex=Global\ProcessHackerMutex

;Specify the architectures that Process Hacker can run on
ArchitecturesAllowed=x86

[Languages]
Name: en; MessagesFile: compiler:Default.isl
Name: gr; MessagesFile: Languages\Greek.isl

[Messages]
BeveledLabel=Process Hacker v{#= app_version} by wj32                                                    Setup v{#= app_version}.{#= installer_build_number} built on {#= installer_build_date}

; Include the installer's custom messages
#include "Custom_Messages.iss"

[Files]
Source: ..\..\bin\Release\Assistant.exe; DestDir: {app}; Flags: ignoreversion
Source: ..\..\bin\Release\base.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\..\bin\Release\CHANGELOG.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\..\bin\Release\Help.htm; DestDir: {app}; Flags: ignoreversion
Source: ..\..\bin\Release\LICENSE.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\..\bin\Release\kprocesshacker.sys; DestDir: {app}; Flags: ignoreversion
Source: ..\..\bin\Release\NProcessHacker.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\..\bin\Release\ProcessHacker.exe; DestDir: {app}; Flags: ignoreversion
Source: ..\..\bin\Release\README.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\..\bin\Release\structs.txt; DestDir: {app}; Flags: ignoreversion
Source: Icons\uninstall.ico; DestDir: {app}; Flags: ignoreversion

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}
Name: desktopicon\user; Description: {cm:tsk_currentuser}; GroupDescription: {cm:AdditionalIcons}; Flags: exclusive
Name: desktopicon\common; Description: {cm:tsk_allusers}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked exclusive
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

Name: startuptask; Description: {cm:tsk_startupdescr}; GroupDescription: {cm:tsk_startup}; Check: StartupRegCheck(); Flags: unchecked checkablealone
Name: startuptask\minimized; Description: {cm:tsk_startupdescrmin}; GroupDescription: {cm:tsk_startup}; Check: StartupRegCheck(); Flags: unchecked
Name: removestartuptask; Description: {cm:tsk_removestartup}; GroupDescription: {cm:tsk_startup}; Check: NOT StartupRegCheck(); Flags: unchecked

Name: resetsettings; Description: {cm:tsk_resetsettings}; GroupDescription: {cm:tsk_other}; Check: SettingsExistCheck(); Flags: unchecked checkablealone

Name: setdefaulttaskmgr; Description: {cm:tsk_setdefaulttaskmgr}; GroupDescription: {cm:tsk_other}; Check: PHDefaultCheck(); Flags: unchecked dontinheritcheck
Name: restoretaskmgr; Description: {cm:tsk_restoretaskmgr}; GroupDescription: {cm:tsk_other}; Check: NOT PHDefaultCheck(); Flags: unchecked dontinheritcheck

Name: createKPHservice; Description: {cm:tsk_createKPHservice}; GroupDescription: {cm:tsk_other}; Check: scExeExistsCheck() AND KPHServiceCheck(); Flags: unchecked dontinheritcheck
Name: deleteKPHservice; Description: {cm:tsk_deleteKPHservice}; GroupDescription: {cm:tsk_other}; Check: scExeExistsCheck() AND NOT KPHServiceCheck(); Flags: unchecked dontinheritcheck

[INI]
Filename: {app}\Homepage.url; Section: InternetShortcut; Key: URL; String: {#= app_updates_url}

[Icons]
Name: {group}\Process Hacker; Filename: {app}\ProcessHacker.exe; Comment: Process Hacker {#= app_version}; WorkingDir: {app}; IconFilename: {app}\ProcessHacker.exe; IconIndex: 0
Name: {group}\{cm:sm_help}\{cm:sm_changelog}; Filename: {app}\CHANGELOG.txt; Comment: {cm:sm_com_changelog}; WorkingDir: {app}
Name: {group}\{cm:sm_help}\{cm:sm_helpfile}; Filename: {app}\Help.htm; Comment: {cm:sm_helpfile}; WorkingDir: {app}
Name: {group}\{cm:sm_help}\{cm:sm_readmefile}; Filename: {app}\README.txt; Comment: {cm:sm_com_readmefile}; WorkingDir: {app}
Name: {group}\{cm:sm_help}\{cm:ProgramOnTheWeb,Process Hacker}; Filename: {#= app_updates_url}; Comment: {cm:ProgramOnTheWeb,Process Hacker}
Name: {group}\{cm:UninstallProgram,Process Hacker}; Filename: {uninstallexe}; IconFilename: {app}\uninstall.ico; Comment: {cm:UninstallProgram,Process Hacker}; WorkingDir: {app}

Name: {commondesktop}\Process Hacker; Filename: {app}\ProcessHacker.exe; Tasks: desktopicon\common; Comment: Process Hacker {#= app_version}; WorkingDir: {app}; IconFilename: {app}\ProcessHacker.exe; IconIndex: 0
Name: {userdesktop}\Process Hacker; Filename: {app}\ProcessHacker.exe; Tasks: desktopicon\user; Comment: Process Hacker {#= app_version}; WorkingDir: {app}; IconFilename: {app}\ProcessHacker.exe; IconIndex: 0
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\Process Hacker; Filename: {app}\ProcessHacker.exe; Tasks: quicklaunchicon; Comment: Process Hacker {#= app_version}; WorkingDir: {app}; IconFilename: {app}\ProcessHacker.exe; IconIndex: 0

[InstallDelete]
;Remove files from the install folder which are not needed anymore
Type: files; Name: {app}\ProcessHacker.exe.config
Type: files; Name: {app}\HACKING.txt
Type: files; Name: {app}\psvince.dll

Type: files; Name: {userdesktop}\Process Hacker.lnk
Type: files; Name: {commondesktop}\Process Hacker.lnk

;Remove other languages' shortcuts in Start Menu
Type: files; Name: {group}\Process Hacker's Readme file.lnk
Type: files; Name: {group}\Process Hacker on the Web.url
Type: files; Name: {group}\Uninstall Process Hacker.lnk
Type: files; Name: {group}\Help and Support\Process Hacker on the Web.url
Type: files; Name: {group}\Help and Support\Change Log.lnk
Type: files; Name: {group}\Help and Support\Changelog.lnk
Type: files; Name: {group}\Help and Support\Process Hacker's Help.lnk
Type: files; Name: {group}\Help and Support\ReadMe File.lnk
Type: files; Name: {group}\Help and Support\ReadMe.lnk
Type: dirifempty; Name: {group}\Help and Support

Type: files; Name: {group}\������ �������� ��� Process Hacker.lnk
Type: files; Name: {group}\�� Process Hacker ��� Internet.url
Type: files; Name: {group}\������������� ��� Process Hacker.lnk
Type: files; Name: {group}\������� ��� ����������\�� Process Hacker ��� Internet.url
Type: files; Name: {group}\������� ��� ����������\�������� ��������.lnk
Type: files; Name: {group}\������� ��� ����������\������ �������� ��� Process Hacker.lnk
Type: files; Name: {group}\������� ��� ����������\������ ReadMe.lnk
Type: dirifempty; Name: {group}\������� ��� ����������

Type: filesandordirs; Name: {localappdata}\wj32; Tasks: resetsettings

[Registry]
Root: HKLM; Subkey: SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\taskmgr.exe; Flags: uninsdeletekeyifempty dontcreatekey
Root: HKCU; SubKey: Software\Microsoft\Windows\CurrentVersion\Run; ValueType: string; ValueName: Process Hacker; ValueData: """{app}\ProcessHacker.exe"""; Tasks: startuptask; Flags: uninsdeletevalue
Root: HKCU; SubKey: Software\Microsoft\Windows\CurrentVersion\Run; ValueType: string; ValueName: Process Hacker; ValueData: """{app}\ProcessHacker.exe"" -m"; Tasks: startuptask\minimized; Flags: uninsdeletevalue
Root: HKCU; SubKey: Software\Microsoft\Windows\CurrentVersion\Run; ValueName: Process Hacker; Tasks: removestartuptask; Flags: deletevalue uninsdeletevalue
Root: HKLM; Subkey: SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\taskmgr.exe; ValueType: string; ValueName: Debugger; ValueData: """{app}\ProcessHacker.exe"""; Tasks: setdefaulttaskmgr; Flags: uninsdeletevalue
Root: HKLM; Subkey: SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\taskmgr.exe; ValueName: Debugger; Tasks: restoretaskmgr resetsettings; Flags: deletevalue uninsdeletevalue

[Run]
Filename: {sys}\sc.exe; Parameters: stop KProcessHacker; Check: scExeExistsCheck() AND KProcessHackerStateCheck(); StatusMsg: {cm:msg_stopkprocesshacker}; Flags: runhidden runascurrentuser
Filename: {sys}\sc.exe; Parameters: "create KProcessHacker binPath= ""{app}\kprocesshacker.sys"" type= kernel start= auto"; Tasks: createKPHservice; StatusMsg: {cm:msg_createkprocesshacker}; Flags: runhidden runascurrentuser
Filename: {sys}\sc.exe; Parameters: start KProcessHacker; Tasks: createKPHservice; StatusMsg: {cm:msg_startkprocesshacker}; Flags: runhidden runascurrentuser
Filename: {win}\Microsoft.NET\Framework\v2.0.50727\ngen.exe; Parameters: "install ""{app}\ProcessHacker.exe"""; StatusMsg: {cm:msg_optimizingperformance}; Flags: runhidden runascurrentuser skipifdoesntexist
Filename: {sys}\sc.exe; Parameters: stop KProcessHacker; Tasks: deleteKPHservice; Flags: runhidden runascurrentuser
Filename: {sys}\sc.exe; Parameters: delete KProcessHacker; Tasks: deleteKPHservice; Flags: runhidden runascurrentuser

Filename: {app}\ProcessHacker.exe; Description: {cm:LaunchProgram,Process Hacker}; Flags: nowait postinstall skipifsilent runascurrentuser
Filename: {app}\Homepage.url; Description: {cm:run_visitwebsite}; Flags: nowait postinstall skipifsilent shellexec runascurrentuser unchecked

[UninstallDelete]
Type: files; Name: {app}\Homepage.url

[UninstallRun]
Filename: {sys}\sc.exe; Parameters: stop KProcessHacker; Check: KProcessHackerStateCheck(); Flags: runhidden skipifdoesntexist
Filename: {sys}\sc.exe; Parameters: delete KProcessHacker; Check: KProcessHackerStateCheck(); Flags: runhidden skipifdoesntexist

[Code]
// Create a mutex for the installer
const installer_mutex_name = 'process_hacker_setup_mutex';


// Function to check if app is already installed
function IsInstalled( AppID: String ): Boolean;
var
	sPrevPath: String;
begin
	sPrevPath := '';
	if not RegQueryStringValue(HKLM, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\'+AppID+'_is1', 'Inno Setup: App Path', sPrevpath) then
		RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\'+AppID+'_is1', 'Inno Setup: App Path', sPrevpath);

  Result := sPrevPath<>'';
end;


// If this is an update then we use the same directories again
function ShouldSkipPage(PageID: Integer): Boolean;
begin
	Result := False;
	if (PageID = wpSelectDir) or (PageID = wpSelectProgramGroup) then begin
		Result := IsInstalled('Process_Hacker');
	end;
end;


// Check if Process Hacker is configured to run on startup in order to control startup choice within the installer
function StartupRegCheck(): Boolean;
begin
	Result := True;
	if RegValueExists(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'Process Hacker') then
	Result := False;
end;


// Check if Process Hacker's settings exist
function SettingsExistCheck(): Boolean;
begin
	Result := False;
	if DirExists(ExpandConstant('{localappdata}\wj32\')) then
	Result := True;
end;


// Check if Process Hacker is set as the default Task Manager for Windows
function PHDefaultCheck(): Boolean;
var
	svalue: String;
begin
	Result := True;
	if RegQueryStringValue(HKLM,
	  'SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\taskmgr.exe', 'Debugger', svalue) then begin
		if svalue = (ExpandConstant('"{app}\ProcessHacker.exe"')) then
		Result := False;
	end;
end;


// Check if KProcessHacker is started
function KProcessHackerStateCheck(): Boolean;
begin
	Result := False;
	if RegKeyExists(HKLM, 'SYSTEM\CurrentControlSet\Services\KProcessHacker') then
	Result := True;
end;


// Check if KProcessHacker is installed as a service
function KPHServiceCheck(): Boolean;
var
	dvalue: DWORD;
begin
	Result := True;
	if RegQueryDWordValue(HKLM, 'SYSTEM\CurrentControlSet\Services\KProcessHacker', 'Start', dvalue) then begin
		if dvalue = 2 then
		Result := False;
	end;
end;


// Check if sc.exe exists
function scExeExistsCheck(): Boolean;
begin
	Result := False;
	if FileExists(ExpandConstant('{sys}\sc.exe')) then
	Result := True;
end;


Procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
	// When uninstalling ask user to delete Process Hacker's and settings based on whether this file exists only
	if CurUninstallStep = usUninstall then begin
		if DirExists(ExpandConstant('{localappdata}\wj32\')) or fileExists(ExpandConstant('{app}\Process Hacker Log.txt'))
		or fileExists(ExpandConstant('{userdocs}\Process Hacker.txt')) or fileExists(ExpandConstant('{userdocs}\Process Hacker.log'))
		or fileExists(ExpandConstant('{userdocs}\Process Hacker.csv')) or fileExists(ExpandConstant('{userdocs}\Process Hacker Log.txt'))
		or fileExists(ExpandConstant('{userdocs}\CSR Processes.txt')) then begin
			if MsgBox(ExpandConstant('{cm:msg_DeleteLogSettings}'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then begin
				DelTree(ExpandConstant('{localappdata}\wj32\'), True, True, True);
				DeleteFile(ExpandConstant('{app}\Process Hacker.txt'));
				DeleteFile(ExpandConstant('{app}\Process Hacker.log'));
				DeleteFile(ExpandConstant('{app}\Process Hacker.csv'));
				DeleteFile(ExpandConstant('{app}\Process Hacker Log.txt'));
				DeleteFile(ExpandConstant('{app}\CSR Processes.txt'));
				DeleteFile(ExpandConstant('{userdocs}\Process Hacker Log.txt'));
				DeleteFile(ExpandConstant('{userdocs}\CSR Processes.txt'));
			end;
		end;
	end;
end;


function InitializeSetup(): Boolean;

// Check if .NET Framework is installed and if not offer to download it
var
  ErrorCode: Integer;
  NetFrameWorkInstalled : Boolean;
  Result1 : Boolean;
begin
	// Create a mutex for the installer and if it's already running then expose a message and stop installation
	if CheckForMutexes(installer_mutex_name) then begin
		if not WizardSilent() then
			MsgBox(ExpandConstant('{cm:msg_SetupIsRunningWarningInstall}'), mbError, MB_OK);
			Result := False;
	end
	else begin
		CreateMutex(installer_mutex_name);

	NetFrameWorkInstalled := RegKeyExists(HKLM,'SOFTWARE\Microsoft\.NETFramework\policy\v2.0');
		if NetFrameWorkInstalled then begin
			Result := True;
			end
			else begin
				Result1 := MsgBox(ExpandConstant('{cm:msg_asknetdown}'), mbCriticalError, MB_YESNO or MB_DEFBUTTON1) = IDYES;
				if Result1 =False then begin
				Result:=False;
			end
			else begin
				Result:=False;
			ShellExec('open', 'http://download.microsoft.com/download/5/6/7/567758a3-759e-473e-bf8f-52154438565a/dotnetfx.exe',
			'','',SW_SHOWNORMAL,ewNoWait,ErrorCode);
			end;
		end;
	end;
end;


function InitializeUninstall(): Boolean;
begin
	Result := True;
	if CheckForMutexes(installer_mutex_name) then begin
		if not WizardSilent() then
			MsgBox(ExpandConstant('{cm:msg_SetupIsRunningWarningUninstall}'), mbError, MB_OK);
		Result := False;
		end
		else begin
		CreateMutex(installer_mutex_name);
	end;
end;
