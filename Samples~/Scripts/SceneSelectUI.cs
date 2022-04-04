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

        private static bool s_limitTextureSize = true;
        private static Vector2Int s_StreamSize = new Vector2Int(DefaultStreamWidth, DefaultStreamHeight);
        private static RTCRtpCodecCapability s_useVideoCodec = null;

        public static bool LimitTextureSize
        {
            get { return s_limitTextureSize; }
            set { s_limitTextureSize = value; }
        }

        public static Vector2Int StreamSize
        {
            get { return s_StreamSize; }
            set { s_StreamSize = value; }
        }

        public static RTCRtpCodecCapability UseVideoCodec
        {
            get { return s_useVideoCodec; }
            set { s_useVideoCodec = value; }
        }
    }

    internal class SceneSelectUI : MonoBehaviour
    {
        [SerializeField] private Dropdown codecSelector;
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

        private static readonly string[] excludeCodecMimeType = { "video/red", "video/ulpfec", "video/rtx" };
        private List<RTCRtpCodecCapability> availableCodecs;

        void Awake()
        {
            WebRTC.Initialize();
        }

        void OnDestroy()
        {
            WebRTC.Dispose();
        }

        void Start()
        {
            var capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
            availableCodecs = capabilities.codecs
                .Where(codec => !excludeCodecMimeType.Contains(codec.mimeType))
                .ToList();
            var list = availableCodecs
                .Select(codec => new Dropdown.OptionData {text = codec.mimeType + " " + codec.sdpFmtpLine})
                .ToList();

            codecSelector.options.AddRange(list);
            var previewCodec = WebRTCSettings.UseVideoCodec;
            codecSelector.value = previewCodec == null
                ? 0
                : availableCodecs.FindIndex(x =>
                    x.mimeType == previewCodec.mimeType && x.sdpFmtpLine == previewCodec.sdpFmtpLine) + 1;
            codecSelector.onValueChanged.AddListener(OnChangeCodecSelect);

            if (WebRTCSettings.StreamSize.x != WebRTCSettings.DefaultStreamWidth ||
                WebRTCSettings.StreamSize.y != WebRTCSettings.DefaultStreamHeight)
            {
                textureWidthInput.text = WebRTCSettings.StreamSize.x.ToString();
                textureHeightInput.text = WebRTCSettings.StreamSize.y.ToString();
            }
            textureWidthInput.onValueChanged.AddListener(OnChangeTextureWidthInput);
            textureHeightInput.onValueChanged.AddListener(OnChangeTextureHeightInput);

            toggleLimitTextureSize.isOn = WebRTCSettings.LimitTextureSize;
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

            // This sample uses Compute Shader, so almost Android devices don't work correctly.
            if (!SystemInfo.supportsComputeShaders)
                buttonLatency.interactable = false;
        }

        private void OnChangeCodecSelect(int index)
        {
            WebRTCSettings.UseVideoCodec = index == 0 ? null : availableCodecs[index - 1];
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
            WebRTCSettings.LimitTextureSize = enable;
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
    }
}
