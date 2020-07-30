# WebRTC 統計情報ツール

エディタ上で WebRTC に関する統計情報を表示するツールを提供しています。

## 使用方法

上部ツールバーの Window -> Analysis -> WebRTC Stats から開くことができます。

![Guidline WebRTC Stats](../images/guideline-webrtc-stats.png)


開くと下記のような画面が出ます。

![WebRTC Stats Empty](../images/webrtc-stats_emptyview.png)


Unity エディタでシーンを実行します。 PeerConnection が生成されると、左側のリストに PeerConnection の一覧が表示されます。

![WebRTC Stats PeerConnection List](../images/webrtc-stats_peerconnection.png)

PeerConnection のリスト上にあるボタンを押下すると、対象の PeerConnection の統計情報一覧がプルダウンメニューとして表示されます。

![WebRTC Stats StatsType List](../images/webrtc-stats_statstypelist.png)

プルダウンから項目を選択すると、統計情報の一覧が表示されます。
下記は `CandidatePair` を選択した際の例です。

![WebRTC Stats Exsample StatsMember](../images/webrtc-stats_example-statsmember.png)

また、時間によって変化するデータに関してはグラフを表示します。

![WebRTC Stats Exsample StatsGraph](../images/webrtc-stats_example-statsgraph.png)

## 統計情報の保存
画面の右上にある Save ボタンを押下することによって、収集した統計情報を保存できます。

![WebRTC Stats Save Dump](../images/webrtc-stats_savedump.png)

生成されるダンプファイルは、Chromeの `chrome://webrtc-internals` で生成されるダンプファイルと互換性があるため、サードパーティのアプリケーション上で統計情報を確認することができます。
