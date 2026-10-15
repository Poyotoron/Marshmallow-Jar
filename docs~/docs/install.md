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
Tools > Marshmallow Jar > Preferences…
Tools > Marshmallow Jar > Project Settings…
```

`Preferences…` で全プロジェクト共通の設定（全体設定）が、`Project Settings…` でこのプロジェクトの設定が開きます。

!!! info "導入した時点では、すべての機能がオフです"
    他の拡張と動きがぶつからないよう、最初はどの機能も動きません。
    `Preferences…` を開き、使いたい機能をオンにしてください。一度オンにすると、この PC の他のプロジェクトでもオンになります。
    詳しくは[設定画面](settings.md)を参照してください。

## 単体版の Irodori Colorizer を使っている場合

Marshmallow Jar には Irodori Colorizer が含まれています。単体版が入ったままだと、
二重に色が付かないよう **Marshmallow Jar 側の色付けが止まります。**
VCC / ALCOM で単体版を削除してください。付けた色と設定はそのまま引き継がれます。

手順と注意点は[単体版からの移行](colorizer-migration.md)を参照してください。

## 更新する {#update}

VCC / ALCOM から更新するか、新しい `.unitypackage` をインポートしてください。
設定は `ProjectSettings/` などに保存されているため、更新しても保たれます。

!!! warning "0.1.0 から 0.2.0 に更新すると、機能のオン・オフは全体設定に従います"
    0.2.0 から、オン・オフは全プロジェクト共通の全体設定で決めるようになりました。
    0.1.0 でプロジェクトごとに保存していたオン・オフは引き継がれません。

    - **この PC で初めて 0.2.0 に更新したとき**は、全体設定がすべてオフで作られます。`Tools > Marshmallow Jar > Preferences…` で、使う機能をオンにしてください。
    - **2 つ目以降のプロジェクト**は、その全体設定に従います。もう一度オンにし直す必要はありません。

    Hierarchy の位置、確認しないシーン、スライダーの範囲、Irodori Colorizer のラベルはそのまま残ります。
