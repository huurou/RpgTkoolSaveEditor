$ErrorActionPreference = "Stop"

# ここで各種変数を定義してください
$VS_DEV_SHELL = "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\Tools\Launch-VsDevShell.ps1"
$SLN_PATH = "..\src\RpgTkoolSaveEditor.sln"
$APP_PATH = "..\bin\Release\net8.0-windows\RpgTkoolSaveEditor.exe"
$NSIS_PATH = "C:\Program Files (x86)\NSIS\Bin\makensis.exe"
$INSTALL_SCRIPT = "install.nsi"

try {
    # 必要なツールの確認
    if ($null -eq $VS_DEV_SHELL) {
        throw "Visual Studioのデベロッパーコマンドプロンプトが見つかりません。サポートされているエディション（Professional/Enterprise/Community）がインストールされているか確認してください。"
    }
    if ($null -eq $NSIS_PATH) {
        throw "NSISが見つかりません。NSISをインストールしてください。"
    }
    if (-not (Test-Path $SLN_PATH)) {
        throw "ソリューションファイルが見つかりません: $SLN_PATH"
    }

    # ビルド環境セットアップ
    & $VS_DEV_SHELL

    # スクリプトのルートディレクトリに移動
    Set-Location -Path $PSScriptRoot
    Write-Host "作業ディレクトリを変更しました: $PSScriptRoot" -ForegroundColor Cyan

    # ビルド リリースでリビルドする /m:並列ビルド /fl:ログファイル出力
    Write-Host "ソリューションのビルドを開始します..." -ForegroundColor Cyan
    & MSBuild $SLN_PATH /t:Clean,Restore,Rebuild /p:Configuration=Release /m /fl
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ErrorLevel: $LASTEXITCODE" -ForegroundColor Red
        throw "ビルドに失敗しました。MSBuildのログを確認してください。"
    }
    
    # ビルド結果の確認
    if (-not (Test-Path $APP_PATH)) {
        throw "ビルド後のアプリケーションが見つかりません: $APP_PATH"
    }

    # バージョン アプリ名 製品名抽出
    Write-Host "アプリケーション情報を抽出しています..." -ForegroundColor Cyan
    $APP_VERSION = (Get-ItemProperty $APP_PATH).VersionInfo.FileVersion
    $APP_NAME = [System.IO.Path]::GetFileNameWithoutExtension($APP_PATH)
    $PRODUCT_NAME = (Get-ItemProperty $APP_PATH).VersionInfo.ProductName
    
    # NSISスクリプト用の定義ファイルを作成
    Write-Host "version.nshファイルを作成しています..." -ForegroundColor Cyan
    Out-File -FilePath version.nsh -Encoding Unicode
    Add-Content version.nsh "!define APP_VERSION `"$APP_VERSION`""
    Add-Content version.nsh "!define APP_NAME `"$APP_NAME`""
    Add-Content version.nsh "!define PRODUCT_NAME `"$PRODUCT_NAME`""

    # インストーラ作成
    Write-Host "インストーラーを作成しています..." -ForegroundColor Cyan
    & $NSIS_PATH $INSTALL_SCRIPT
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ErrorLevel: $LASTEXITCODE" -ForegroundColor Red
        throw "インストーラーの作成に失敗しました。NSISのログを確認してください。"
    }
    Write-Host "インストーラーの作成に成功しました。" -ForegroundColor Green
}
catch {
    Write-Host "エラーが発生しました: $_" -ForegroundColor Red
}
finally {
    Pause
}