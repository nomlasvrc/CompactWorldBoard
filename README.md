# Compact World Board
[**VPM追加はこちら**](https://nomlasvrc.github.io/nomlas-package-listing/)  

VRChat ワールド向けの、コンパクトなワールド/インスタンス一覧ボードです。  
`VRCPortalMarker` を生成し、ポータルのサムネイルや表示テキストを取得して、ボタン状の一覧として表示します。

## 動作環境

- Unity 2022.3
- VRChat Worlds SDK

## 内容物

- `Runtime/CompactWorldBoard.prefab`
  - 基本となるボード Prefab です。
- `Runtime/Worlds.prefab`
  - 複数のワールドを一覧表示するためのボードです。
- `Runtime/Instances.prefab`
  - 1つのワールドに対して、A/B/C... のような連番インスタンスを一覧表示するためのボードです。

## 使い方

1. `Runtime/Worlds.prefab` または `Runtime/Instances.prefab` をシーンに配置します。
2. Inspector で表示したいワールド/インスタンス情報を設定します。


## 注意事項

- ポータル情報の取得は VRChat 側のポータル生成後に行われます。
- 本パッケージは VRChat のポータル表示構造を利用しています。VRChat SDK の仕様変更により表示取得やレイアウト調整が影響を受ける場合があります。

## ライセンス

[LICENSE](LICENSE) を確認してください。
