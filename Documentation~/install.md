# Install package

> [!NOTE]
> In Unity `2020.3` and `2019.4`, there are differences about how to install WebRTC package so please pay attention to the Unity version you are using and follow the instructions below.

## Case of Unity 2019.4

Select `Window/Package Manager` in the menu bar.

![Install Package Manager from menu bar](images/install_select_packman_menu_unity2019.png)

Check Package Manager window, Click `Advanced` button and enable `Show preview packages`.

![Select show preview packages on advanced options](images/install_select_show_preview_packages.png)

Input `webrtc` to the search box at the top of the Package Manager window.

![Search webrtc package](images/install_search_webrtc_package.png)

Click `Install` button at the bottom left of the window, and will start install the package.

## Case of Unity 2020.3

Select `Window/Package Manager` in the menu bar.

![Install Package Manager from menu bar](images/install_select_packman_menu_unity2020.png)

Check Package Manager window, Click `+` button and select `Add package from git URL...`.

![Select add package from git url](images/install_select_add_package_from_git_url.png)

Input the string below to the input field.

```
com.unity.webrtc@2.4.0-exp.4
```

The list of version string is [here](https://github.com/Unity-Technologies/com.unity.webrtc/tags). In most cases, the latest version is recommended to use.

![Input webrtc package git URL](images/install_input_webrtc_git_url.png)

 Click `Add` button, and will start install the package.