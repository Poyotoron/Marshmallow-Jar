# 設定画面

Marshmallow Jar の設定は 2 か所に分かれています。

| ページ | 役割 | 効く範囲 |
|---|---|---|
| **Preferences** の `Marshmallow Jar`（全体設定） | 各機能のオン・オフと完了通知の設定を決める | 全プロジェクト |
| **Project Settings** の `Marshmallow Jar` | 全体設定を、このプロジェクトだけ上書きする。プロジェクトごとの値（Hierarchy の位置など）もここにある | このプロジェクトだけ |

ふだんは全体設定でオン・オフを決め、特定のプロジェクトだけ変えたいときに Project Settings で上書きします。

!!! info "最初はすべてオフです"
    初めて 0.2.0 以降を入れたときは、全体設定がすべてオフの状態で作られます。他の拡張と動きがぶつからないよう、使う機能だけをオンにしてください。
    すでに全体設定がある PC では、別のプロジェクトを更新しても全体設定がそのまま使われます。

## 開き方

| 開き方 | 全体設定（Preferences） | このプロジェクト（Project Settings） |
|---|---|---|
| メニュー | `Tools > Marshmallow Jar > Preferences…` | `Tools > Marshmallow Jar > Project Settings…` |
| Project ウィンドウで右クリック | `Marshmallow Jar > Preferences…` | `Marshmallow Jar > Project Settings…` |
| Hierarchy で右クリック | `Marshmallow Jar > Preferences…` | `Marshmallow Jar > Project Settings…` |
| Unity の設定画面 | `Edit > Preferences > Marshmallow Jar` | `Edit > Project Settings > Marshmallow Jar` |

それぞれのページの先頭に、もう一方のページを開くボタンがあります。

チェックボックスを切り替えた時点で保存され、すぐに反映されます。「適用」ボタンはありません。

## 上書きのしくみ {#override}

Project Settings でチェックボックスを切り替えると、その項目は**このプロジェクトだけの値（上書き）**になります。
上書きした項目は、両方のページで色付きで表示されます。

| 表示 | 意味 |
|---|---|
| 色なし | 上書きしていない。全体設定に従う（Project Settings の右端に「全体設定のまま」） |
| 青 | 上書きしているが、値は全体設定と同じ |
| オレンジ | 上書きしていて、値が全体設定と違う |

- Project Settings では、上書きした行のラベルが太字になり、右端に **全体設定（ON）に戻す** / **全体設定（OFF）に戻す** が出ます（括弧内は全体設定の値）。押すと上書きが外れ、全体設定に従います。
- Preferences では、上書きされている行の右端に「このプロジェクトで上書き中」「このプロジェクトでは OFF」などと表示されます。比べる相手は、いま開いているプロジェクトです。
- 上書きは、全体設定と同じ値にしても外れません。外すのは「全体設定に戻す」だけです。

## エディタ機能

| 項目 | 説明 | 詳しく |
|---|---|---|
| Hierarchy に表示 / EditorOnly のチェックボックスを出す | 行の右端に 2 つのチェックボックスを出す。Project Settings の **右端からの距離**で位置を調整できる | [Hierarchy のチェックボックス](hierarchy.md) |
| Prefab Variant の名前に「Variant」を付けない | Variant を作ったとき、元と同じ名前で保存する | [Prefab Variant の名前](variant.md) |
| シーンのライティングとカメラを整える | 新しいシーンを推奨のライティングで作り、既存のシーンは確認してから変換する。Project Settings に **今のシーンに適用**・**確認しないシーンをリセット** のボタンがある | [シーンのライティング](scene-lighting.md) |
| VRChat SDK に名前とサムネイルを自動入力する | 初めてアップロードするアバターの名前とサムネイルを入れる | [VRChat SDK への自動入力](sdk-autofill.md) |
| Inspector に「別ウィンドウで開く」ボタンを出す | Inspector のヘッダーにボタンを出す | [Inspector を別ウィンドウで開く](inspector.md) |
| Inspector に「コピーを保存…」ボタンを出す | アセットの Inspector に、保存先を選んでコピーするボタンを出す | [コピーを保存](copy-asset.md) |
| Irodori Colorizer を使う | Project / Hierarchy の行に色付きのラベルを付ける | [Irodori Colorizer](colorizer.md) |

!!! note "VRChat SDK が入っていないと、自動入力の項目は選べません"
    VRChat SDK - Avatars 3.10.0 以降が入っていないプロジェクトでは、項目が灰色になり、必要条件が表示されます。

## 完了通知

アップロードなど時間のかかる処理が終わったときに、音と Webhook で知らせる機能です。
両方のページの末尾に設定があります。詳しくは[完了通知](notifications.md)を参照してください。

## Project Settings だけにある設定

次の値はプロジェクトごとのもので、全体設定はありません。

| 項目 | 説明 |
|---|---|
| Hierarchy の右端からの距離 | [Hierarchy のチェックボックス](hierarchy.md)の位置 |
| シーンのボタン | 今のシーンに適用 / 確認しないシーンをリセット |
| ブレンドシェイプのスライダーの下限 / 上限 | [ブレンドシェイプのスライダー](blendshape-sliders.md)の範囲。ウィンドウを開くボタンもここにある |
| Irodori Colorizer の設定 | `Marshmallow Jar` の下の **`Irodori Colorizer`** のページ（[Irodori Colorizer の設定](colorizer-settings.md)） |

単体版の Irodori Colorizer が入っているときは、Irodori Colorizer の欄に警告が表示されます（[単体版からの移行](colorizer-migration.md)）。

## 設定の保存先と共有

| 保存先 | 中身 | 共有 |
|---|---|---|
| Unity のエディタ設定（`EditorPrefs`） | 全体設定（各機能のオン・オフ、完了通知の音・Webhook など） | この PC のすべてのプロジェクトで共通 |
| `ProjectSettings/MarshmallowJar.asset` | オン・オフの上書き、確認しないシーンの一覧、Hierarchy の位置、スライダーの範囲 | バージョン管理で共有できる |
| `ProjectSettings/MarshmallowJar.IrodoriColorizer.asset` | Irodori Colorizer のラベルと、どこに何色を付けたか | バージョン管理で共有できる |
| `UserSettings/MarshmallowJar.asset` | 完了通知の音・Webhook・時間の上書き | 共有しない（Unity の標準の `.gitignore` で除外される） |

!!! warning "Webhook の URL は、知っていれば誰でも投稿できます"
    URL は全体設定か `UserSettings/` にだけ保存し、`ProjectSettings/` には書きません。
    `UserSettings/` をバージョン管理に含めている場合は、除外してください。
