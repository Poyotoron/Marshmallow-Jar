# Prefab Variant の名前

Unity は Prefab Variant を作ると、ファイル名の末尾に ` Variant` を付けます（例: `Kikyo Variant.prefab`）。
Prefab をシーンに置いたときの名前はファイル名と同じになるため、Hierarchy や VRChat SDK の画面にも「〇〇 Variant」と出てしまいます。

この機能をオンにしていると、Variant を作った直後に **` Variant` を外した名前**へ自動で変更します。

## 名前が変わる条件

次をすべて満たすときだけ、名前を変えます。

| 条件 | 例 |
|---|---|
| 新しく作った Prefab Variant である | 作成済みの Variant を編集・保存しても変わりません |
| 名前が「元の名前 + ` Variant`」のままである | 作るときに自分で名前を入力した場合は変わりません |
| 同じフォルダに、同じ名前の Prefab がない | 下記を参照 |

## 同じフォルダに同じ名前があるとき

`Create > Prefab Variant` は、元の Prefab と**同じフォルダ**に Variant を作ります。
元が `Kikyo.prefab` だと、` Variant` を外した `Kikyo.prefab` は元のファイルと同じ名前になってしまいます。

この場合は名前を変えず、**`Kikyo Variant.prefab` のまま**にします。連番を付けたり、別のフォルダへ移したりはしません。

| 元 | 作った場所 | 結果 |
|---|---|---|
| `Kikyo.fbx`（モデル） | 同じフォルダ | `Kikyo.prefab` |
| `Kikyo.prefab` | 同じフォルダ | `Kikyo Variant.prefab` のまま |
| `Kikyo.prefab` | 別のフォルダ | `Kikyo.prefab` |

!!! tip "元と同じ名前で使いたいときは、別のフォルダに作ってください"
    元の Prefab とは別のフォルダに Variant を作れば名前が衝突しないので、` Variant` を外した名前になります。

## 元に戻す

名前の変更は「元に戻す（Undo）」の対象外です。名前を戻したい場合は、Project ウィンドウで手動で改名してください。

## オフにする

[設定画面](settings.md)の「Prefab Variant の名前に「Variant」を付けない」を外すと、Unity の標準どおり ` Variant` が付きます。
