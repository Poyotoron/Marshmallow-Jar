# はじめに

**Marshmallow Jar** は、VRChat のアバター改変で **Unity エディタそのものを少し使いやすくする**小さな機能の詰め合わせです。

新しい道具を増やすのではなく、「毎回 Inspector に移って切り替える」「毎回名前を直す」といった、
エディタの細かい手間を減らすことを目的にしています。

## 収録している機能

| 機能 | 何が変わるか |
|---|---|
| [Hierarchy のチェックボックス](hierarchy.md) | Hierarchy の行の右端で、表示（Active）と EditorOnly を切り替えられる |
| [Prefab Variant の名前](variant.md) | Variant を作ったとき、名前の末尾に「Variant」が付かない |
| [シーンのライティング](scene-lighting.md) | アバターの色味を確かめやすいライティングでシーンを作る。既存のシーンは確認してから変換する |
| [VRChat SDK への自動入力](sdk-autofill.md) | 初めてアップロードするアバターの名前とサムネイルを自動で入れる（VRChat SDK が必要） |
| [Inspector を別ウィンドウで開く](inspector.md) | 選んでいるオブジェクトを固定した Inspector を、いくつでも並べて開ける |
| [コピーを保存](copy-asset.md) | 保存先と名前を選んで、マテリアルなどのアセットをコピーできる |
| [ブレンドシェイプのスライダー](blendshape-sliders.md) | 下限と上限を自由に決めたスライダーでブレンドシェイプを動かせる（開いたときだけ働く） |
| [完了通知](notifications.md) | アップロードやインポートなど、時間のかかる処理が終わったら音と Webhook で知らせる |
| [Irodori Colorizer](colorizer.md) | Project / Hierarchy の行に色付きのラベルを付ける。色で絞り込める |

導入した時点では、**どの機能もオフ**です。[設定画面](settings.md)の全体設定で、使う機能だけをオンにしてください。
オン・オフは全プロジェクトで共通にでき、プロジェクトごとに上書きもできます。オフにした機能は一切動きません。

## 共通する考え方

| 約束 | 内容 |
|---|---|
| 独自のコンポーネントを増やさない | アバターやシーンに Marshmallow Jar 専用のコンポーネントを付けることはありません |
| 実行時には何もしない | すべてエディタ専用です。アップロードしたアバターの中身には関与しません |
| 元に戻せる | 値の変更は「元に戻す（Undo）」で取り消せます（一部の例外は各ページに明記しています） |
| Assets を汚さない | 設定は Unity のエディタ設定・`ProjectSettings/`・`UserSettings/` に保存します。`Assets/` にファイルを作りません |
| 作業を止めない | 自動で出る確認は、エディタを止めない小さなウィンドウで出します |

!!! warning "重要なプロジェクトでは、事前のバックアップを推奨します"
    Undo で戻せる操作でも、Unity の再起動やスクリプトの再コンパイルをまたぐと戻せなくなります。
    大切な作業の前には、バージョン管理でのコミットやプロジェクトの複製をおすすめします。

## どこから読むか

| 目的 | ページ |
|---|---|
| 導入したい | [インストール](install.md) |
| 機能のオン・オフを切り替えたい | [設定画面](settings.md) |
| 表示や EditorOnly の切り替えのたびに Inspector に移るのが面倒 | [Hierarchy のチェックボックス](hierarchy.md) |
| Variant をシーンに置くと名前が「〇〇 Variant」になる | [Prefab Variant の名前](variant.md) |
| Unity で合わせた色味が、ワールドで見ると黄色っぽい | [シーンのライティング](scene-lighting.md) |
| 初回アップロードのたびに名前とサムネイルを入れている | [VRChat SDK への自動入力](sdk-autofill.md) |
| 2 つのオブジェクトの Inspector を見比べたい | [Inspector を別ウィンドウで開く](inspector.md) |
| マテリアルを別のフォルダや `Assets/` に複製したい | [コピーを保存](copy-asset.md) |
| ブレンドシェイプを 100 より大きく、または細かく動かしたい | [ブレンドシェイプのスライダー](blendshape-sliders.md) |
| アップロードが終わったら Discord で知りたい | [完了通知](notifications.md) |
| フォルダや GameObject を色で見分けたい | [Irodori Colorizer](colorizer.md) |
| 単体版の Irodori Colorizer を使っていた | [単体版からの移行](colorizer-migration.md) |
| うまく動かない | [困ったときは](troubleshooting.md) |
