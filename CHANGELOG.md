# CHANGELOG

Town Of Host-P-P(TownOfHost-Pko の非公式プライベートフォーク・身内専用)の変更履歴です。
日時はすべて JST。変更のたびにこのファイルの先頭側に追記します。

---

## 2026-09-30 13:16 JST — v1.9.1(ベース: TownOfHost-Pko v5.37.23.99)

### 追加(試作)
- **ナイスサイエンティスト (NiceScientist)** / クルー(`Roles/Crewmate/NiceScientist.cs`、ConfigId 40300、SuperNewRoles より)
  - ペットを撫でると、好きなタイミングで一定時間透明になれる。会議開始・死亡で解除
  - クルーはファントムの「消える」を持てないため、部屋主が自分以外の全員へ「消える/現れる」通信(StartVanish / StartAppear)を直接送る方式。部屋主の画面はその場で処理
  - MOD 未導入端末で消えて見えるかは未確認。見えない場合の予備として設定「透明中にファントム役職を送る(テスト用)」を用意(オンにするとインポスター画面で一瞬仲間表示になる恐れあり)
  - 設定: クールダウン(30秒) / 透明になれる時間(10秒) / 透明中にファントム役職を送る(オフ)
- `Roles/Core/CustomRoleManager.cs`: CustomRoles に NiceScientist を追加
- `Resources/string.csv`: 名前・説明・設定名(日本語/英語)を追加
- `README.md`: 追加役職一覧を更新
- `main.cs`: バージョンを `1.9.0` → `1.9.1`

## 2026-09-30 13:09 JST — v1.9.0(ベース: TownOfHost-Pko v5.37.23.99)

### 追加
- **イビルサイエンティスト (EvilScientist)** / インポスター(`Roles/Impostor/EvilScientist.cs`、ConfigId 11300、SuperNewRoles より、ファントム基盤)
  - 能力ボタン(ファントムの「消える」)で、好きなタイミングで一定時間透明になれる。透明中はキル不可(Among Us のファントムの仕様)
  - 設定: キルクール(30秒) / クールダウン(30秒) / 透明になれる時間(10秒、2.5〜60)
  - `Patches/PlayerContorols/RoleAbilityPatch.cs`: ファントムボタンを「ワンクリック能力」として横取りする処理から、イビルサイエンティストだけ除外して本来の透明化を通す
- `Roles/Core/CustomRoleManager.cs`: CustomRoles に EvilScientist を追加
- `Resources/string.csv`: 名前・説明・設定名(日本語/英語)を追加
- `README.md`: 追加役職一覧を更新
- `main.cs`: バージョンを `1.8.0` → `1.9.0`

## 2026-09-30 12:58 JST — v1.8.0(ベース: TownOfHost-Pko v5.37.23.99)

### 追加(TOH-P-P オリジナル役職)
- **記録係 (Recorder)** / クルー(`Roles/Crewmate/Recorder.cs`、ConfigId 39700)
  - 会議の最初に、前のターンで死んだ人の「死んだ順番」だけが本人に届く(誰がキルしたかは分からない)。死者がいないターンは通知なし
- **救急隊員 (Paramedic)** / クルー(`Roles/Crewmate/Paramedic.cs`、ConfigId 40000)
  - 自分で死体を通報すると、その人が死んでからの経過時間が「25〜30秒」のような幅で本人に届く
  - 設定: 時間の幅(5秒、1〜30)
- `Roles/Core/CustomRoleManager.cs`: CustomRoles に Recorder / Paramedic を追加
- `Resources/string.csv`: 2役職の名前・説明・設定名・メッセージ(日本語/英語)を追加
- `README.md`: 追加役職一覧を更新
- `main.cs`: バージョンを `1.7.0` → `1.8.0`

## 2026-09-30 11:45 JST — v1.7.0(ベース: TownOfHost-Pko v5.37.23.99)

### 追加(TOH-P-P オリジナル役職)
- **灯台守 (Lighthouse)** / クルー(`Roles/Crewmate/Lighthouse.cs`、ConfigId 39400)
  - ペットを撫でると、その瞬間に近くにいた人全員(自分も含む・インポスターも対象)の視界が数秒間広がる。停電中にも効く。会議開始で解除
  - 設定: クールダウン(20秒) / 視界が広がる時間(5秒) / 効果が届く距離(3) / 視界の倍率(2倍、1.25〜5、最大視界は5倍で頭打ち)
  - `Modules/GameOptionsSender/PlayerGameOptionsSender.cs`: プレイヤーごとの視界設定の最後に灯台守のブーストを適用
- `Roles/Core/CustomRoleManager.cs`: CustomRoles に Lighthouse を追加
- `Resources/string.csv`: 名前・説明・設定名(日本語/英語)を追加
- `README.md`: 追加役職一覧を更新
- `main.cs`: バージョンを `1.6.0` → `1.7.0`

## 2026-09-30 11:27 JST — v1.6.0(ベース: TownOfHost-Pko v5.37.23.99)

### 追加(TOH-P-P オリジナル役職)
- **ハイエナ (Hyena)** / インポスター(`Roles/Impostor/Hyena.cs`、ConfigId 10400)
  - 死体の近くにいる間キルクールが速く進む。死体から離れた時点で「近くにいた時間 × 倍率」だけ残りキルクールを減らす(0.5秒未満は無効、潜伏者と同じ方式)
  - 設定: キルクール(30秒) / 死体のそばでのキルクール進行倍率(3倍、1.5〜10) / 死体のそばとみなす距離(2、1〜5)
- **足止め屋 (Binder)** / インポスター(`Roles/Impostor/Binder.cs`、ConfigId 10700、シェイプシフター基盤)
  - シェイプシフトボタンで選んだ人を、その場で数秒動けなくする(変身はしない、仲間のインポスターは選べない)。会議開始で解除
  - 設定: キルクール(30秒) / クールダウン(25秒) / 足止めする時間(3秒、1〜10)
- `Roles/Core/CustomRoleManager.cs`: CustomRoles に Hyena / Binder を追加
- `Resources/string.csv`: 2役職の名前・説明・設定名(日本語/英語)を追加
- `README.md`: 追加役職一覧を更新
- `main.cs`: バージョンを `1.5.0` → `1.6.0`

## 2026-09-30 10:01 JST — v1.5.0(ベース: TownOfHost-Pko v5.37.23.99)

### 追加(TOH-P-P オリジナル役職)
- **ナイスボタナー (NiceButtoner)** / クルー(`Roles/Crewmate/NiceButtoner.cs`、ConfigId 38300)
  - ペットを撫でると、どこからでも緊急会議を開ける
  - 設定: 使える回数(1回、1〜5) / クールダウン(20秒、タスクターン開始からの経過時間)
- **イビルボタナー (EvilButtoner)** / インポスター(`Roles/Impostor/EvilButtoner.cs`、ConfigId 9700、ファントム基盤)
  - ワンクリックボタンで、どこからでも緊急会議を開ける
  - 設定: キルクール(30秒) / 使える回数(1回、1〜5) / クールダウン(20秒)
- どちらもリアクター・酸素・停電・通信妨害などのサボタージュ中は使えない(ゴーストボタナーと同じ条件)
- `Roles/Core/CustomRoleManager.cs`: CustomRoles に NiceButtoner / EvilButtoner を追加
- `Resources/string.csv`: 2役職の名前・説明・設定名(日本語/英語)を追加
- `README.md`: 追加役職一覧を更新
- `main.cs`: バージョンを `1.4.0` → `1.5.0`

## 2026-09-30 09:58 JST — v1.4.0(ベース: TownOfHost-Pko v5.37.23.99)

### 追加(TOH-P-P オリジナル役職)
- **目撃者 (Witness)** / クルー(`Roles/Crewmate/Witness.cs`、ConfigId 37600)
  - 自分の近く(設定距離内)でキルが起きると、次の会議開始時に「キラーの色が明るい色か暗い色か」を本人にだけ通知。カモフラージュ中は「分からない」
  - 設定: 目撃できる距離(3、1〜6) / 被害者の名前も伝える(オン)
- **カメラマン (Photographer)** / クルー(`Roles/Crewmate/Photographer.cs`、ConfigId 37900)
  - ペットを撫でると、その瞬間に近くにいた生存者と撮影場所(部屋名)を記録。次の会議開始時に写真を公開
  - 設定: クールダウン(15秒) / 撮影できる回数(3回) / 写る距離(3) / 写真を全員に公開する(オン、オフなら本人だけ)
- **ブックメーカー (Bookmaker)** / 第三陣営(`Roles/Neutral/Bookmaker.cs`、ConfigId 57400)
  - 会議で自投票→予想モード→次に投票した人を「追放される人」と予想(その票は数えない。予想後も普通に投票できる)
  - 予想した人が実際に追放されると的中。規定回数的中で単独勝利(人数判定ではクルー側)
  - 設定: 勝利に必要な的中回数(3回)
- `Roles/Core/CustomRoleManager.cs`: CustomRoles に Witness / Photographer / Bookmaker を追加
- `main.cs`: CustomWinner に Bookmaker を追加、バージョンを `1.3.3` → `1.4.0`
- `Resources/string.csv`: 3役職の名前・説明・設定名・メッセージ(日本語/英語)を追加
- `README.md`: 追加役職一覧を更新

## 2026-09-28 22:37 JST — v1.3.3(ベース: TownOfHost-Pko v5.37.23.99)

### 修正
- デバッグ版で「オプションIDが重複しています(ERR-001-010-3)」が出る不具合を修正
  - 原因: ジャッカルスターの ConfigId 52100 が、ジャッカルエイリアン(52000)の属性付与設定(ベース+100〜)と重複
  - ジャッカルスターの ConfigId を 52100 → **57100** に変更(設定は初期値に戻る)
- `main.cs`: バージョンを `1.3.2` → `1.3.3`

## 2026-09-28 22:33 JST — v1.3.2(ベース: TownOfHost-Pko v5.37.23.99)

### 修正
- v1.2.0〜v1.3.1 で MOD が読み込まれなかった不具合を修正
  - 原因: 役職の ConfigId が既存役職と重複し、起動時に例外(`An item with the same key has already been added. Key: 35000`)
  - スター 35000(占い系 Seer.cs と重複)→ **35100**、マッドスター 21500(守護天使の設定IDと重複)→ **21800**、ジャッカルスター 50000 → **52100**(念のため、他で使われている数値を避けた)
  - ConfigId が変わったため、この3役職の設定(確率など)は初期値に戻る
- `main.cs`: バージョンを `1.3.1` → `1.3.2`

## 2026-09-28 22:31 JST — v1.3.1(ベース: TownOfHost-Pko v5.37.23.99)

### 変更
- 「イビルスター」の名前を「**マッドスター**」(Mad Star)に変更。内部名も EvilStar → MadStar、ファイルを `Roles/Madmate/MadStar.cs` に。ConfigId(21500)は変更なしなので設定は引き継がれる
- `main.cs`: バージョンを `1.3.0` → `1.3.1`

## 2026-09-28 22:30 JST — v1.3.0(ベース: TownOfHost-Pko v5.37.23.99)

### 追加(TOH-P-P オリジナル、スターの別陣営版)
- **イビルスター (EvilStar)** / インポスター陣営・マッドメイト扱い(`Roles/Madmate/EvilStar.cs`、ConfigId 21500)
  - 能力なし・タスクなし。全員視点で名前が黄色。インポスター陣営の勝利で一緒に勝利。インポスターが誰かは分からない
- **ジャッカルスター (JackalStar)** / 第三陣営・ジャッカル陣営(`Roles/Neutral/Jackal/JackalStar.cs`、ConfigId 50000)
  - 能力なし・タスクなし。全員視点で名前が黄色。ジャッカル陣営の勝利で一緒に勝利。ジャッカルが誰かは分からない
  - 人数による勝敗判定ではクルー側として数える(マッドメイトと同じ扱い。能力のない役職だけでジャッカル陣営が人数勝利しないように)
  - ジャッカル系役職(ジャッカル/ジャッカルエイリアン/ジャッカルマフィア/ジャッカルウルフ/波動砲ジャッカル)が有効なときだけ配役
  - `Patches/CheckGameEndPatch.cs`: ジャッカル陣営勝利時にジャッカルスターを勝者に追加
- `Roles/Crewmate/Star.cs`: 名前を黄色で見せる判定をスター系3役職に拡張
- `Roles/Core/CustomRoleManager.cs`: CustomRoles に EvilStar / JackalStar を追加
- `Resources/string.csv`: 2役職の名前・説明(日本語/英語)を追加
- `README.md`: 追加役職一覧を更新
- `main.cs`: バージョンを `1.2.0` → `1.3.0`

## 2026-09-28 22:27 JST — v1.2.0(ベース: TownOfHost-Pko v5.37.23.99)

### 追加
- **スター (Star)** / クルーメイト(`Roles/Crewmate/Star.cs`、ConfigId 35000、SuperNewRoles より)
  - 能力なし。ゲーム開始時から全員視点で名前が黄色(#ffff00)に見える。MOD 未導入の参加者にも反映
  - `Modules/NameColorManager.cs`: 名前の色を役職色で見せる判定(KnowTargetRoleColor)にスターを追加
- `Roles/Core/CustomRoleManager.cs`: CustomRoles に Star を追加
- `Resources/string.csv`: スターの名前・説明(日本語/英語)を追加
- `README.md`: このフォークで追加した役職と出典を追記
- `main.cs`: バージョンを `1.1.2` → `1.2.0`

## 2026-09-26 21:51 JST — v1.1.2(ベース: TownOfHost-Pko v5.37.23.99)

### 変更
- フォーク元のマッチメイキング中継サーバー(Discord 募集連携)へ部屋情報を送らないように変更
  - `main.cs`: `MatchmakingRelayUrl` を `"none"` に。部屋を閉じたときに部屋コードが送信されていたのを確認したため
- `main.cs`: バージョンを `1.1.1` → `1.1.2`

## 2026-09-26 17:00 JST — v1.1.1(ベース: TownOfHost-Pko v5.37.23.99)

### 変更
- `/cmd` の一覧「--デバッグ用--」の `/cr` の説明を詳しく: 試合中は `/cr 役職 ID` で指定した人(IDなしなら自分)の役職を変更、ロビーでは `/cr 役職` で次の試合の自分の役職を予約
- `main.cs`: バージョンを `1.1.0` → `1.1.1`

## 2026-09-25 18:47 JST — v1.1.0(ベース: TownOfHost-Pko v5.37.23.99)

### 追加(TOH-P-P オリジナル役職)
- **潜伏者 (Lurker)** / インポスター(`Roles/Impostor/Lurker.cs`、ConfigId 8500)
  - ベントに潜っている間キルクールが速く進む。ベントから出た時点で「潜っていた時間 × 倍率」だけ残りキルクールを減らす(0.5秒未満の潜伏は無効)
  - 設定: キルクール(30秒) / ベント中のキルクール進行倍率(3倍、1.5〜10倍)
  - 残りキルクールはホスト側で「ゲーム開始10秒・会議後・自分のキル後」を起点に追跡して計算
- **口止め屋 (Silencer)** / インポスター(`Roles/Impostor/Silencer.cs`、ConfigId 8600、シェイプシフター基盤)
  - シェイプシフトボタンで1人を指定(変身はしない)。次の会議でその人の投票が0票になる。1会議1人まで
  - 口止めされた本人には会議開始時に必ず通知。設定がオンなら会議開始メッセージで全員にも公表
  - 設定: キルクール(30秒) / クールダウン(20秒) / 口止めできる回数(2回) / 口止めされた人を全員に公表する(オン)
- `Roles/Core/CustomRoleManager.cs`: CustomRoles に Lurker / Silencer を追加
- `Roles/Core/SimpleRoleInfo.cs`, `Modules/Utils/UtilsOption.cs`: 役職の出典に「Town Of Host-P-P」を追加
- `Resources/string.csv`: 2役職の名前・説明・設定名・メッセージ(日本語/英語)を追加
- `main.cs`: バージョンを `1.0.6` → `1.1.0`

## 2026-09-25 18:17 JST — v1.0.6(ベース: TownOfHost-Pko v5.37.23.99)

### 修正
- ローカル部屋で `/bot` のテストボットを入れてゲーム開始すると暗転する不具合を修正(フォーク元由来)
  - 原因: テストボットは試合用の PlayerState を持たないため、役職インスタンス生成(`CustomRoleManager.CreateInstance`)で NullReferenceException → 以降の役職処理が止まる。さらに毎フレーム処理(`FixedUpdatePatch`)で KeyNotFoundException が連発
  - `Roles/Core/CustomRoleManager.cs`: 役職インスタンス生成でテストボットを除外
  - `Patches/PlayerContorols/FixedUpatePatch.cs`: テストボットは毎フレーム処理をスキップ
- `main.cs`: バージョンを `1.0.5` → `1.0.6`

## 2026-09-25 18:11 JST — v1.0.5(ベース: TownOfHost-Pko v5.37.23.99)

### 変更
- `/cmd` のコマンド一覧を見やすく装飾
  - 区切り見出しを大きく太字にし、区切りごとに色分け(誰でも=青 / ゲッサー=黄土 / 秘匿チャット=ピンク / モデレーター用=緑 / 部屋主専用=オレンジ / デバッグ用=紫)
  - コマンド名を区切りの色+太字に、(試合中) などの補足を小さい灰色に
  - 1行1コマンドに整理
  - タイトル「【コマンド一覧】」を太字オレンジに
- `main.cs`: バージョンを `1.0.4` → `1.0.5`

## 2026-09-25 18:05 JST — v1.0.4(ベース: TownOfHost-Pko v5.37.23.99)

### 変更
- `/cmd` のコマンド一覧の「--デバッグ用--」にテストボットのコマンドを追加(`/bot`、`/bot hide`、`/bot snr`、`/bot snr cancel`、`/botmark`、`/botunmark`、`/botclear`)。ローカル部屋のみ・デバッグモード不要と明記
- `main.cs`: バージョンを `1.0.3` → `1.0.4`

## 2026-09-25 18:02 JST — v1.0.3(ビルド設定のみ・Mod の中身は変更なし)

### 変更
- ビルド後に dll を `BepInEx\plugins` へ自動コピーするように変更
  - `TownOfHost.csproj`: コピー処理を、Among Us 起動中で dll がロックされていてもビルド失敗にせず警告だけ出すように変更。コピー成功時はメッセージを表示
  - `TownOfHost.csproj.user`(新規・git 管理外): このPCの Among Us のパス(`...\Among Us - TownOfHost-P-P`)と `CopyToPlugins=true` を設定。`dotnet build -c Release` だけで plugins まで更新される
  - `.gitignore`: `*.csproj.user` を追加

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
