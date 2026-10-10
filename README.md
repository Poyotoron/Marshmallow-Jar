# Marshmallow Jar

VRChat のアバター改変で、Unity エディタそのものを少し使いやすくする小さな機能の詰め合わせです。
機能ごとにオン・オフできます。

詳しい使い方は[ドキュメントサイト](https://ghp.maaaaa.net/Marshmallow-Jar/)をご覧ください。

## 動作環境

- Unity 2022.3
- エディタ専用
- 必須の依存パッケージはありません。「VRChat SDK への自動入力」だけは VRChat SDK - Avatars 3.10.0 以降（4.0.0 未満）が必要です。

## 導入

VCC / ALCOM に配布元のリポジトリを追加し、プロジェクトに「Marshmallow Jar」を追加します。
VCC を使っていない場合は、[リリースページ](https://github.com/Poyotoron/Marshmallow-Jar/releases)から `.unitypackage` をダウンロードしてインポートしてください。

## 設定

`Tools > Marshmallow Jar > Settings…`（または Project / Hierarchy の右クリック `Marshmallow Jar > Settings…`）で、
Project Settings の `Marshmallow Jar` ページが開きます。各機能はここでオン・オフできます。

## 収録機能

| 機能 | 内容 |
|---|---|
| Hierarchy のチェックボックス | Hierarchy の行の右端で、表示（Active）と EditorOnly タグを切り替えられます。位置は設定で調整できます。 |
| Prefab Variant の名前 | Variant を作ったとき、名前の末尾に「Variant」を付けずに保存します。同じフォルダに同じ名前の Prefab があるときは付けたままにします。 |
| シーンのライティング | 新しいシーンを、アバターの色味を確かめやすいライティングとカメラで作ります。既存のシーンは、開いたときに確認してから変換します。 |
| VRChat SDK への自動入力 | 初めてアップロードするアバターの名前とサムネイルが空なら、アバターの名前と Scene ビューの画像を入れます。 |
| Inspector を別ウィンドウで開く | 選んでいるオブジェクトを固定した Inspector を、別ウィンドウでいくつでも開けます。 |
| ブレンドシェイプのスライダー | 下限と上限を自由に決めたスライダーで、ブレンドシェイプを動かせるウィンドウです。 |
| Irodori Colorizer | Project / Hierarchy の行に色付きのラベルを付けます。Project ウィンドウを色で絞り込めます。 |

自動で出る確認は、エディタを止めない小さなウィンドウで表示します。同じ操作はメニューからも選べます。

## 単体版の Irodori Colorizer から移行する

単体版が入っている間は、二重に色が付かないよう Marshmallow Jar 側の色付けが止まります。
VCC / ALCOM で単体版を削除すると、付けた色と設定が自動で引き継がれます。

## ライセンス

[MIT License](LICENSE)
