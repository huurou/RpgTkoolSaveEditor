# RpgTkoolSaveEditor v1.1.0

RPGツクールMV および RPGツクールMZ のセーブデータ閲覧・編集ソフト

## 概要

このソフトはRPGツクールMVとRPGツクールMZのセーブデータの閲覧・編集ソフトです。セーブデータをリアルタイムで編集でき、ゲーム側での変更も自動的に反映されます。

## 機能

- セーブデータの閲覧・編集
- セーブデータの自動保存（編集時に即座に反映）
- ゲーム側の変更の自動検知・更新
- 武器・防具の一括変更機能（1〜99個）
- コンテキストメニューによる操作

## システム要件

- Windows 10/11
- .NET 8.0 Desktop Runtime

## インストール方法

1. [Releases](https://github.com/huurou/RpgTkoolSaveEditor/releases) からインストーラーをダウンロード
2. インストーラーを実行してソフトウェアをインストール
3. デスクトップに作成されたショートカットから起動

## 使用方法

事前にゲームで1番目のスロットでセーブを行っておく必要があります。

1. デスクトップに作成されたショートカットにwwwフォルダ内にあるsaveフォルダをドラッグアンドドロップしてください。
2. セーブエディタが開きます。
3. セーブデータの値を編集するとセーブデータが自動で上書きされます。
4. ゲームでセーブするなどしてセーブデータが変更された場合、自動でセーブエディタに読み込まれ画面が更新されます。
5. ゲームに反映する場合はゲーム側でセーブデータをロードしてください。

## 注意点

- このソフトはWindows専用です。
- このソフトはRPGツクールMVとRPGツクールMZ専用です。
- ファイル名が「file1」となっているもののみ編集可能です。

## 開発者向け情報

### ビルド方法

```bash
git clone https://github.com/huurou/RpgTkoolSaveEditor.git
cd RpgTkoolSaveEditor/src
dotnet build RpgTkoolSaveEditor.sln
```

### 使用技術

- .NET 8.0
- WPF (Windows Presentation Foundation)
- MVVM パターン（CommunityToolkit.Mvvm使用）
- Microsoft.Extensions.DependencyInjection
- Microsoft.Xaml.Behaviors.Wpf

### プロジェクト構造

- `src/RpgTkoolSaveEditor/` - メインのWPFアプリケーション
- `src/RpgTkoolSaveEditor.Model/` - セーブデータのモデルクラス
- `src/RpgTkoolSaveEditor.Model.Tests/` - テストプロジェクト
- `installer/` - NSISインストーラーファイル

## ライセンス

このプロジェクトはMIT Licenseの下でライセンスされています。詳細については [LICENSE.txt](LICENSE.txt) を参照してください。

## 作者

huurou

## 更新履歴

### v1.1.0 (最新)
- RpgSaveが保存されていなかった問題を修正
- WeaponとArmorの一括変更個数を1から99に変更
- コンテキストメニューの表示改善
- 値入力中のロード時にフォーカスが外れる問題を修正

### v0.1.0
- 初期リリース