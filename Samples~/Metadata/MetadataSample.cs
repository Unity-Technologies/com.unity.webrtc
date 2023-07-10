using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.WebRTC;
using Unity.WebRTC.Samples;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

class MetadataSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button startButton;
    [SerializeField] private Button callButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button hangUpButton;

    [SerializeField] private Camera cam;
    [SerializeField] private RawImage sourceImage;
    [SerializeField] private RawImage receiveImage;
    [SerializeField] private Transform rotateObject;
    [SerializeField] private InputField metadataInput;
    [SerializeField] private Text metadataOutput;
#pragma warning restore 0649

    private RTCPeerConnection _pc1, _pc2;
    private List<RTCRtpSender> pc1Senders;
    private List<RTCRtpReceiver> pc1Receivers;
    private MediaStream videoStream, receiveStream;
    private DelegateOnIceConnectionChange pc1OnIceConnectionChange;
    private DelegateOnIceConnectionChange pc2OnIceConnectionChange;
    private DelegateOnIceCandidate pc1OnIceCandidate;
    private DelegateOnIceCandidate pc2OnIceCandidate;
    private DelegateOnTrack pc2Ontrack;
    private DelegateOnNegotiationNeeded pc1OnNegotiationNeeded;
    private bool videoUpdateStarted;

    private readonly object metadataInputLock = new object();
    private readonly object metadataOutputLock = new object();

    private NativeArray<byte> metadataArray;
    private NativeArray<byte> senderArray;
    private byte[] metadataOutputArray;


    private void Awake()
    {
        startButton.onClick.AddListener(OnStart);
        callButton.onClick.AddListener(Call);
        restartButton.onClick.AddListener(RestartIce);
        hangUpButton.onClick.AddListener(HangUp);
        receiveStream = new MediaStream();
    }

    private void Start()
    {
        pc1Senders = new List<RTCRtpSender>();
        pc1Receivers = new List<RTCRtpReceiver>();
        callButton.interactable = false;
        restartButton.interactable = false;
        hangUpButton.interactable = false;
        metadataInput.onValueChanged.AddListener(OnChangedMetadata);

        pc1OnIceConnectionChange = state => { OnIceConnectionChange(_pc1, state); };
        pc2OnIceConnectionChange = state => { OnIceConnectionChange(_pc2, state); };
        pc1OnIceCandidate = candidate => { OnIceCandidate(_pc1, candidate); };
        pc2OnIceCandidate = candidate => { OnIceCandidate(_pc2, candidate); };
        pc2Ontrack = e =>
        {
            var receiver = e.Receiver;
            pc1Receivers.Add(receiver);
            SetUpReceiverTransform(receiver);
            receiveStream.AddTrack(e.Track);
        };
        pc1OnNegotiationNeeded = () => { StartCoroutine(PeerNegotiationNeeded(_pc1)); };

        receiveStream.OnAddTrack = e =>
        {
            if (e.Track is VideoStreamTrack track)
            {
                track.OnVideoReceived += tex =>
                {
                    receiveImage.texture = tex;
                    receiveImage.color = Color.white;
                };
            }
        };
    }

    private void OnChangedMetadata(string data)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);

        lock (metadataInputLock)
        {
            if (metadataArray.IsCreated)
                metadataArray.Dispose();
            metadataArray = new NativeArray<byte>(bytes, Allocator.Persistent);
        }
    }

    private void OnDestroy()
    {
        lock (metadataInputLock)
        {
            if (metadataArray.IsCreated)
                metadataArray.Dispose();
            if (senderArray.IsCreated)
                senderArray.Dispose();
        }
    }

    private void SetUpSenderTransform(RTCRtpSender sender)
    {
        sender.Transform = new RTCRtpScriptTransform(TrackKind.Video, e => OnSenderTransform(sender.Transform, e));
    }

    private void SetUpReceiverTransform(RTCRtpReceiver receiver)
    {
        receiver.Transform = new RTCRtpScriptTransform(TrackKind.Video, e => OnReceiverTransform(receiver.Transform, e));
    }

    void OnSenderTransform(RTCRtpTransform transform, RTCTransformEvent e)
    {
        lock (metadataInputLock)
        {
            var data = e.Frame.GetData();
            var length = 1 + metadataArray.Length + data.Length;

            // resize NativeArray
            if (length > senderArray.Length)
            {
                if (senderArray.IsCreated)
                    senderArray.Dispose();
                senderArray = new NativeArray<byte>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }

            // encoded data
            NativeArray<byte>.Copy(src: data, srcIndex: 0, dst: senderArray, dstIndex: 0, length: data.Length);

            // metadata
            if (metadataArray.IsCreated)
                NativeArray<byte>.Copy(src: metadataArray, srcIndex: 0, dst: senderArray, dstIndex: data.Length, length: metadataArray.Length);

            // metadata length (< 256)
            NativeArray<byte>.Copy(src: new byte[] { (byte)metadataArray.Length }, srcIndex: 0, dst: senderArray, dstIndex: length - 1, length: 1);

            e.Frame.SetData(senderArray.AsReadOnly(), 0, length);
            transform.Write(e.Frame);
        }
    }

    void OnReceiverTransform(RTCRtpTransform transform, RTCTransformEvent e)
    {
        var data = e.Frame.GetData();

        var metadataLength = data[data.Length - 1];
        var length = data.Length - (1 + metadataLength);
        e.Frame.SetData(data, 0, length);
        transform.Write(e.Frame);

        lock (metadataOutputLock)
        {
            if (metadataOutputArray == null || metadataOutputArray.Length != metadataLength)
                metadataOutputArray = new byte[metadataLength];
            for (int i = 0; i < metadataLength; i++)
            {
                metadataOutputArray[i] = data[length + i];
            }
        }
    }

    private void OnStart()
    {
        startButton.interactable = false;
        callButton.interactable = true;

        if (videoStream == null)
        {
            videoStream = cam.CaptureStream(WebRTCSettings.StreamSize.x, WebRTCSettings.StreamSize.y);
        }

        sourceImage.texture = cam.targetTexture;
        sourceImage.color = Color.white;
    }

    private void Update()
    {
        if (rotateObject != null)
        {
            float t = Time.deltaTime;
            rotateObject.Rotate(100 * t, 200 * t, 300 * t);
        }

        lock (metadataOutputLock)
        {
            if (metadataOutputArray != null)
                metadataOutput.text = System.Text.Encoding.UTF8.GetString(metadataOutputArray);
        }
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
        foreach (var track in videoStream.GetTracks())
        {
            pc1Senders.Add(_pc1.AddTrack(track, videoStream));
        }

        if (WebRTCSettings.UseVideoCodec != null)
        {
            var codecs = new[] { WebRTCSettings.UseVideoCodec };
            foreach (var transceiver in _pc1.GetTransceivers())
            {
                if (pc1Senders.Contains(transceiver.Sender))
                {
                    transceiver.SetCodecPreferences(codecs);
                }
            }
        }

        foreach (var sender in pc1Senders)
        {
            SetUpSenderTransform(sender);
        }

        if (!videoUpdateStarted)
        {
            StartCoroutine(WebRTC.Update());
            videoUpdateStarted = true;
        }
    }

    private void RemoveTracks()
    {
        foreach (var sender in pc1Senders)
        {
            _pc1.RemoveTrack(sender);
        }

        pc1Senders.Clear();
        pc1Receivers.Clear();

        var tracks = receiveStream.GetTracks().ToArray();
        foreach (var track in tracks)
        {
            receiveStream.RemoveTrack(track);
        }
    }

    private void Call()
    {
        callButton.interactable = false;
        hangUpButton.interactable = true;
        restartButton.interactable = true;

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

    private void RestartIce()
    {
        restartButton.interactable = false;

        _pc1.RestartIce();
    }

    private void HangUp()
    {
        RemoveTracks();

        _pc1.Close();
        _pc2.Close();
        _pc1.Dispose();
        _pc2.Dispose();
        _pc1 = null;
        _pc2 = null;

        callButton.interactable = true;
        restartButton.interactable = false;
        hangUpButton.interactable = false;

        receiveImage.color = Color.black;
    }

    private void OnIceCandidate(RTCPeerConnection pc, RTCIceCandidate candidate)
    {
        GetOtherPc(pc).AddIceCandidate(candidate);
        Debug.Log($"{GetName(pc)} ICE candidate:\n {candidate.Candidate}");
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
        HangUp();
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
