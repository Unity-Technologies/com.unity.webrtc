using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Unity.WebRTC.Samples
{
    public static class WebRTCSettings
    {
        private static bool s_enableHWCodec = false;

        public static bool EnableHWCodec
        {
            get { return s_enableHWCodec; }
            set { s_enableHWCodec = value; }
        }

        public static EncoderType EncoderType
        {
            get { return s_enableHWCodec ? EncoderType.Hardware : EncoderType.Software; }
        }
    }

    public class SceneSelectUI : MonoBehaviour
    {
        [SerializeField] private Toggle toggleEnableHWCodec;

        void Start()
        {
            toggleEnableHWCodec.isOn = WebRTCSettings.EnableHWCodec;
        }

        public void OnChangeHWCodec(bool enable)
        {
            WebRTCSettings.EnableHWCodec = enable;
        }

        public void OnPressedPeerConnectionButton()
        {
            SceneManager.LoadScene("PeerConnection", LoadSceneMode.Single);
        }
        public void OnPressedDataChannelButton()
        {
            SceneManager.LoadScene("DataChannel", LoadSceneMode.Single);
        }
        public void OnPressedMediaStreamButton()
        {
            SceneManager.LoadScene("MediaStream", LoadSceneMode.Single);
        }
        public void OnPressedMultiPeersButton()
        {
            SceneManager.LoadScene("MultiplePeerConnections", LoadSceneMode.Single);
        }
        public void OnPressedMultiVideoRecvButton()
        {
            SceneManager.LoadScene("MultiVideoReceive", LoadSceneMode.Single);
        }
        public void OnPressedMungeSDPButton()
        {
            SceneManager.LoadScene("MungeSDP", LoadSceneMode.Single);
        }
        public void OnPressedStatsButton()
        {
            SceneManager.LoadScene("Stats", LoadSceneMode.Single);
        }
        public void OnPressedPeerChangeCodecsButton()
        {
            SceneManager.LoadScene("ChangeCodecs", LoadSceneMode.Single);
        }
        public void OnPressedPeerTrickleIceButton()
        {
            SceneManager.LoadScene("TrickleIce", LoadSceneMode.Single);
        }
        public void OnPressedPeerVideoReceiveButton()
        {
            SceneManager.LoadScene("VideoReceive", LoadSceneMode.Single);
        }
        public void OnPressedPeerBandwidthButton()
        {
            SceneManager.LoadScene("Bandwidth", LoadSceneMode.Single);
        }
        public void OnPressedPeerPerfectNegotiationButton()
        {
            SceneManager.LoadScene("PerfectNegotiation", LoadSceneMode.Single);
        }
    }
}
