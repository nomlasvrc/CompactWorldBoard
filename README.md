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
2. Inspector の「ボードを編集」から、表示したいワールド/インスタンス情報を設定します。

## 編集画面

Unity 2022.3のUI Toolkitを使用した、フラットで高密度な編集画面です。Unityのライト／ダークテーマに対応し、背景の階調・hover・選択状態で操作対象を区別します。

- Inspectorの「ボードを編集」、または `Tools > Compact World Board > Worlds Board / Instances Board` から編集ウィンドウを開きます。
- Worlds Boardの「ワールド」タブは、左にお気に入り、右にワールド一覧を表示します。中央の区切りをドラッグして幅を調整できます。「インスタンス設定」は別タブで、インスタンスID・アクセス範囲・リージョンをまとめて設定します。
- Instances BoardではワールドID・インスタンスID一覧・アクセス範囲・リージョンを同じウィンドウで編集します。
- ワールドID、または `https://vrchat.com/home/world/wrld_…` のURLを入力し、「追加」かEnterで登録します。
- 「情報を取得」で名前・作者・定員を取得し、名前・作者・IDで検索できます。
- お気に入りとワールド一覧にはサムネイルを表示します。画像の取得に失敗した場合は、画像欄をクリックして再試行できます。
- 行のIDはEnterまたはフォーカス移動で確定します。重複・形式エラーは画面に表示されます。
- ↑／↓で並べ替え、削除や編集はUnityのUndo／Redoで戻せます。
- お気に入りの取得にはVRChat SDKへのログインが必要です。ドロップダウン右の更新ボタンでグループを読み込み、選択後に「ワールドを取得」を押します。
- Inspectorには概要・ポータル設定・Prefab内部の参照を常時表示します。折りたたみ操作はありません。
- バージョンはヘッダーにコンパクトに表示します。更新がある場合はアクセントで知らせ、クリックまたはEnterでパッケージ一覧を開けます。

デザインの共通スタイルは `Editor/BoardUI.uss` にまとめています。

## 注意事項

- ポータル情報の取得は VRChat 側のポータル生成後に行われます。
- 本パッケージは VRChat のポータル表示構造を利用しています。VRChat SDK の仕様変更により表示取得やレイアウト調整が影響を受ける場合があります。

## ライセンス

[LICENSE](LICENSE) を確認してください。
