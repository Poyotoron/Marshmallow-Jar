# 設定画面

Marshmallow Jar の設定は **Project Settings** にまとめてあります。次のどれからでも開けます。

| 開き方 | 場所 |
|---|---|
| メニュー | `Tools > Marshmallow Jar > Settings…` |
| Project ウィンドウで右クリック | `Marshmallow Jar > Settings…` |
| Hierarchy で右クリック | `Marshmallow Jar > Settings…` |
| Project Settings | `Edit > Project Settings > Marshmallow Jar` |

チェックボックスを切り替えた時点で保存され、すぐに反映されます。「適用」ボタンはありません。

## エディタ機能

| 項目 | 説明 | 詳しく |
|---|---|---|
| Hierarchy に表示 / EditorOnly のチェックボックスを出す | 行の右端に 2 つのチェックボックスを出す。**右端からの距離**で位置を調整できる | [Hierarchy のチェックボックス](hierarchy.md) |
| Prefab Variant の名前に「Variant」を付けない | Variant を作ったとき、元と同じ名前で保存する | [Prefab Variant の名前](variant.md) |
| シーンのライティングとカメラを整える | 新しいシーンを推奨のライティングで作り、既存のシーンは確認してから変換する。**今のシーンに適用**・**確認しないシーンをリセット**のボタンもここにある | [シーンのライティング](scene-lighting.md) |
| VRChat SDK に名前とサムネイルを自動入力する | 初めてアップロードするアバターの名前とサムネイルを入れる | [VRChat SDK への自動入力](sdk-autofill.md) |
| Inspector に「別ウィンドウで開く」ボタンを出す | Inspector のヘッダーにボタンを出す | [Inspector を別ウィンドウで開く](inspector.md) |

!!! note "VRChat SDK が入っていないと、自動入力の項目は選べません"
    VRChat SDK - Avatars 3.10.0 以降が入っていないプロジェクトでは、項目が灰色になり、必要条件が表示されます。

## ブレンドシェイプのスライダー

チェックボックスはありません。ウィンドウを開いたときだけ働く機能です。

| 項目 | 説明 |
|---|---|
| 下限 / 上限 | スライダーの範囲。ウィンドウ側の範囲と同じ値です |
| `0〜100` | 範囲を 0〜100 に戻す |
| ウィンドウを開く | [ブレンドシェイプのスライダー](blendshape-sliders.md)を開く |

## Irodori Colorizer

色付け機能の設定は、`Marshmallow Jar` の下にある **`Irodori Colorizer`** のページにあります。
`Irodori Colorizer の設定を開く` ボタンでも移動できます。中身は[Irodori Colorizer の設定](colorizer-settings.md)を参照してください。

単体版の Irodori Colorizer が入っているときは、ボタンの代わりに警告が表示されます（[単体版からの移行](colorizer-migration.md)）。

## 設定の保存先と共有

| ファイル | 中身 |
|---|---|
| `ProjectSettings/MarshmallowJar.asset` | エディタ機能のオン・オフ、確認しないシーンの一覧、スライダーの範囲など |
| `ProjectSettings/MarshmallowJar.IrodoriColorizer.asset` | Irodori Colorizer のラベルと、どこに何色を付けたか |

どちらもプロジェクト単位の設定です。バージョン管理で共有すれば、同じプロジェクトを開いた全員に同じ設定が適用されます。
