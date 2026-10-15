# Changelog

このプロジェクトの変更履歴。バージョニングは [Semantic Versioning](https://semver.org/lang/ja/) に従う。

## [0.2.1] - 2026-10-15

Continuous Avatar Uploader の連続アップロードの通知が、完了ダイアログを閉じるまで届かなかった不具合を直した。
Webhook で送る内容を欄ごとに編集できるようにし、アセットのコピーを好きな場所に保存するボタンを追加した。

### Added
- **コピーを保存**: アセットの Inspector に `コピーを保存…` ボタンを出し、保存先と名前を選んでコピーを作れるようにした（`Inspector に「コピーを保存…」ボタンを出す`。最初はオフ）。`Packages/` の中のアセットも `Assets/` にコピーできる。同じ名前のファイルがあるときは上書きしない。
- **Webhook のかんたん編集**: 送る内容を、送信者名・アイコンの URL・メッセージ・タイトル・本文・フッターの欄ごとに編集できる。JSON の直接編集とはタブで切り替えられ、どちらで変えても同じ内容に反映される。JSON で書き足したほかの項目は、かんたん編集で欄を変えても残る。
- **差し込みボタン**: `{title}` `{message}` `{result}` `{project}` `{time}` をボタンで入れられる。マウスを乗せると、何に置き換わるかを表示する。

### Fixed
- **連続アップロードの通知**: Continuous Avatar Uploader の連続アップロードが終わったとき、完了ダイアログの OK を押すまで通知が届かなかった。最後のアップロードが終わった時点で、ダイアログより前に知らせるようにした。失敗で打ち切られたときも同じ。

## [0.2.0] - 2026-10-15

機能のオン・オフを全プロジェクト共通の全体設定で決め、プロジェクトごとに上書きできるようにした。他の拡張とぶつからないよう、最初はすべての機能をオフにした。
時間のかかる処理が終わったら音と Webhook で知らせる、完了通知を追加した。

### Added
- **全体設定**: `Tools > Marshmallow Jar > Preferences…`（Preferences の `Marshmallow Jar` ページ）で、各機能のオン・オフを全プロジェクト共通で決められる。
- **プロジェクトごとの上書き**: Project Settings の `Marshmallow Jar` ページで切り替えた項目は、そのプロジェクトだけの上書きになる。上書きした項目は両方のページで青（値は全体設定と同じ）またはオレンジ（値が違う）で表示し、「全体設定に戻す」で外せる。
- **完了通知**: VRChat へのアップロード（アバター・ワールド）、Continuous Avatar Uploader の連続アップロード、もちふぃった～ の衣装変換、Play モードへの切り替え、パッケージのインポートとスクリプトのコンパイルが終わったら、音と Webhook で知らせる。イベントごとに音・Webhook をオン・オフでき、Discord 向けのテンプレートを同梱した。
  - マルチプラットフォームのアップロードと連続アップロードは、すべて終わってから 1 回だけ知らせる。
  - Play モードとインポート・コンパイルは、設定した時間（既定 30 秒）より短く終わったときは知らせない。
  - 音・Webhook URL・本文のテンプレート・時間は、プロジェクトごとに上書きできる。上書きは共有されない `UserSettings/` に保存する。
- **メニュー**: Project / Hierarchy の右クリック `Marshmallow Jar` に `Preferences…` を追加した。

### Changed
- **最初はすべての機能がオフ**: 導入した時点では、どの機能も動かない。使う機能を全体設定でオンにする。
- **0.1.0 からの更新**: 0.1.0 でプロジェクトごとに保存していたオン・オフは引き継がず、全体設定に従う。全体設定がまだ無い PC では、すべてオフで作る。Hierarchy の位置、確認しないシーン、スライダーの範囲、Irodori Colorizer のラベルはそのまま残る。
- **Irodori Colorizer のオン・オフ**: Irodori Colorizer のページの「有効」をなくし、`Marshmallow Jar` のページの「Irodori Colorizer を使う」（全体設定と上書き）に移した。オフの間はラベルの右クリックメニューも無効になる。付けた色は消えない。
- **メニュー名**: `Settings…` を `Project Settings…` に改名した（`Tools` と右クリックの両方）。

## [0.1.0] - 2026-10-10

最初のリリース。Unity エディタの細かい手間を減らす機能と、単体で配布していた Irodori Colorizer をまとめた。
各機能は Project Settings の `Marshmallow Jar` ページで個別にオン・オフできる。

### Added
- **設定画面**: `Tools > Marshmallow Jar > Settings…`、または Project / Hierarchy の右クリック `Marshmallow Jar > Settings…` から開く。
- **Hierarchy のチェックボックス**: 行の右端で表示（Active）と EditorOnly タグを切り替えられる。複数選択でまとめて切り替えられ、元に戻すにも対応する。独自のタグが付いた行では EditorOnly を切り替えない。他の拡張のボタンと重ならないよう、右端からの距離を調整できる。
- **Prefab Variant の名前**: Variant を作ったとき、末尾の「Variant」を外した名前で保存する。同じフォルダに同じ名前の Prefab があるときは付けたままにする。
- **シーンのライティング**: 新しいシーンを、アバターの色味を確かめやすいライティング（無彩色の環境光、白い Directional Light）と視野角 40 の Scene ビューで作る。既存のシーンは、開いたときに確認ウィンドウを出してから変換する。確認は `Tools > Marshmallow Jar > Scene Lighting` からも選べる。
- **VRChat SDK への自動入力**: 初めてアップロードするアバターの名前とサムネイルが空なら、アバターの名前と Scene ビューの画像を入れる。VRChat SDK - Avatars 3.10.0 以降が必要。
- **Inspector を別ウィンドウで開く**: Inspector のヘッダーのボタンから、選んでいるオブジェクトを固定したウィンドウを開く。
- **ブレンドシェイプのスライダー**: 下限と上限を決めたスライダーでブレンドシェイプを動かすウィンドウ。検索と「0 以外」の絞り込みに対応し、元に戻す、Prefab の override、Animation の記録に乗る。
- **Irodori Colorizer**: Project / Hierarchy の行に色付きのラベルを付ける機能を収録した。右クリックメニューは `Marshmallow Jar` の下に移り、設定は `Project Settings > Marshmallow Jar > Irodori Colorizer` に移った。
- **Irodori Colorizer の色で絞り込む**: 選んだ色のラベルが付いたアセットと、そのフォルダの中身・親フォルダ以外の行を、Project ウィンドウで暗く表示する。

### Changed
- **単体版の Irodori Colorizer からの移行**: 単体版の設定を自動で引き継ぐ。単体版が入っている間は、二重に色が付かないよう Marshmallow Jar 側の色付けを止め、Console と設定画面で単体版の削除を案内する。

### Fixed
- **Irodori Colorizer**: Packages 内のパッケージのフォルダに色を付けると、選択していないときにパッケージの表示名ではなくフォルダ名が表示されていたのを直した。
