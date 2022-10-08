using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Unity.WebRTC.Samples
{
    internal static class WebRTCSettings
    {
        public const int DefaultStreamWidth = 1280;
        public const int DefaultStreamHeight = 720;

        private static Vector2Int s_StreamSize = new Vector2Int(DefaultStreamWidth, DefaultStreamHeight);
        private static RTCRtpCodecCapability s_senderVideoCodec = null;
        private static RTCRtpCodecCapability s_receiverVideoCodec = null;

        public static Vector2Int StreamSize
        {
            get { return s_StreamSize; }
            set { s_StreamSize = value; }
        }

        public static RTCRtpCodecCapability SenderCodec
        {
            get { return s_senderVideoCodec; }
            set { s_senderVideoCodec = value; }
        }
        public static RTCRtpCodecCapability ReceiverCodec
        {
            get { return s_receiverVideoCodec; }
            set { s_receiverVideoCodec = value; }
        }
    }

    internal class SceneSelectUI : MonoBehaviour
    {
        [SerializeField] private Dropdown senderCodecSelector;
        [SerializeField] private Dropdown receiverCodecSelector;
        [SerializeField] private Dropdown streamSizeSelector;
        [SerializeField] private InputField textureWidthInput;
        [SerializeField] private InputField textureHeightInput;
        [SerializeField] private Toggle toggleLimitTextureSize;
        [SerializeField] private Button buttonPeerConnection;
        [SerializeField] private Button buttonDataChannel;
        [SerializeField] private Button buttonMediaStream;
        [SerializeField] private Button buttonAudio;
        [SerializeField] private Button buttonMultiPeers;
        [SerializeField] private Button buttonMultiAudioRecv;
        [SerializeField] private Button buttonMultiVideoRecv;
        [SerializeField] private Button buttonMungeSDP;
        [SerializeField] private Button buttonStats;
        [SerializeField] private Button buttonChangeCodecs;
        [SerializeField] private Button buttonTrickleIce;
        [SerializeField] private Button buttonVideoReceive;
        [SerializeField] private Button buttonBandwidth;
        [SerializeField] private Button buttonPerfectNegotiation;
        [SerializeField] private Button buttonLatency;
        [SerializeField] private Button buttonReplaceTrack;

        List<Vector2Int> streamSizeList = new List<Vector2Int>()
        {
            new Vector2Int(640, 360),
            new Vector2Int(1280, 720),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440),
            new Vector2Int(3840, 2160),
            new Vector2Int(360, 640),
            new Vector2Int(720, 1280),
            new Vector2Int(1080, 1920),
            new Vector2Int(1440, 2560),
            new Vector2Int(2160, 3840),
        };

        private static readonly string[] excludeCodecMimeType = { "video/red", "video/ulpfec", "video/rtx", "video/flexfec-03" };
        private List<RTCRtpCodecCapability> availableSenderCodecs;
        private List<RTCRtpCodecCapability> availableReceiverCodecs;

        enum Direction
        {
            Sender,
            Receiver
        }

        static IEnumerable<RTCRtpCodecCapability> GetCodecCapabilities(Direction direction, TrackKind kind)
        {
            switch(direction)
            {
                case Direction.Sender:
                    return RTCRtpSender.GetCapabilities(kind).codecs
                        .Where(codec => !excludeCodecMimeType.Contains(codec.mimeType));
                case Direction.Receiver:
                    return RTCRtpReceiver.GetCapabilities(kind).codecs
                        .Where(codec => !excludeCodecMimeType.Contains(codec.mimeType));
            }
            throw new System.ArgumentException();
        }


        void Start()
        {
            var senderCodecs = GetCodecCapabilities(Direction.Sender, TrackKind.Video).ToList();
            var receiverCodecs = GetCodecCapabilities(Direction.Receiver, TrackKind.Video).ToList();

            senderCodecSelector.options.AddRange(senderCodecs
                .Select(codec => new Dropdown.OptionData { text = codec.mimeType + " " + codec.sdpFmtpLine })
                .ToList());
            receiverCodecSelector.options.AddRange(receiverCodecs
                .Select(codec => new Dropdown.OptionData { text = codec.mimeType + " " + codec.sdpFmtpLine })
                .ToList());
            var previewCodec = WebRTCSettings.SenderCodec;
            senderCodecSelector.value = previewCodec == null
                ? 0
                : senderCodecs.FindIndex(x =>
                    x.mimeType == previewCodec.mimeType && x.sdpFmtpLine == previewCodec.sdpFmtpLine) + 1;

            senderCodecSelector.onValueChanged.AddListener(index => OnChangeCodec(index, Direction.Sender, TrackKind.Video));
            receiverCodecSelector.onValueChanged.AddListener(index => OnChangeCodec(index, Direction.Receiver, TrackKind.Video));

            var optionList = streamSizeList.Select(size => new Dropdown.OptionData($" {size.x} x {size.y} ")).ToList();
            optionList.Add(new Dropdown.OptionData(" Custom "));
            streamSizeSelector.options = optionList;

            var existInList = streamSizeList.Contains(WebRTCSettings.StreamSize);
            if (existInList)
            {
                streamSizeSelector.value = streamSizeList.IndexOf(WebRTCSettings.StreamSize);
            }
            else
            {
                streamSizeSelector.value = optionList.Count - 1;
                textureWidthInput.text = WebRTCSettings.StreamSize.x.ToString();
                textureHeightInput.text = WebRTCSettings.StreamSize.y.ToString();
                textureWidthInput.interactable = true;
                textureHeightInput.interactable = true;
            }

            streamSizeSelector.onValueChanged.AddListener(OnChangeStreamSizeSelect);
            textureWidthInput.onValueChanged.AddListener(OnChangeTextureWidthInput);
            textureHeightInput.onValueChanged.AddListener(OnChangeTextureHeightInput);

            toggleLimitTextureSize.isOn = WebRTC.enableLimitTextureSize;
            toggleLimitTextureSize.onValueChanged.AddListener(OnChangeLimitTextureSize);

            buttonPeerConnection.onClick.AddListener(OnPressedPeerConnectionButton);
            buttonDataChannel.onClick.AddListener(OnPressedDataChannelButton);
            buttonMediaStream.onClick.AddListener(OnPressedMediaStreamButton);
            buttonAudio.onClick.AddListener(OnPressedAudioButton);
            buttonMultiPeers.onClick.AddListener(OnPressedMultiPeersButton);
            buttonMultiAudioRecv.onClick.AddListener(OnPressedMultiAudioRecvButton);
            buttonMultiVideoRecv.onClick.AddListener(OnPressedMultiVideoRecvButton);
            buttonMungeSDP.onClick.AddListener(OnPressedMungeSDPButton);
            buttonStats.onClick.AddListener(OnPressedStatsButton);
            buttonChangeCodecs.onClick.AddListener(OnPressedChangeCodecsButton);
            buttonTrickleIce.onClick.AddListener(OnPressedTrickleIceButton);
            buttonVideoReceive.onClick.AddListener(OnPressedVideoReceiveButton);
            buttonBandwidth.onClick.AddListener(OnPressedBandwidthButton);
            buttonPerfectNegotiation.onClick.AddListener(OnPressedPerfectNegotiationButton);
            buttonLatency.onClick.AddListener(OnPressedLatencyButton);
            buttonReplaceTrack.onClick.AddListener(OnPressedReplaceTrackButton);

            // This sample uses Compute Shader, so almost Android devices don't work correctly.
            if (!SystemInfo.supportsComputeShaders)
                buttonLatency.interactable = false;
        }

        private void OnChangeCodec(int index, Direction direction, TrackKind kind)
        {
            var codecs = GetCodecCapabilities(direction, kind).ToList();            
            var codec = index == 0 ? null : codecs[index - 1];
            switch(direction)
            {
                case Direction.Sender:
                    WebRTCSettings.SenderCodec = codec;
                    break;
                case Direction.Receiver:
                    WebRTCSettings.ReceiverCodec = codec;
                    break;
            }
        }

        private void OnChangeStreamSizeSelect(int index)
        {
            var isCustom = index >= streamSizeList.Count;
            textureWidthInput.interactable = isCustom;
            textureHeightInput.interactable = isCustom;

            if (isCustom)
            {
                return;
            }

            WebRTCSettings.StreamSize = streamSizeList[index];
        }

        private void OnChangeTextureWidthInput(string input)
        {
            var height = WebRTCSettings.StreamSize.y;

            if (string.IsNullOrEmpty(input))
            {
                WebRTCSettings.StreamSize = new Vector2Int(WebRTCSettings.DefaultStreamWidth, height);
                return;
            }

            if (int.TryParse(input, out var width))
            {
                WebRTCSettings.StreamSize = new Vector2Int(width, height);
            }
        }

        private void OnChangeTextureHeightInput(string input)
        {
            var width = WebRTCSettings.StreamSize.x;

            if (string.IsNullOrEmpty(input))
            {
                WebRTCSettings.StreamSize = new Vector2Int(width, WebRTCSettings.DefaultStreamHeight);
                return;
            }

            if (int.TryParse(input, out var height))
            {
                WebRTCSettings.StreamSize = new Vector2Int(width, height);
            }
        }

        private void OnChangeLimitTextureSize(bool enable)
        {
            WebRTC.enableLimitTextureSize = enable;
        }

        private void OnPressedPeerConnectionButton()
        {
            SceneManager.LoadScene("PeerConnection", LoadSceneMode.Single);
        }
        private void OnPressedDataChannelButton()
        {
            SceneManager.LoadScene("DataChannel", LoadSceneMode.Single);
        }
        private void OnPressedMediaStreamButton()
        {
            SceneManager.LoadScene("MediaStream", LoadSceneMode.Single);
        }

        private void OnPressedAudioButton()
        {
            SceneManager.LoadScene("Audio", LoadSceneMode.Single);
        }

        private void OnPressedMultiPeersButton()
        {
            SceneManager.LoadScene("MultiplePeerConnections", LoadSceneMode.Single);
        }
        private void OnPressedMultiAudioRecvButton()
        {
            SceneManager.LoadScene("MultiAudioReceive", LoadSceneMode.Single);
        }
        private void OnPressedMultiVideoRecvButton()
        {
            SceneManager.LoadScene("MultiVideoReceive", LoadSceneMode.Single);
        }
        private void OnPressedMungeSDPButton()
        {
            SceneManager.LoadScene("MungeSDP", LoadSceneMode.Single);
        }
        private void OnPressedStatsButton()
        {
            SceneManager.LoadScene("Stats", LoadSceneMode.Single);
        }
        private void OnPressedChangeCodecsButton()
        {
            SceneManager.LoadScene("ChangeCodecs", LoadSceneMode.Single);
        }
        private void OnPressedTrickleIceButton()
        {
            SceneManager.LoadScene("TrickleIce", LoadSceneMode.Single);
        }
        private void OnPressedVideoReceiveButton()
        {
            SceneManager.LoadScene("VideoReceive", LoadSceneMode.Single);
        }
        private void OnPressedBandwidthButton()
        {
            SceneManager.LoadScene("Bandwidth", LoadSceneMode.Single);
        }
        private void OnPressedPerfectNegotiationButton()
        {
            SceneManager.LoadScene("PerfectNegotiation", LoadSceneMode.Single);
        }
        private void OnPressedLatencyButton()
        {
            SceneManager.LoadScene("E2ELatency", LoadSceneMode.Single);
        }
        private void OnPressedReplaceTrackButton()
        {
            SceneManager.LoadScene("ReplaceTrack", LoadSceneMode.Single);
        }
    }
}
