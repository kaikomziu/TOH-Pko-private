# CHANGELOG

Town Of Host-P-P(TownOfHost-Pko の非公式プライベートフォーク・身内専用)の変更履歴です。
日時はすべて JST。変更のたびにこのファイルの先頭側に追記します。

---

## 2026-09-25 08:30 JST — P-P 0.1.0(ベース: TownOfHost-Pko v5.37.23.99 / upstream `0c8b7a0f`)

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
