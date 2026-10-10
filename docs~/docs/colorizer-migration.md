# 単体版からの移行

単体のパッケージとして配布していた Irodori Colorizer は、Marshmallow Jar に含まれるようになりました。
単体版を使っていたプロジェクトでは、次の手順で移行してください。

## 手順

1. Marshmallow Jar を導入します（[インストール](install.md)）。
2. VCC / ALCOM で、プロジェクトから **Irodori Colorizer（単体版）を削除**します。
3. Unity に戻ると、Marshmallow Jar 側の色付けが動き始めます。

**付けた色、独自のラベル、プリセットのオン・オフなどの設定は、自動で引き継がれます。** 付け直す必要はありません。

## 単体版が入っている間の動き

両方が入っていると、同じ行に二重に色が付いてしまいます。それを防ぐため、
単体版が入っている間は **Marshmallow Jar 側の色付けを止め**、単体版だけが動きます。

| 場所 | 表示 |
|---|---|
| Console | エディタを起動するたびに 1 回、単体版を削除するよう案内が出ます |
| [設定画面](settings.md)の Irodori Colorizer 欄 | 同じ内容の警告が表示されます |
| 右クリックメニュー | 単体版の `Irodori Colorizer` の項目だけが出ます |

## 変わったところ

| 項目 | 単体版 | Marshmallow Jar |
|---|---|---|
| 右クリックメニュー | `Irodori Colorizer > Set Label…` | `Marshmallow Jar > Set Label…`（階層の深さは同じ） |
| 設定画面 | `Project Settings > Irodori Colorizer` | `Project Settings > Marshmallow Jar > Irodori Colorizer` |
| 設定の保存先 | `ProjectSettings/IrodoriColorizer.asset` | `ProjectSettings/MarshmallowJar.IrodoriColorizer.asset` |
| 追加された機能 | — | [色で絞り込む](colorizer-filter.md) |
| 修正 | — | Packages 内のパッケージのフォルダに色を付けたとき、パッケージの表示名で表示されるようにした |

ラベルの種類や色の付き方、設定の項目は単体版と同じです。

## 引き継ぎについて

単体版を削除したあと、Marshmallow Jar が初めて色付けの設定を読むときに、
`ProjectSettings/IrodoriColorizer.asset` の内容を `ProjectSettings/MarshmallowJar.IrodoriColorizer.asset` へ写します。
成功すると、Console に「単体版の設定を引き継ぎました。」と出ます。

!!! note "元のファイルは残ります"
    `ProjectSettings/IrodoriColorizer.asset` は消さずに残します。引き継ぎが済んだら、不要であれば削除して構いません。
    引き継ぎは、新しいファイルがまだ無いときに 1 回だけ行います。

!!! warning "チームで共有している場合は、新しいファイルも共有してください"
    色の情報は新しいファイル `ProjectSettings/MarshmallowJar.IrodoriColorizer.asset` に入ります。
    バージョン管理の対象に含めてください。
