using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC.Samples;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.WebRTC
{
    class AudioSample : MonoBehaviour
    {
        [SerializeField] private AudioSource inputAudioSource;
        [SerializeField] private AudioSource outputAudioSource;
        [SerializeField] private Toggle toggleEnableMicrophone;
        [SerializeField] private Toggle toggleLoopback;
        [SerializeField] private Dropdown dropdownAudioClips;
        [SerializeField] private Dropdown dropdownMicrophoneDevices;
        [SerializeField] private Dropdown dropdownAudioCodecs;
        [SerializeField] private Dropdown dropdownSpeakerMode;
        [SerializeField] private Dropdown dropdownDSPBufferSize;
        [SerializeField] private Dropdown dropdownBandwidth;
        [SerializeField] private Button buttonStart;
        [SerializeField] private Button buttonCall;
        [SerializeField] private Button buttonPause;
        [SerializeField] private Button buttonResume;
        [SerializeField] private Button buttonHangup;
        [SerializeField] private AudioClip[] audioclipList;
        [SerializeField] private Text textBandwidth;

        private RTCPeerConnection _pc1, _pc2;
        private MediaStream _sendStream;
        private MediaStream _receiveStream;

        private AudioClip m_clipInput;
        private AudioStreamTrack m_audioTrack;
        private List<RTCRtpCodecCapability> availableCodecs = new List<RTCRtpCodecCapability>();

        int m_samplingFrequency = 48000;
        int m_lengthSeconds = 1;

        private string m_deviceName = null;

        private Dictionary<string, ulong?> bandwidthOptions = new Dictionary<string, ulong?>()
        {
            { "undefined", null },
            { "320",  320 },
            { "160",  160 },
            { "80", 80 },
            { "40", 40 },
            { "20",  20 },
        };

        private Dictionary<string, int> dspBufferSizeOptions = new Dictionary<string, int>()
        {
            { "Best Latency",  256 },
            { "Good Latency", 512 },
            { "Best Performance", 1024 },
        };

        void Start()
        {
            StartCoroutine(WebRTC.Update());
            StartCoroutine(LoopStatsCoroutine());

            toggleEnableMicrophone.isOn = false;
            toggleEnableMicrophone.onValueChanged.AddListener(OnEnableMicrophone);
            toggleEnableMicrophone.isOn = false;
            toggleLoopback.onValueChanged.AddListener(OnChangeLoopback);
            dropdownAudioClips.interactable = true;
            dropdownAudioClips.options =
                audioclipList.Select(clip => new Dropdown.OptionData(clip.name)).ToList();
            dropdownMicrophoneDevices.interactable = false;
            dropdownMicrophoneDevices.options =
                Microphone.devices.Select(name => new Dropdown.OptionData(name)).ToList();
            dropdownMicrophoneDevices.onValueChanged.AddListener(OnDeviceChanged);
            var audioConf = AudioSettings.GetConfiguration();
            dropdownSpeakerMode.options =
                Enum.GetNames(typeof(AudioSpeakerMode)).Select(mode => new Dropdown.OptionData(mode)).ToList();
            dropdownSpeakerMode.value = (int)audioConf.speakerMode;
            dropdownSpeakerMode.onValueChanged.AddListener(OnSpeakerModeChanged);

            dropdownDSPBufferSize.options =
                dspBufferSizeOptions.Select(clip => new Dropdown.OptionData(clip.Key)).ToList();
            dropdownDSPBufferSize.onValueChanged.AddListener(OnDSPBufferSizeChanged);

            // best latency is default
            OnDSPBufferSizeChanged(dropdownDSPBufferSize.value);

            dropdownAudioCodecs.AddOptions(new List<string> { "Default" });
            var codecs = RTCRtpSender.GetCapabilities(TrackKind.Audio).codecs;

            var excludeCodecTypes = new[] { "audio/CN", "audio/telephone-event" };
            foreach (var codec in codecs)
            {
                if (excludeCodecTypes.Count(type => codec.mimeType.Contains(type)) > 0)
                    continue;
                availableCodecs.Add(codec);
            }
            dropdownAudioCodecs.AddOptions(availableCodecs.Select(codec =>
                new Dropdown.OptionData(CodecToOptionName(codec))).ToList());

            dropdownBandwidth.options = bandwidthOptions
                .Select(pair => new Dropdown.OptionData { text = pair.Key })
                .ToList();
            dropdownBandwidth.onValueChanged.AddListener(OnBandwidthChanged);
            dropdownBandwidth.interactable = false;

            // Update UI
            OnDeviceChanged(dropdownMicrophoneDevices.value);

            buttonStart.onClick.AddListener(OnStart);
            buttonCall.onClick.AddListener(OnCall);
            buttonPause.onClick.AddListener(OnPause);
            buttonResume.onClick.AddListener(OnResume);
            buttonHangup.onClick.AddListener(OnHangUp);
        }

        static string CodecToOptionName(RTCRtpCodecCapability cap)
        {
            return string.Format($"{cap.mimeType} " +
                $"{cap.clockRate} " +
                $"channel={cap.channels}");
        }

        void OnStart()
        {
            if (toggleEnableMicrophone.isOn)
            {
                m_deviceName = dropdownMicrophoneDevices.captionText.text;
                m_clipInput = Microphone.Start(m_deviceName, true, m_lengthSeconds, m_samplingFrequency);
                // set the latency to “0” samples before the audio starts to play.
                while (!(Microphone.GetPosition(m_deviceName) > 0)) { }
            }
            else
            {
                var clipIndex = dropdownAudioClips.value;
                m_clipInput = audioclipList[clipIndex];
            }
            inputAudioSource.loop = true;
            inputAudioSource.clip = m_clipInput;
            inputAudioSource.Play();

            buttonStart.interactable = false;
            buttonCall.interactable = true;
            buttonHangup.interactable = true;
            dropdownSpeakerMode.interactable = false;
            dropdownDSPBufferSize.interactable = false;
            dropdownAudioCodecs.interactable = false;
        }

        void OnEnableMicrophone(bool enable)
        {
            dropdownMicrophoneDevices.interactable = enable;
            dropdownAudioClips.interactable = !enable;
        }

        void OnChangeLoopback(bool loopback)
        {
            if (m_audioTrack != null)
            {
                m_audioTrack.Loopback = loopback;
            }
        }

        void OnCall()
        {
            buttonCall.interactable = false;
            buttonPause.interactable = true;
            dropdownBandwidth.interactable = true;

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
            m_audioTrack.Loopback = toggleLoopback.isOn;
            _pc1.AddTrack(m_audioTrack, _sendStream);

            var transceiver1 = _pc1.GetTransceivers().First();
            if (dropdownAudioCodecs.value == 0)
            {
                var error = transceiver1.SetCodecPreferences(this.availableCodecs.ToArray());
                if (error != RTCErrorType.None)
                    Debug.LogError(error);
            }
            else
            {
                var codec = availableCodecs[dropdownAudioCodecs.value - 1];
                var error = transceiver1.SetCodecPreferences(new[] { codec });
                if (error != RTCErrorType.None)
                    Debug.LogError(error);

            }
        }

        void OnPause()
        {
            var transceiver1 = _pc1.GetTransceivers().First();
            var track = transceiver1.Sender.Track;
            track.Enabled = false;

            buttonResume.gameObject.SetActive(true);
            buttonPause.gameObject.SetActive(false);
        }

        void OnResume()
        {
            var transceiver1 = _pc1.GetTransceivers().First();
            var track = transceiver1.Sender.Track;
            track.Enabled = true;

            buttonResume.gameObject.SetActive(false);
            buttonPause.gameObject.SetActive(true);
        }

        void OnAddTrack(MediaStreamTrackEvent e)
        {
            var track = e.Track as AudioStreamTrack;
            outputAudioSource.SetTrack(track);
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
            _pc1 = null;
            _pc2 = null;

            inputAudioSource.Stop();
            outputAudioSource.Stop();

            buttonStart.interactable = true;
            buttonCall.interactable = false;
            buttonHangup.interactable = false;
            buttonPause.interactable = false;

            buttonResume.gameObject.SetActive(false);
            buttonPause.gameObject.SetActive(true);

            dropdownSpeakerMode.interactable = true;
            dropdownDSPBufferSize.interactable = true;
            dropdownAudioCodecs.interactable = true;

            dropdownBandwidth.interactable = false;

        }

        void OnDeviceChanged(int value)
        {
            if (dropdownMicrophoneDevices.options.Count == 0)
                return;
            m_deviceName = dropdownMicrophoneDevices.options[value].text;
            Microphone.GetDeviceCaps(m_deviceName, out int minFreq, out int maxFreq);
        }

        private void OnBandwidthChanged(int index)
        {
            if (_pc1 == null || _pc2 == null)
                return;
            ulong? bandwidth = bandwidthOptions.Values.ElementAt(index);
            RTCRtpSender sender = _pc1.GetSenders().First();
            RTCRtpSendParameters parameters = sender.GetParameters();
            if (bandwidth == null)
            {
                parameters.encodings[0].maxBitrate = null;
                parameters.encodings[0].minBitrate = null;
            }
            else
            {
                parameters.encodings[0].maxBitrate = bandwidth * 1000;
                parameters.encodings[0].minBitrate = bandwidth * 1000;
            }

            RTCError error = sender.SetParameters(parameters);
            if (error.errorType != RTCErrorType.None)
            {
                Debug.LogErrorFormat("RTCRtpSender.SetParameters failed {0}", error.errorType);
            }

            Debug.Log("SetParameters:" + bandwidth);
        }

        void OnSpeakerModeChanged(int value)
        {
            var audioConf = AudioSettings.GetConfiguration();
            audioConf.speakerMode = (AudioSpeakerMode)value;
            Debug.Log(audioConf.speakerMode);
            if (!AudioSettings.Reset(audioConf))
            {
                Debug.LogError("Failed changing Audio Settings");
            }
        }

        void OnDSPBufferSizeChanged(int value)
        {
            var audioConf = AudioSettings.GetConfiguration();
            audioConf.dspBufferSize = dspBufferSizeOptions.Values.ToArray()[value];
            if (!AudioSettings.Reset(audioConf))
            {
                Debug.LogError("Failed changing Audio Settings");
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

        private IEnumerator LoopStatsCoroutine()
        {
            while (true)
            {
                yield return StartCoroutine(UpdateStatsCoroutine());
                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator UpdateStatsCoroutine()
        {
            RTCRtpSender sender = _pc1?.GetSenders().First();
            if (sender == null)
                yield break;
            RTCStatsReportAsyncOperation op = sender.GetStats();
            yield return op;
            if (op.IsError)
            {
                Debug.LogErrorFormat("RTCRtpSender.GetStats() is failed {0}", op.Error.errorType);
            }
            else
            {
                UpdateStatsPacketSize(op.Value);
            }
        }

        private RTCStatsReport lastResult = null;
        private void UpdateStatsPacketSize(RTCStatsReport res)
        {
            foreach (RTCStats stats in res.Stats.Values)
            {
                if (!(stats is RTCOutboundRTPStreamStats report))
                {
                    continue;
                }

                long now = report.Timestamp;
                ulong bytes = report.bytesSent;

                if (lastResult != null)
                {
                    if (!lastResult.TryGetValue(report.Id, out RTCStats last))
                        continue;

                    var lastStats = last as RTCOutboundRTPStreamStats;
                    var duration = (double)(now - lastStats.Timestamp) / 1000000;
                    ulong bitrate = (ulong)(8 * (bytes - lastStats.bytesSent) / duration);
                    textBandwidth.text = (bitrate / 1000.0f).ToString("f2");
                    //if (autoScroll.isOn)
                    //{
                    //    statsField.MoveTextEnd(false);
                    //}
                }

            }
            lastResult = res;
        }
    }
}
