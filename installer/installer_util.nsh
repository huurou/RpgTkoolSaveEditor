!include "FileFunc.nsh"
!include "WordFunc.nsh"

; ----------------------------------------------------------------------
; 登録されたアプリケーションをアンインストールする
; ----------------------------------------------------------------------
!macro UninstallInSilent RegKey UninstKey
  Push $1
  Push $2

  ReadRegStr $1 HKLM "${UninstKey}" "UninstallString"

  ${IF} $1 != ""
    ReadRegStr $2 HKLM "${RegKey}" ""
    nsExec::Exec "$1 /S _?=$2"
    Pop $0
    Delete $1
    RMDir "$2"
  ${ENDIF}

  Pop $2
  Pop $1
!macroend

; ----------------------------------------------------------------------
; アプリケーションをレジストリに登録する
; ----------------------------------------------------------------------
!macro RegisterProduct RegKey UninstKey ProductName ProductIcon UninstallerPath AppVersion CompanyName
  WriteRegStr HKLM "${RegKey}" "" $INSTDIR
  WriteRegStr HKLM "${UninstKey}" "DisplayName" "${ProductName}"
  WriteRegStr HKLM "${UninstKey}" "DisplayIcon" "$\"${ProductIcon}$\""
  WriteRegStr HKLM "${UninstKey}" "UninstallString" "$\"${UninstallerPath}$\""
  WriteRegStr HKLM "${UninstKey}" "QuietUninstallString" "$\"${UninstallerPath}$\" /S"
  WriteRegStr HKLM "${UninstKey}" "DisplayVersion" "${AppVersion}"
  WriteRegStr HKLM "${UninstKey}" "Publisher" "${CompanyName}"
  WriteRegDWORD HKLM "${UninstKey}" "NoModify" 1
  WriteRegDWORD HKLM "${UninstKey}" "NoRepair" 1
!macroend

; ----------------------------------------------------------------------
; アプリケーションをレジストリから削除する
; ----------------------------------------------------------------------
!macro UnregisterProduct RegKey UninstKey
  DeleteRegKey HKLM "${UninstKey}"
  DeleteRegKey HKLM "${RegKey}"
!macroend
