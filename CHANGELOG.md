# CHANGELOG

Town Of Host-P-P(TownOfHost-Pko の非公式プライベートフォーク・身内専用)の変更履歴です。
日時はすべて JST。変更のたびにこのファイルの先頭側に追記します。

---

## 2026-09-25 17:58 JST — v1.0.3(ベース: TownOfHost-Pko v5.37.23.99)

### 変更
- `/cm`(会議画面の再送)をモデレーターも使えるように変更(部屋主・モデレーター専用)
  - `/cm` で送信者と部屋主以外の全員、`/cm ID` で指定した1人に再送。会議中のみ。実行後に再送した人数を返す
  - それ以外の人には「/cm は部屋主・モデレーター専用です」と返す
  - `/cmd` の一覧では「--モデレーター用--」の区切りに移動
- `main.cs`: バージョンを `1.0.2` → `1.0.3`

## 2026-09-25 17:57 JST — v1.0.2(ベース: TownOfHost-Pko v5.37.23.99)

### 変更
- `/cmd` のコマンド一覧を「--誰でも--」「--ゲッサー--」「--秘匿チャット--」「--モデレーター用--」「--部屋主専用--」「--デバッグ用--」で区切る形に変更
  - モデレーター用: 部屋主とモデレーターにだけ表示
  - 部屋主専用・デバッグ用: 部屋主にだけ表示
- `/cm`(会議画面の再送)を部屋主専用に変更。本家では誰でも自分宛てに会議開始 RPC を送らせることができたため
  - 部屋主: `/cm` で部屋主以外の全員、`/cm ID` で指定した1人に再送。会議中のみ
  - 部屋主以外: 「/cm は部屋主専用です」と返す
- `main.cs`: バージョンを `1.0.1` → `1.0.2`

## 2026-09-25 17:54 JST — v1.0.1(ベース: TownOfHost-Pko v5.37.23.99)

### 追加
- チャットで `/cmd` だけを送るとコマンド一覧を表示(`Patches/ChatCommandPatch.cs`)
  - 部屋主: 「誰でも使える」+「部屋主のみ」の一覧が自分にだけ表示される
  - MOD 導入済みの参加者: 自分の画面にだけ表示される
  - MOD 未導入の参加者(スマホ等): 部屋主の MOD が本人にだけ送る

### 変更
- `main.cs`: バージョンを `1.0.0` → `1.0.1`

## 2026-09-25 17:48 JST — v1.0.0(ベース: TownOfHost-Pko v5.37.23.99)

Mod のバージョン番号をフォーク独自の番号に切り替え。以後この CHANGELOG の見出しバージョン = ゲーム内表示バージョン。

### 変更
- `main.cs`: `BepInExPluginVersion` / `PluginVersion` / `PluginShowVersion` を `5.37.23.99` → `1.0.0` に変更(ゲーム内表示は v1.0.0)
- `Modules/Statistics.cs`: 保存済み統計のバージョンが自分より新しいと統計・実績の保存を止める判定を無効化(本家 5.x で保存されたデータで誤判定しないため)

### 補足
- 部屋主と参加者のバージョン一致判定は ForkId + バージョンで行われるので、参加者も同じ v1.0.0 の dll を入れる必要がある
- フォーク元 `versions.json` に 1.0.0 の項目は無いため、バージョン別の遠隔設定(告知・停止フラグ等)は適用されなくなる

## 2026-09-25 17:36 JST — P-P 0.1.1(ベース: TownOfHost-Pko v5.37.23.99)

フォーク元 Wiki「Forkについて」のお願い(ゲーム内の TownOfHost-Pko / TOH-P 表記の変更、どの Mod か区別できるようにする)への対応。

### 変更
- タイトル画面: ロゴ下に「Town Of Host-P-P / TownOfHost-Pko 非公式プライベートフォーク」を表示(`Patches/CredentialsPatch.cs`)。ロゴ画像自体は未変更
- ロビー: 参加時・welcome テンプレートのチャット見出しを「This Room Use "Town Of Host-P-P" (Unofficial private fork of TownOfHost-Pko)」に変更(`Patches/PlayerJoinAndLeftPatch.cs`, `Modules/TemplateManager.cs`)
- ロビー: タイトル文字、MOD設定ボタン名を `Main.ModName` に変更(`Patches/LobbyImageDisplayPatch.cs`, `Patches/GameOptions/MenuPatch.cs`)
- ゲーム中/終了画面: 名前下のバージョン表記・非クライアント向け表記・勝利画面の Mod 名を P-P 表記に変更(`Modules/Utils/Utils.cs`, `Modules/Utils/UtilsNotifyRoles.cs`, `Patches/CheckGameEndPatch.cs`)
- デバッグ版の告知: 「TOH-Pko の Discord で報告」→「フォーク元には報告せず部屋主に伝える」に変更
- `Resources/string.csv`: 表示文言中の TOH-P / TOWNOFHOST-PKO を TOH-P-P / TOWNOFHOST-P-P に置換(翻訳キーは変更なし)
- `main.cs`: BepInPlugin の表示名を `Town Of Host-P-P` に、配信者モードの伏せ名の既定値を `Town Of Host-P-P` に(GUID は変更なし)
- `Patches/MainMenuManagerPatch.cs`: 更新詳細ポップアップのタイトル表記を TOH-P-P に

### 修正
- `Modules/ModUpdater.cs`: 0.1.0 で文字列リテラル内に改行が入りコンパイルエラーになる箇所を修正

## 2026-09-25 17:30 JST — P-P 0.1.0(ベース: TownOfHost-Pko v5.37.23.99 / upstream `0c8b7a0f`)

プライベートフォークとしての初期セットアップ。役職の追加・改造はまだ行っていません。

### 変更
- `main.cs` プログラム設定をプライベートフォーク用に変更
  - `ModName`: `Town Of Host-Pko` → `Town Of Host-P-P`
  - `ModColor`: `#FF9631` → `#ff8c00`
  - `AllowPublicRoom`: `true` → `false`(公開ルームを建てられないようにする)
  - `ForkId`: `TOH-PKO` → `TOH-Pko-PrivateVer`
  - `ShowDiscordButton`: `true` → `false`(フォーク元 Discord への導線を非表示)
  - `BepInPlugin` の GUID(`com.satokazoku.TownOfHost-Pko`)は変更なし
- `Modules/ModUpdater.cs` 自動アップデートを無効化
  - `UpdateCheckEnabled = false` を追加
  - `CheckRelease()` はフォーク元 GitHub Releases API へアクセスせず「更新なし」で即 return
  - `StartUpdate()` は何もしない(フォーク元の公式 dll で上書きされるのを防止)
- `Modules/VersionInfoManager.cs` フォーク元 `versions.json` による更新ボタン表示・リリース取得を、`UpdateCheckEnabled` が false のときは行わない
- `TownOfHost.csproj` ビルド後に `BepInEx\plugins` へ dll を自動コピーする処理を既定で無効化(`-p:CopyToPlugins=true` 指定時のみ実行)
- `README.md` 冒頭にプライベートフォークである旨とフォーク元リンクを追記、フォーク元 Discord 招待リンクを削除(クレジット欄は維持)
- `README.EN.md` Discord 招待リンク(TOHK のもの)を削除
- `CHANGELOG.md` を新規作成
