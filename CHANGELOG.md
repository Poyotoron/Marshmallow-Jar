# Changelog

このプロジェクトの変更履歴。バージョニングは [Semantic Versioning](https://semver.org/lang/ja/) に従う。

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
