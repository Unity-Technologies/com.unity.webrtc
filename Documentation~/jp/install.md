# パッケージのインストール

> [!NOTE]
> Unity 2020.1 と 2019.4 では WebRTC パッケージのインストール方法が異なるため、利用するエディタに応じて、以下の手順に従ってください。

## Unity 2019.4 の場合

メニューバーから `Window/Package Manager` を選択します。

![Install Package Manager from menu bar](../images/install_select_packman_menu_unity2019.png)

Package Manager ウィンドウに移動し、`Advanced` ボタンをクリックして、`Show preview packages` を有効にします。

![Select show preview packages on advanced options](../images/install_select_show_preview_packages.png)

Package Manager ウィンドウ上部にある検索ボックスに `webrtc` と入力します。

![Search webrtc package](../images/install_search_webrtc_package.png)

画面右下の `Install` ボタンを押すと、インストールが開始されます。

## Unity 2020.1 の場合

メニューバーから `Window/Package Manager` を選択します。

![Install Package Manager from menu bar](../images/install_select_packman_menu_unity2020.png)

Package Manager ウィンドウに移動して、左上の `+` ボタンを押し、`Add package from git URL...` を選択します。

![Select add package from git url](../images/install_select_add_package_from_git_url.png)

入力ボックスに以下の文字列を追加します。

```
com.unity.webrtc@2.1.0-preview
```

![Input webrtc package git URL](../images/install_input_webrtc_git_url.png);

`Add` ボタンを押すと、インストールが開始されます。
