
' CUSTOM ACTION
' Shows a warning message that Powershel is not installed
' Requires property "NoPowershellWarningMessage"
Sub ShowNoPowershellWarningMessage()
  Set rec = Session.Installer.CreateRecord(1)
  rec.StringData(0) = session.Property("NoPowershellWarningMessage")
  Session.Message &H02000000,rec
End Sub

' CUSTOM ACTION
' Detects the version of installed Powershell and puts the result into properties PSVERSIONMAJOR,PSVERSIONMINOR,PSVERSIONBUILD,PSVERSIONREV
' If Powershell is not installed (or, cannot be called by "powershell" in command line, the properties are emptied.
Sub GetPowershellVersion()

  ' Temp registry key that Powershell populates with a version
  regpath = "HKEY_CURRENT_USER\Software\NLedger\InstallerScripts"
  regname = "PSVersion"
  regkey = regpath&"\"&regname

  ' Powershell command that puts PS version to a registry key
  pscmd = "if(!(Test-Path 'Registry::"&regpath&"')){New-Item -Path 'Registry::"&regpath&"' -ItemType RegistryKey -Force};Set-ItemProperty -path 'Registry::"&regpath&"' -Name '"&regname&"' -Type 'String' -Value ($PSVersionTable.PSVersion.ToSTring())"

  ' Version numbers to populate
  versionMajor = ""
  versionMinor = ""
  versionBuild = ""
  versionRevision = ""

  ' Getting PS version; the numbers are populated only if everything is OK
  with CreateObject("WScript.Shell")
      on error resume next
      .RegDelete regkey
      .run "powershell -NoProfile -Command ""& { "&pscmd&" }""",0,True
      Err.Clear
      sPsVersion = .regread(regkey)
      if err.number = 0 then
         versionNumbers = Split(sPsVersion,".")
         if (UBound(versionNumbers)>=0) then
            versionMajor = versionNumbers(0)
         end if
         if (UBound(versionNumbers)>=1) then
            versionMinor = versionNumbers(1)
         end if
         if (UBound(versionNumbers)>=2) then
            versionBuild = versionNumbers(2)
         end if
         if (UBound(versionNumbers)>=3) then
            versionRevision = versionNumbers(3)
         end if
      end if
  end with

  session.Property("PSVERSIONMAJOR") = versionMajor
  session.Property("PSVERSIONMINOR") = versionMinor
  session.Property("PSVERSIONBUILD") = versionBuild
  session.Property("PSVERSIONREV") = versionRevision

End Sub

const LOCAL_APPLICATION_DATA = &H1c
const COMMON_APPLICATION_DATA = &H23
const PERSONAL = &H05

' Internal function - returns a full path to a special folder
Function SpecialFolderPath(specFolderID)
  Set objShell = CreateObject("Shell.Application")
  Set objFolder = objShell.Namespace(specFolderID)
  Set objFolderItem = objFolder.Self
  SpecialFolderPath = objFolderItem.Path
End Function

' Internal function - deletes a folder if exists but it is empty
Function RemoveEmptyFolder(fso,path)
  If fso.FolderExists(path) Then
     Set objFolder = fso.GetFolder(path)
     if (objFolder.Files.Count = 0 And objFolder.SubFolders.Count = 0) Then
        ' Folder exists and it is empty
        fso.DeleteFolder path
     End If
  End If
End Function

' Internal function - shows Yes/No dialog
Function YesNoMessage(message)
  Set rec = Session.Installer.CreateRecord(1)
  rec.StringData(0) = message
  dlgResult = Session.Message(&H03000000+vbYesNo,rec)
  YesNoMessage = (dlgResult = vbYes)
End Function

' CUSTOM ACTION
' Asks a user whether to delete folders with user files and deletes them
' If folders are empty - deletes them silently
' Requires property "DeleteUserSettingsQuestionText"
Sub DeleteUserSettings()

  on error resume next

  commonSettingsFolder = SpecialFolderPath(COMMON_APPLICATION_DATA)&"\NLedger"
  userSettingsFolder = SpecialFolderPath(LOCAL_APPLICATION_DATA)&"\NLedger"
  nledgerDocsFolder = SpecialFolderPath(PERSONAL)&"\NLedger"
  nledgerDocsSandboxFolder = SpecialFolderPath(PERSONAL)&"\NLedger\DemoSandbox"

  Set fso = CreateObject("Scripting.FileSystemObject")

  ' Try to remove folders if they are empty
  RemoveEmptyFolder fso,commonSettingsFolder
  RemoveEmptyFolder fso,userSettingsFolder
  RemoveEmptyFolder fso,nledgerDocsSandboxFolder
  RemoveEmptyFolder fso,nledgerDocsFolder

  questionFolders = ""
  if (fso.FolderExists(commonSettingsFolder)) Then
     questionFolders = questionFolders&commonSettingsFolder&vbCrLf
  End If
  if (fso.FolderExists(userSettingsFolder)) Then
     questionFolders = questionFolders&userSettingsFolder&vbCrLf
  End If
  if (fso.FolderExists(nledgerDocsFolder)) Then
     questionFolders = questionFolders&nledgerDocsFolder&vbCrLf
  End If

  questionText = session.Property("DeleteUserSettingsQuestionText")&vbCrLf&vbCrLf

  if questionFolders<>"" Then
     if YesNoMessage(questionText&questionFolders) Then    
        if (fso.FolderExists(commonSettingsFolder)) Then
            fso.DeleteFolder commonSettingsFolder
        End If
        if (fso.FolderExists(userSettingsFolder)) Then
            fso.DeleteFolder userSettingsFolder
        End If
        if (fso.FolderExists(nledgerDocsFolder)) Then
            fso.DeleteFolder nledgerDocsFolder
        End If
     End If
  End If

End Sub
