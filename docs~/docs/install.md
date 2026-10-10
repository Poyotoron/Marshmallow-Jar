# インストール

## 動作環境

| 項目 | 要件 |
|---|---|
| Unity | 2022.3 (LTS) |
| VRChat SDK | 不要（[VRChat SDK への自動入力](sdk-autofill.md)だけは、VRChat SDK - Avatars 3.10.0 以降 4.0.0 未満が必要） |
| 対応 | エディタ専用。実行時のアセンブリを持ちません |

!!! note "パッケージ自体には必須の依存パッケージがありません"
    VRChat SDK が入っていないプロジェクトでも問題なく動きます。
    その場合は「VRChat SDK への自動入力」だけが使えない状態になり、設定画面でも選べなくなります。

## VCC / ALCOM から導入する（推奨）

1. VCC または ALCOM に、配布元のリポジトリを追加します。
2. 対象の Unity プロジェクトを開き、**Manage Packages** を選びます。
3. 一覧から「Marshmallow Jar」を追加します。

## unitypackage から導入する

VCC を使っていない場合は、[リリースページ](https://github.com/Poyotoron/Marshmallow-Jar/releases)から
`.unitypackage` をダウンロードし、Unity のプロジェクトへドラッグ＆ドロップしてインポートしてください。

## 導入できたか確認する

Unity のメニューに次の項目が増えていればインストール成功です。

```
Tools > Marshmallow Jar > Settings…
```

クリックすると、Project Settings の `Marshmallow Jar` ページが開きます。

!!! info "導入した時点で、ほとんどの機能がオンになっています"
    Hierarchy のチェックボックスなどは、導入してすぐに表示されます。
    不要なものは[設定画面](settings.md)でオフにしてください。

## 単体版の Irodori Colorizer を使っている場合

Marshmallow Jar には Irodori Colorizer が含まれています。単体版が入ったままだと、
二重に色が付かないよう **Marshmallow Jar 側の色付けが止まります。**
VCC / ALCOM で単体版を削除してください。付けた色と設定はそのまま引き継がれます。

手順と注意点は[単体版からの移行](colorizer-migration.md)を参照してください。

## 更新する

VCC / ALCOM から更新するか、新しい `.unitypackage` をインポートしてください。
設定は `ProjectSettings/` に保存されているため、更新しても保たれます。
