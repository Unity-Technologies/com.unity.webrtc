using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Unity.WebRTC.Samples
{
    internal static class WebRTCSettings
    {
        private static bool s_enableHWCodec = false;
        private static bool s_limitTextureSize = true;

        public static bool EnableHWCodec
        {
            get { return s_enableHWCodec; }
            set { s_enableHWCodec = value; }
        }

        public static bool LimitTextureSize
        {
            get { return s_limitTextureSize; }
            set { s_limitTextureSize = value; }
        }

        public static EncoderType EncoderType
        {
            get { return s_enableHWCodec ? EncoderType.Hardware : EncoderType.Software; }
        }
    }

    internal class SceneSelectUI : MonoBehaviour
    {
        [SerializeField] private Toggle toggleEnableHWCodec;
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

        void Start()
        {
            toggleEnableHWCodec.isOn = WebRTCSettings.EnableHWCodec;
            toggleLimitTextureSize.isOn = WebRTCSettings.LimitTextureSize;
            toggleEnableHWCodec.onValueChanged.AddListener(OnChangeHWCodec);
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
        }

        private void OnChangeHWCodec(bool enable)
        {
            WebRTCSettings.EnableHWCodec = enable;
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
    }
}
