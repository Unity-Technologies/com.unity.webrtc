using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC.Samples;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.WebRTC
{
    class AudioOutput
    {
        private AudioClip m_clip;
        private int m_head = 0;

        public AudioClip clip
        {
            get {
                return m_clip; 
            }
        }

        public AudioOutput(string name, int lengthSamples, int channels, int frequency)
        {
            m_clip = AudioClip.Create(name, lengthSamples, channels, frequency, false);
        }

        public void SetData(float[] data, int length)
        {
            m_clip.SetData(data, m_head);
            m_head += length;
            if (m_head >= m_clip.samples)
            {
                m_head = 0;
            }
        }
    }

    public class AudioSample : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource receiverAudioSource;
        [SerializeField] private Dropdown dropdownMicrophoneDevices;
        [SerializeField] private Button buttonStart;
        [SerializeField] private Button buttonCall;
        [SerializeField] private Button buttonHangup;
        [SerializeField] private Text textChannelCount;
        [SerializeField] private Text textMinFrequency;
        [SerializeField] private Text textMaxFrequency;

        private RTCPeerConnection _pc1, _pc2;
        private MediaStream _receiveStream;
        private DelegateOnTrack pc2Ontrack;
        private DelegateOnNegotiationNeeded pc1OnNegotiationNeeded;


        private AudioClip m_clipInput;
        private AudioOutput m_audioOutput;
        private AudioStreamTrack m_audioTrack;
        private AudioStreamTrack m_receiverTrack;
        int m_head;
        int m_samplingFrequency = 44100;
        int m_lengthSeconds = 1;
        int m_channelCount = 1;
        float[] m_microphoneBuffer = new float[0];

        private string m_deviceName = null;

        void Start()
        {
            WebRTC.Initialize(WebRTCSettings.EncoderType, WebRTCSettings.LimitTextureSize);
            StartCoroutine(WebRTC.Update());

            dropdownMicrophoneDevices.options =
                Microphone.devices.Select(name => new Dropdown.OptionData(name)).ToList();
            dropdownMicrophoneDevices.onValueChanged.AddListener(OnDeviceChanged);

            // Update UI
            OnDeviceChanged(dropdownMicrophoneDevices.value);

            buttonStart.onClick.AddListener(OnStart);
            buttonCall.onClick.AddListener(OnCall);
            buttonHangup.onClick.AddListener(OnHangUp);
        }

        void OnDestroy()
        {
            WebRTC.Dispose();
        }

        void OnStart()
        {
            m_deviceName = dropdownMicrophoneDevices.captionText.text;
            m_clipInput = Microphone.Start(m_deviceName, true, m_lengthSeconds, m_samplingFrequency);
            m_channelCount = m_clipInput.channels;

            m_audioOutput = new AudioOutput(
                "output", m_clipInput.samples, m_clipInput.channels, m_clipInput.frequency);
            audioSource.clip = m_audioOutput.clip;
            audioSource.Play();

            buttonStart.interactable = false;
            buttonCall.interactable = true;
            buttonHangup.interactable = true;
        }

        void OnCall()
        {
            _receiveStream = new MediaStream();
            _receiveStream.OnAddTrack = e => (e.Track as AudioStreamTrack).OnAudioReceived += OnAudioReceived;

            var configuration = GetSelectedSdpSemantics();
            _pc1 = new RTCPeerConnection(ref configuration)
            {
                OnIceCandidate = candidate => _pc2.AddIceCandidate(candidate),
                OnNegotiationNeeded = () => StartCoroutine(PeerNegotiationNeeded(_pc1))
            };

            _pc2 = new RTCPeerConnection(ref configuration)
            {
                OnIceCandidate = candidate => _pc1.AddIceCandidate(candidate),
                OnTrack = e => _receiveStream.AddTrack(e.Track),
            };

            var transceiver = _pc2.AddTransceiver(TrackKind.Audio);
            transceiver.Direction = RTCRtpTransceiverDirection.RecvOnly;

            m_audioTrack = new AudioStreamTrack("audio", audioSource);
            _pc1.AddTrack(m_audioTrack);
        }

        void OnAudioReceived(float[] data, int bitsPerSample, int sampleRate, int numOfChannels, int numOfFrames)
        {
            Debug.Log($"data.Length {data.Length}, " +
                      $"bitsPerSample {bitsPerSample}, " +
                      $"sampleRate {sampleRate}, " +
                      $"numOfChannels {numOfChannels}, " +
                      $"numOfFrames {numOfFrames}");
        }

        //private void OnAudioFilterRead(float[] data, int channels)
        //{
        //    if (m_audioTrack != null)
        //    {
        //        Audio.Update(data, channels);
        //    }
        //}

        void OnHangUp()
        {
            Microphone.End(m_deviceName);
            m_clipInput = null;

            buttonStart.interactable = true;
            buttonCall.interactable = false;
            buttonHangup.interactable = false;
        }

        void OnDeviceChanged(int value)
        {
            m_deviceName = dropdownMicrophoneDevices.options[value].text;
            Microphone.GetDeviceCaps(m_deviceName, out int minFreq, out int maxFreq);

            textChannelCount.text = string.Format($"Channel Count: {m_channelCount}");
            textMinFrequency.text = string.Format($"Minimum frequency: {minFreq}");
            textMaxFrequency.text = string.Format($"Maximum frequency: {maxFreq}");
        }

        private void OnIceConnectionChange(RTCPeerConnection pc, RTCIceConnectionState state)
        {
            //Debug.Log($"{GetName(pc)} IceConnectionState: {state}");

            if (state == RTCIceConnectionState.Connected || state == RTCIceConnectionState.Completed)
            {
                //StartCoroutine(CheckStats(pc));
            }
        }

        private static RTCConfiguration GetSelectedSdpSemantics()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };

            return config;
        }


        IEnumerator PeerNegotiationNeeded(RTCPeerConnection pc)
        {
            var op = pc.CreateOffer();
            yield return op;

            if (!op.IsError)
            {
                if (pc.SignalingState != RTCSignalingState.Stable)
                {
                    yield break;
                }

                yield return StartCoroutine(OnCreateOfferSuccess(pc, op.Desc));
            }
            else
            {
                var error = op.Error;
                OnSetSessionDescriptionError(ref error);
            }
        }

        private RTCPeerConnection GetOtherPc(RTCPeerConnection pc)
        {
            return (pc == _pc1) ? _pc2 : _pc1;
        }

        private IEnumerator OnCreateOfferSuccess(RTCPeerConnection pc, RTCSessionDescription desc)
        {
            Debug.Log("setLocalDescription start");
            var op = pc.SetLocalDescription(ref desc);
            yield return op;

            if (!op.IsError)
            {
                OnSetLocalSuccess(pc);
            }
            else
            {
                var error = op.Error;
                OnSetSessionDescriptionError(ref error);
            }

            var otherPc = GetOtherPc(pc);
//            Debug.Log($"{GetName(otherPc)} setRemoteDescription start");
            var op2 = otherPc.SetRemoteDescription(ref desc);
            yield return op2;
            if (!op2.IsError)
            {
//                OnSetRemoteSuccess(otherPc);
            }
            else
            {
                var error = op2.Error;
                OnSetSessionDescriptionError(ref error);
            }
            var op3 = otherPc.CreateAnswer();
            yield return op3;
            if (!op3.IsError)
            {
                yield return OnCreateAnswerSuccess(otherPc, op3.Desc);
            }
            else
            {
//                OnCreateSessionDescriptionError(op3.Error);
            }
        }


        IEnumerator OnCreateAnswerSuccess(RTCPeerConnection pc, RTCSessionDescription desc)
        {
            //Debug.Log($"Answer from {GetName(pc)}:\n{desc.sdp}");
            //Debug.Log($"{GetName(pc)} setLocalDescription start");
            var op = pc.SetLocalDescription(ref desc);
            yield return op;

            if (!op.IsError)
            {
                OnSetLocalSuccess(pc);
            }
            else
            {
                var error = op.Error;
                OnSetSessionDescriptionError(ref error);
            }

            var otherPc = GetOtherPc(pc);
//            Debug.Log($"{GetName(otherPc)} setRemoteDescription start");

            var op2 = otherPc.SetRemoteDescription(ref desc);
            yield return op2;
            if (!op2.IsError)
            {
//                OnSetRemoteSuccess(otherPc);
            }
            else
            {
                var error = op2.Error;
                OnSetSessionDescriptionError(ref error);
            }
        }

        private void OnSetLocalSuccess(RTCPeerConnection pc)
        {
            Debug.Log("SetLocalDescription complete");
        }

        static void OnSetSessionDescriptionError(ref RTCError error)
        {
            Debug.LogError($"Error Detail Type: {error.message}");
        }

        void Update()
        {
            if (!Microphone.IsRecording(m_deviceName))
                return;

            int position = Microphone.GetPosition(m_deviceName);
            if (position < 0 || m_head == position)
            {
                return;
            }

            if (m_head > position)
            {
                m_head = 0;
            }

            if (m_microphoneBuffer.Length != m_samplingFrequency * m_lengthSeconds * m_channelCount)
            {
                m_microphoneBuffer = new float[m_samplingFrequency * m_lengthSeconds * m_channelCount];
            }
            m_clipInput.GetData(m_microphoneBuffer, m_head);
            ProcessAudio(m_microphoneBuffer, position - m_head);

            m_head = position;
        }



        private void ProcessAudio(float[] data, int dataLength)
        {
            // Audio.Update(data, dataLength, m_channelCount);
        }
    }
}
