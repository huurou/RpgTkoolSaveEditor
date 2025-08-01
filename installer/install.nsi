Unicode True

!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "Sections.nsh"
!include "winmessages.nsh" ; include for some of the windows messages defines

!include ".\installer_util.nsh"
!include ".\version.nsh"


;--------------------------------
; ここで各種定数を定義してください
; APP_VERSION, APP_NAME, PRODUCT_NAME はmake_install.ps1で作成されるversion.nshから読み込まれます。
;--------------------------------

; インストール元フォルダ
!define INST_SRC_DIR "..\bin\Release"
; インストール先フォルダ
!define INST_DST_DIR "$PROGRAMFILES64\${APP_NAME}"
; インストールされたアプリのパス
!define APP_PATH "${INST_DST_DIR}\Release\net8.0-windows\${APP_NAME}.exe"
; インストーラー名
!define INST_NAME "${APP_NAME}_installer.exe"
; アンインストーラーファイルパス インストール先フォルダからの相対パス
!define UNINST_PATH "${APP_NAME}_uninstaller.exe"
; 製品レジストリキー
!define REG_KEY "Software\${APP_NAME}"
; アンインストーラーレジストリキー
!define UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"
; 発行年
!define PUB_YEAR "2025"
; 会社名
!define COMPANY_NAME ""

;-------------------------------
; 生成されるインストーラのファイルプロパティ
;-------------------------------

; ファイルの説明
VIAddVersionKey "FileDescription" "Installer of ${PRODUCT_NAME}."
; ファイルバージョン
VIAddVersionKey "FileVersion" "${APP_VERSION}"
; 製品バージョン
VIAddVersionKey "ProductVersion" "${APP_Version}"
; 製品の著作権
VIAddVersionKey "LegalCopyright" "(C) ${PUB_YEAR} ${COMPANY_NAME}"

; 製品のバージョン
VIProductVersion "${APP_VERSION}"
; インストーラーのタイトルバーに表示される名前
Name "${PRODUCT_NAME} Ver.${APP_VERSION}"
; 生成されるインストーラー
OutFile "${INST_NAME}"
; デフォルトのインストール先ディレクトリ
InstallDir "${INST_DST_DIR}"
; デフォルトでインストーラに「NSIS ver X.XX」と出るので空白にする
BrandingText " "
;管理者権限を要求する
RequestExecutionLevel admin

;--------------------------------
; 画面設定
;--------------------------------

; 画面構成(インストーラー)

; インストール開始画面
!insertmacro MUI_PAGE_WELCOME
; インストール進捗画面
!insertmacro MUI_PAGE_INSTFILES
; インストール完了画面
!insertmacro MUI_PAGE_FINISH

; 画面構成(アンインストーラー)

; アンインストール開始画面
!insertmacro MUI_UNPAGE_WELCOME
; アンインストール確認画面
!insertmacro MUI_UNPAGE_CONFIRM
; アンインストール進捗画面
!insertmacro MUI_UNPAGE_INSTFILES
; アンインストール完了画面
!insertmacro MUI_UNPAGE_FINISH

; 言語設定
!insertmacro MUI_LANGUAGE "japanese"

;--------------------------------
; インストーラー動作設定
;--------------------------------

Section "Install" InstSec
  ; インストール先フォルダを指定
  SetOutPath "$INSTDIR"
  ; レジストリキーの登録
  !insertmacro RegisterProduct "${REG_KEY}" "${UNINST_KEY}" "${PRODUCT_NAME}" "${APP_PATH}" "${UNINST_PATH}" "${APP_VERSION}" "${COMPANY_NAME}"
  ; フォルダの抽出
  File /nonfatal /a /r "${INST_SRC_DIR}"
  ; アンインストーラを出力
  WriteUninstaller "${UNINST_PATH}"
  ; スタートメニューに登録
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}.lnk" "${APP_PATH}"
SectionEnd

;--------------------------------
; アンインストーラー動作設定
;--------------------------------

Section "Uninstall" UninstSec
  ; アンインストーラーを削除
  Delete "${UNINST_PATH}"
  ; フォルダを削除
  RMDir /r "$INSTDIR"
  ; レジストリキーの登録解除
  !insertmacro UnregisterProduct "${REG_KEY}" "${UNINST_KEY}"
  ; スタートメニューから削除
  Delete "$SMPROGRAMS\${PRODUCT_NAME}.lnk"
SectionEnd