using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC.Samples;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.WebRTC
{
    public class AudioSample : MonoBehaviour
    {
        [SerializeField] private AudioSource inputAudioSource;
        [SerializeField] private AudioSource outputAudioSource;
        [SerializeField] private Toggle toggleEnableMicrophone;
        [SerializeField] private Dropdown dropdownMicrophoneDevices;
        [SerializeField] private Dropdown dropdownAudioCodecs;
        [SerializeField] private Button buttonStart;
        [SerializeField] private Button buttonCall;
        [SerializeField] private Button buttonHangup;
        [SerializeField] private Text textChannelCount;
        [SerializeField] private Text textMinFrequency;
        [SerializeField] private Text textMaxFrequency;
        [SerializeField] private AudioClip audioclipStereoSample;

        private RTCPeerConnection _pc1, _pc2;
        private MediaStream _sendStream;
        private MediaStream _receiveStream;

        private AudioClip m_clipInput;
        private AudioStreamTrack m_audioTrack;
        private List<RTCRtpCodecCapability> availableCodecs = new List<RTCRtpCodecCapability>();

        int m_samplingFrequency = 48000;
        int m_lengthSeconds = 1;
        int m_channelCount = 1;

        private string m_deviceName = null;

        void Start()
        {
            WebRTC.Initialize(WebRTCSettings.EncoderType, WebRTCSettings.LimitTextureSize);
            StartCoroutine(WebRTC.Update());

            toggleEnableMicrophone.isOn = false;
            toggleEnableMicrophone.onValueChanged.AddListener(OnEnableMicrophone);
            dropdownMicrophoneDevices.interactable = false;
            dropdownMicrophoneDevices.options =
                Microphone.devices.Select(name => new Dropdown.OptionData(name)).ToList();
            dropdownMicrophoneDevices.onValueChanged.AddListener(OnDeviceChanged);

            dropdownAudioCodecs.AddOptions(new List<string>{"Default"});
            var codecs = RTCRtpSender.GetCapabilities(TrackKind.Audio).codecs;

            var excludeCodecTypes = new[] { "audio/CN", "audio/telephone-event" };
            foreach (var codec in codecs)
            {
                if (excludeCodecTypes.Count(type => codec.mimeType.Contains(type)) > 0)
                    continue;
                availableCodecs.Add(codec);
            }
            dropdownAudioCodecs.AddOptions(availableCodecs.Select(codec =>
                new Dropdown.OptionData(string.Format($"{codec.mimeType} {codec.clockRate} {codec.sdpFmtpLine.Replace(";", " ")}"))).ToList());

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
            if (toggleEnableMicrophone.isOn)
            {
                m_deviceName = dropdownMicrophoneDevices.captionText.text;
                m_clipInput = Microphone.Start(m_deviceName, true, m_lengthSeconds, m_samplingFrequency);
            }
            else
            {
                m_clipInput = audioclipStereoSample;
            }
            m_channelCount = m_clipInput.channels;



            inputAudioSource.loop = true;
            inputAudioSource.clip = m_clipInput;
            inputAudioSource.Play();

            buttonStart.interactable = false;
            buttonCall.interactable = true;
            buttonHangup.interactable = true;
        }

        void OnEnableMicrophone(bool enable)
        {
            dropdownMicrophoneDevices.interactable = enable;
        }

        void OnCall()
        {
            buttonCall.interactable = false;

            _receiveStream = new MediaStream();
            _receiveStream.OnAddTrack += OnAddTrack;
            _sendStream = new MediaStream();

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

            var transceiver2 = _pc2.AddTransceiver(TrackKind.Audio);
            transceiver2.Direction = RTCRtpTransceiverDirection.RecvOnly;

            m_audioTrack = new AudioStreamTrack(inputAudioSource);

            _pc1.AddTrack(m_audioTrack, _sendStream);

            var transceiver1 = _pc1.GetTransceivers().First();
            if (dropdownAudioCodecs.value == 0)
            {
                var error = transceiver1.SetCodecPreferences(this.availableCodecs.ToArray());
                Debug.Log(error);
            }
            else
            {
                var codec = availableCodecs[dropdownAudioCodecs.value - 1];
                var error = transceiver1.SetCodecPreferences(new[] { codec });
                Debug.Log(error);
            }
        }

        void OnAddTrack(MediaStreamTrackEvent e)
        {
            var track = e.Track as AudioStreamTrack;
            track.OnAudioReceived += OnAudioReceived;
        }

        void OnAudioReceived(AudioClip renderer)
        {
            outputAudioSource.clip = renderer;
            outputAudioSource.loop = true;
            outputAudioSource.Play();
        }

        void OnHangUp()
        {
            Microphone.End(m_deviceName);
            m_clipInput = null;

            m_audioTrack?.Dispose();
            _receiveStream?.Dispose();
            _sendStream?.Dispose();
            _pc1?.Dispose();
            _pc2?.Dispose();
                
            inputAudioSource.Stop();
            outputAudioSource.Stop();

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
            var op2 = otherPc.SetRemoteDescription(ref desc);
            yield return op2;
            if (op2.IsError)
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
        }


        IEnumerator OnCreateAnswerSuccess(RTCPeerConnection pc, RTCSessionDescription desc)
        {
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
            var op2 = otherPc.SetRemoteDescription(ref desc);
            yield return op2;
            if (op2.IsError)
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
    }
}
