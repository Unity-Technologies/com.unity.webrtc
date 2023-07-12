using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC;
using Unity.WebRTC.Samples;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

class E2ELatencySample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button startButton;
    [SerializeField] private Button callButton;
    [SerializeField] private Button hangUpButton;
    [SerializeField] private Text textLocalTimestamp;
    [SerializeField] private Text textRemoteTimestamp;
    [SerializeField] private Text textLatency;
    [SerializeField] private Text textAverageLatency;
    [SerializeField] private Dropdown dropDownFramerate;
    [SerializeField] private Toggle toggleSyncApplicationFramerate;

    [SerializeField] private BarcodeEncoder encoder;
    [SerializeField] private BarcodeDecoder decoder;

    [SerializeField] private RawImage sourceImage;
    [SerializeField] private RawImage receiveImage;
#pragma warning restore 0649

    private RTCPeerConnection _pc1, _pc2;
    private MediaStream sendStream, receiveStream;
    private DelegateOnIceConnectionChange pc1OnIceConnectionChange;
    private DelegateOnIceConnectionChange pc2OnIceConnectionChange;
    private DelegateOnIceCandidate pc1OnIceCandidate;
    private DelegateOnIceCandidate pc2OnIceCandidate;
    private DelegateOnTrack pc2Ontrack;
    private DelegateOnNegotiationNeeded pc1OnNegotiationNeeded;
    private bool videoUpdateStarted;

    RenderTexture destTexture;
    int averageLantecy = 0;
    Queue<int> queueLatency = new Queue<int>(10);

    int vSyncCount;
    int targetFrameRate;

    List<int> listFramerate = new List<int>()
    {
        15,
        30,
        60,
        90
    };

    private void Awake()
    {
        startButton.onClick.AddListener(OnStart);
        callButton.onClick.AddListener(OnCall);
        hangUpButton.onClick.AddListener(OnHangUp);
    }

    private void OnDestroy()
    {
        // Revert global settings
        QualitySettings.vSyncCount = vSyncCount;
        Application.targetFrameRate = targetFrameRate;
    }

    private void Start()
    {
        // This sample uses Compute Shader.
        if (!SystemInfo.supportsComputeShaders)
            throw new System.NotSupportedException("Compute shader is not supported on this device");

        vSyncCount = QualitySettings.vSyncCount;
        targetFrameRate = Application.targetFrameRate;

        callButton.interactable = false;
        hangUpButton.interactable = false;
        dropDownFramerate.interactable = true;
        dropDownFramerate.options =
            listFramerate.Select(_ => new Dropdown.OptionData($"{_}")).ToList();
        dropDownFramerate.value = 1;
        dropDownFramerate.onValueChanged.AddListener(OnFramerateChanged);
        toggleSyncApplicationFramerate.interactable = true;
        toggleSyncApplicationFramerate.isOn = true;

        OnFramerateChanged(dropDownFramerate.value);

        pc1OnIceConnectionChange = state => { OnIceConnectionChange(_pc1, state); };
        pc2OnIceConnectionChange = state => { OnIceConnectionChange(_pc2, state); };
        pc1OnIceCandidate = candidate => { OnIceCandidate(_pc1, candidate); };
        pc2OnIceCandidate = candidate => { OnIceCandidate(_pc2, candidate); };
        pc2Ontrack = e =>
        {
            receiveStream.AddTrack(e.Track);
        };
        pc1OnNegotiationNeeded = () => { StartCoroutine(PeerNegotiationNeeded(_pc1)); };

        StartCoroutine(WebRTC.Update());
    }

    private void OnFramerateChanged(int value)
    {
        // Set "Don't Sync" for changing framerate,
        // but iOS ignores this setting
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = listFramerate[value];
    }

    private void OnStart()
    {
        startButton.interactable = false;
        callButton.interactable = true;
        dropDownFramerate.interactable = false;
        toggleSyncApplicationFramerate.interactable = false;
        if (sendStream == null)
        {
            int width = WebRTCSettings.StreamSize.x;
            int height = WebRTCSettings.StreamSize.y;
            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            var tex = new RenderTexture(width, height, 0, format);
            tex.Create();
            destTexture = new RenderTexture(width, height, 0, format);
            destTexture.Create();
            sourceImage.texture = tex;
            sourceImage.color = Color.white;

            sendStream = new MediaStream();
            sendStream.AddTrack(new VideoStreamTrack(destTexture));
        }
        if (receiveStream == null)
        {
            receiveStream = new MediaStream();
            receiveStream.OnAddTrack = e =>
            {
                if (e.Track is VideoStreamTrack track)
                {
                    track.OnVideoReceived += tex =>
                    {
                        receiveImage.texture = tex;
                        receiveImage.color = Color.white;
                        videoUpdateStarted = true;
                    };
                }
            };
        }
    }

    private void Update()
    {
        if (!videoUpdateStarted)
            return;

        int localTimestamp = (int)(Time.realtimeSinceStartup * 1000);
        encoder.SetValue(localTimestamp);
        Graphics.Blit(sourceImage.texture, destTexture, encoder.Material);

        int remoteTimestamp = decoder.GetValue(receiveImage.texture);

        textLocalTimestamp.text = localTimestamp.ToString();
        textRemoteTimestamp.text = remoteTimestamp.ToString();

        int latency = localTimestamp - remoteTimestamp;
        UpdateLatencyInfo(latency);
    }

    void UpdateLatencyInfo(int latency)
    {
        queueLatency.Enqueue(latency);
        if (queueLatency.Count == 10)
        {
            averageLantecy = (int)queueLatency.Average();
            queueLatency.Clear();
        }
        textLatency.text = latency.ToString();
        textAverageLatency.text = averageLantecy.ToString();
    }

    private static RTCConfiguration GetSelectedSdpSemantics()
    {
        RTCConfiguration config = default;
        config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };

        return config;
    }

    private void OnIceConnectionChange(RTCPeerConnection pc, RTCIceConnectionState state)
    {
        Debug.Log($"{GetName(pc)} IceConnectionState: {state}");

        if (state == RTCIceConnectionState.Connected || state == RTCIceConnectionState.Completed)
        {
            StartCoroutine(CheckStats(pc));
            foreach (var sender in _pc1.GetSenders())
            {
                ChangeFramerate(sender, (uint)Application.targetFrameRate);
                sender.SyncApplicationFramerate = toggleSyncApplicationFramerate.isOn;
            }
        }
    }

    // Display the video codec that is actually used.
    IEnumerator CheckStats(RTCPeerConnection pc)
    {
        yield return new WaitForSeconds(0.1f);
        if (pc == null)
            yield break;

        var op = pc.GetStats();
        yield return op;
        if (op.IsError)
        {
            Debug.LogErrorFormat("RTCPeerConnection.GetStats failed: {0}", op.Error);
            yield break;
        }

        RTCStatsReport report = op.Value;
        RTCIceCandidatePairStats activeCandidatePairStats = null;
        RTCIceCandidateStats remoteCandidateStats = null;

        foreach (var transportStatus in report.Stats.Values.OfType<RTCTransportStats>())
        {
            if (report.Stats.TryGetValue(transportStatus.selectedCandidatePairId, out var tmp))
            {
                activeCandidatePairStats = tmp as RTCIceCandidatePairStats;
            }
        }

        if (activeCandidatePairStats == null || string.IsNullOrEmpty(activeCandidatePairStats.remoteCandidateId))
        {
            yield break;
        }

        foreach (var iceCandidateStatus in report.Stats.Values.OfType<RTCIceCandidateStats>())
        {
            if (iceCandidateStatus.Id == activeCandidatePairStats.remoteCandidateId)
            {
                remoteCandidateStats = iceCandidateStatus;
            }
        }

        if (remoteCandidateStats == null || string.IsNullOrEmpty(remoteCandidateStats.Id))
        {
            yield break;
        }

        Debug.Log($"{GetName(pc)} candidate stats Id:{remoteCandidateStats.Id}, Type:{remoteCandidateStats.candidateType}");
    }

    IEnumerator PeerNegotiationNeeded(RTCPeerConnection pc)
    {
        var op = pc.CreateOffer();
        yield return op;

        if (!op.IsError)
        {
            if (pc.SignalingState != RTCSignalingState.Stable)
            {
                Debug.LogError($"{GetName(pc)} signaling state is not stable.");
                yield break;
            }

            yield return StartCoroutine(OnCreateOfferSuccess(pc, op.Desc));
        }
        else
        {
            OnCreateSessionDescriptionError(op.Error);
        }
    }

    private void AddTracks()
    {
        var pc1VideoSenders = new List<RTCRtpSender>();
        foreach (var track in sendStream.GetTracks())
        {
            var sender = _pc1.AddTrack(track, sendStream);
            if (track.Kind == TrackKind.Video)
            {
                pc1VideoSenders.Add(sender);
            }
        }

        if (WebRTCSettings.UseVideoCodec != null)
        {
            var codecs = new[] { WebRTCSettings.UseVideoCodec };
            foreach (var transceiver in _pc1.GetTransceivers())
            {
                if (pc1VideoSenders.Contains(transceiver.Sender))
                {
                    transceiver.SetCodecPreferences(codecs);
                }
            }
        }
    }

    private void ChangeFramerate(RTCRtpSender sender, uint framerate)
    {
        RTCRtpSendParameters parameters = sender.GetParameters();
        parameters.encodings[0].maxFramerate = framerate;
        RTCError error = sender.SetParameters(parameters);
        if (error.errorType != RTCErrorType.None)
        {
            Debug.LogErrorFormat("RTCRtpSender.SetParameters failed {0}", error.errorType);
        }
    }

    private void DeleteTracks()
    {
        MediaStreamTrack[] senderTracks = sendStream.GetTracks().ToArray();
        foreach (var track in senderTracks)
        {
            sendStream.RemoveTrack(track);
            track.Dispose();
        }

        MediaStreamTrack[] receiveTracks = receiveStream.GetTracks().ToArray();
        foreach (var track in receiveTracks)
        {
            receiveStream.RemoveTrack(track);
            track.Dispose();
        }
    }

    private void OnCall()
    {
        callButton.interactable = false;
        hangUpButton.interactable = true;

        var configuration = GetSelectedSdpSemantics();
        _pc1 = new RTCPeerConnection(ref configuration);
        _pc1.OnIceCandidate = pc1OnIceCandidate;
        _pc1.OnIceConnectionChange = pc1OnIceConnectionChange;
        _pc1.OnNegotiationNeeded = pc1OnNegotiationNeeded;
        _pc2 = new RTCPeerConnection(ref configuration);
        _pc2.OnIceCandidate = pc2OnIceCandidate;
        _pc2.OnIceConnectionChange = pc2OnIceConnectionChange;
        _pc2.OnTrack = pc2Ontrack;

        AddTracks();
    }

    private void OnHangUp()
    {
        videoUpdateStarted = false;

        DeleteTracks();

        _pc1.Close();
        _pc2.Close();
        _pc1.Dispose();
        _pc2.Dispose();
        _pc1 = null;
        _pc2 = null;

        sendStream.Dispose();
        sendStream = null;

        receiveStream.Dispose();
        receiveStream = null;

        startButton.interactable = true;
        callButton.interactable = false;
        hangUpButton.interactable = false;
        dropDownFramerate.interactable = true;
        toggleSyncApplicationFramerate.interactable = true;
        receiveImage.color = Color.black;
    }

    private void OnIceCandidate(RTCPeerConnection pc, RTCIceCandidate candidate)
    {
        GetOtherPc(pc).AddIceCandidate(candidate);
    }

    private string GetName(RTCPeerConnection pc)
    {
        return (pc == _pc1) ? "pc1" : "pc2";
    }

    private RTCPeerConnection GetOtherPc(RTCPeerConnection pc)
    {
        return (pc == _pc1) ? _pc2 : _pc1;
    }

    private IEnumerator OnCreateOfferSuccess(RTCPeerConnection pc, RTCSessionDescription desc)
    {
        Debug.Log($"Offer from {GetName(pc)}\n{desc.sdp}");
        Debug.Log($"{GetName(pc)} setLocalDescription start");
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
            yield break;
        }

        var otherPc = GetOtherPc(pc);
        Debug.Log($"{GetName(otherPc)} setRemoteDescription start");
        var op2 = otherPc.SetRemoteDescription(ref desc);
        yield return op2;
        if (!op2.IsError)
        {
            OnSetRemoteSuccess(otherPc);
        }
        else
        {
            var error = op2.Error;
            OnSetSessionDescriptionError(ref error);
            yield break;
        }

        Debug.Log($"{GetName(otherPc)} createAnswer start");
        // Since the 'remote' side has no media stream we need
        // to pass in the right constraints in order for it to
        // accept the incoming offer of audio and video.

        var op3 = otherPc.CreateAnswer();
        yield return op3;
        if (!op3.IsError)
        {
            yield return OnCreateAnswerSuccess(otherPc, op3.Desc);
        }
        else
        {
            OnCreateSessionDescriptionError(op3.Error);
        }
    }

    private void OnSetLocalSuccess(RTCPeerConnection pc)
    {
        Debug.Log($"{GetName(pc)} SetLocalDescription complete");
    }

    void OnSetSessionDescriptionError(ref RTCError error)
    {
        Debug.LogError($"Error Detail Type: {error.message}");
        OnHangUp();
    }

    private void OnSetRemoteSuccess(RTCPeerConnection pc)
    {
        Debug.Log($"{GetName(pc)} SetRemoteDescription complete");
    }

    IEnumerator OnCreateAnswerSuccess(RTCPeerConnection pc, RTCSessionDescription desc)
    {
        Debug.Log($"Answer from {GetName(pc)}:\n{desc.sdp}");
        Debug.Log($"{GetName(pc)} setLocalDescription start");
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
        Debug.Log($"{GetName(otherPc)} setRemoteDescription start");

        var op2 = otherPc.SetRemoteDescription(ref desc);
        yield return op2;
        if (!op2.IsError)
        {
            OnSetRemoteSuccess(otherPc);
        }
        else
        {
            var error = op2.Error;
            OnSetSessionDescriptionError(ref error);
        }
    }

    private static void OnCreateSessionDescriptionError(RTCError error)
    {
        Debug.LogError($"Error Detail Type: {error.message}");
    }
}
