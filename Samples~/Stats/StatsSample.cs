using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

class StatsSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button callButton;
    [SerializeField] private Button hangupButton;
    [SerializeField] private InputField text;
    [SerializeField] private Dropdown dropdown;
    [SerializeField] private Dropdown dropdownFreq;
    [SerializeField] private AudioSource source;
    [SerializeField] private Camera cam;
#pragma warning restore 0649

    private RTCStatsReport report;
    private RTCPeerConnection pc1, pc2;
    private RTCDataChannel dataChannel, remoteDataChannel;
    private DelegateOnIceConnectionChange pc1OnIceConnectionChange = null;
    private DelegateOnIceConnectionChange pc2OnIceConnectionChange = null;
    private DelegateOnIceCandidate pc1OnIceCandidate = null;
    private DelegateOnIceCandidate pc2OnIceCandidate = null;
    private DelegateOnMessage onDataChannelMessage = null;
    private DelegateOnDataChannel onDataChannel = null;
    private int currentValue = 0;
    private WaitForSeconds wait;
    private Dictionary<string, float> dictFrequency = new Dictionary<string, float>()
    {
        { "60ms", 0.06f},
        { "120ms", 0.12f},
        { "300ms", 0.3f},
        { "1000ms", 1f},
    };

    private void Awake()
    {
        callButton.onClick.AddListener(() =>
        {
            callButton.interactable = false;
            hangupButton.interactable = true;
            StartCoroutine(Call());
        });
        hangupButton.onClick.AddListener(() =>
        {
            callButton.interactable = true;
            hangupButton.interactable = false;
            Dispose();
        });
    }

    private void Start()
    {
        dropdown.interactable = false;
        dropdown.onValueChanged.AddListener(OnValueChangedStats);

        callButton.interactable = true;

        pc1OnIceConnectionChange = state => { OnIceConnectionChange(pc1, state); };
        pc2OnIceConnectionChange = state => { OnIceConnectionChange(pc2, state); };
        pc1OnIceCandidate = candidate => { OnIceCandidate(pc1, candidate); };
        pc2OnIceCandidate = candidate => { OnIceCandidate(pc1, candidate); };

        onDataChannel = channel =>
        {
            remoteDataChannel = channel;
            remoteDataChannel.OnMessage = onDataChannelMessage;
        };

        dropdownFreq.options = dictFrequency.Select(pair => new Dropdown.OptionData(pair.Key)).ToList();
        dropdownFreq.value = 0;
        dropdownFreq.onValueChanged.AddListener(OnValueChangedFreq);
        OnValueChangedFreq(0);

        StartCoroutine(LoopGetStats());
    }

    void OnValueChangedFreq(int value)
    {
        wait = new WaitForSeconds(dictFrequency.ElementAt(value).Value);
    }

    void OnValueChangedStats(int value)
    {
        currentValue = value;
    }

    RTCConfiguration GetSelectedSdpSemantics()
    {
        RTCConfiguration config = default;
        config.iceServers = new RTCIceServer[]
        {
            new RTCIceServer { urls = new string[] { "stun:stun.l.google.com:19302" } }
        };

        return config;
    }
    void OnIceConnectionChange(RTCPeerConnection pc, RTCIceConnectionState state)
    {
        switch (state)
        {
            case RTCIceConnectionState.New:
                Debug.Log($"{GetName(pc)} IceConnectionState: New");
                break;
            case RTCIceConnectionState.Checking:
                Debug.Log($"{GetName(pc)} IceConnectionState: Checking");
                break;
            case RTCIceConnectionState.Closed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Closed");
                break;
            case RTCIceConnectionState.Completed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Completed");
                break;
            case RTCIceConnectionState.Connected:
                Debug.Log($"{GetName(pc)} IceConnectionState: Connected");
                break;
            case RTCIceConnectionState.Disconnected:
                Debug.Log($"{GetName(pc)} IceConnectionState: Disconnected");
                break;
            case RTCIceConnectionState.Failed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Failed");
                break;
            case RTCIceConnectionState.Max:
                Debug.Log($"{GetName(pc)} IceConnectionState: Max");
                break;
            default:
                break;
        }
    }

    IEnumerator Call()
    {
        callButton.interactable = false;
        Debug.Log("GetSelectedSdpSemantics");
        var configuration = GetSelectedSdpSemantics();
        pc1 = new RTCPeerConnection(ref configuration);
        Debug.Log("Created local peer connection object pc1");
        pc1.OnIceCandidate = pc1OnIceCandidate;
        pc1.OnIceConnectionChange = pc1OnIceConnectionChange;
        pc2 = new RTCPeerConnection(ref configuration);
        Debug.Log("Created remote peer connection object pc2");
        pc2.OnIceCandidate = pc2OnIceCandidate;
        pc2.OnIceConnectionChange = pc2OnIceConnectionChange;
        pc2.OnDataChannel = onDataChannel;

        dataChannel = pc1.CreateDataChannel("data");

        var audioTrack = new AudioStreamTrack(source);
        var videoTrack = cam.CaptureStreamTrack(1280, 720);
        yield return 0;

        pc1.AddTrack(audioTrack);
        pc1.AddTrack(videoTrack);

        Debug.Log("pc1 createOffer start");
        var op = pc1.CreateOffer();
        yield return op;

        if (!op.IsError)
        {
            yield return StartCoroutine(OnCreateOfferSuccess(op.Desc));
        }
        else
        {
            OnCreateSessionDescriptionError(op.Error);
        }
    }

    void Dispose()
    {
        dropdown.options = new List<Dropdown.OptionData>();
        dropdown.interactable = false;
        text.text = string.Empty;
        text.interactable = false;
        dataChannel.Dispose();
        pc1.Dispose();
        pc2.Dispose();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="pc"></param>
    /// <param name="streamEvent"></param>
    void OnIceCandidate(RTCPeerConnection pc, RTCIceCandidate candidate)
    {
        GetOtherPc(pc).AddIceCandidate(candidate);
        Debug.Log($"{GetName(pc)} ICE candidate:\n {candidate.Candidate}");
    }

    string GetName(RTCPeerConnection pc)
    {
        return (pc == pc1) ? "pc1" : "pc2";
    }

    RTCPeerConnection GetOtherPc(RTCPeerConnection pc)
    {
        return (pc == pc1) ? pc2 : pc1;
    }

    IEnumerator OnCreateOfferSuccess(RTCSessionDescription desc)
    {
        Debug.Log($"Offer from pc1\n{desc.sdp}");
        Debug.Log("pc1 setLocalDescription start");
        var op = pc1.SetLocalDescription(ref desc);
        yield return op;

        if (!op.IsError)
        {
            OnSetLocalSuccess(pc1);
        }
        else
        {
            var error = op.Error;
            OnSetSessionDescriptionError(ref error);
        }

        Debug.Log("pc2 setRemoteDescription start");
        var op2 = pc2.SetRemoteDescription(ref desc);
        yield return op2;
        if (!op2.IsError)
        {
            OnSetRemoteSuccess(pc2);
        }
        else
        {
            var error = op2.Error;
            OnSetSessionDescriptionError(ref error);
        }
        Debug.Log("pc2 createAnswer start");
        // Since the 'remote' side has no media stream we need
        // to pass in the right constraints in order for it to
        // accept the incoming offer of audio and video.

        var op3 = pc2.CreateAnswer();
        yield return op3;
        if (!op3.IsError)
        {
            yield return OnCreateAnswerSuccess(op3.Desc);
        }
        else
        {
            OnCreateSessionDescriptionError(op3.Error);
        }
    }

    void OnSetLocalSuccess(RTCPeerConnection pc)
    {
        Debug.Log($"{GetName(pc)} SetLocalDescription complete");
    }

    void OnSetSessionDescriptionError(ref RTCError error) { }

    void OnSetRemoteSuccess(RTCPeerConnection pc)
    {
        Debug.Log($"{GetName(pc)} SetRemoteDescription complete");
    }

    IEnumerator OnCreateAnswerSuccess(RTCSessionDescription desc)
    {
        Debug.Log($"Answer from pc2:\n{desc.sdp}");
        Debug.Log("pc2 setLocalDescription start");
        var op = pc2.SetLocalDescription(ref desc);
        yield return op;

        if (!op.IsError)
        {
            OnSetLocalSuccess(pc2);
        }
        else
        {
            var error = op.Error;
            OnSetSessionDescriptionError(ref error);
        }

        Debug.Log("pc1 setRemoteDescription start");

        var op2 = pc1.SetRemoteDescription(ref desc);
        yield return op2;
        if (!op2.IsError)
        {
            OnSetRemoteSuccess(pc1);
        }
        else
        {
            var error = op2.Error;
            OnSetSessionDescriptionError(ref error);
        }
    }

    IEnumerator LoopGetStats()
    {
        while (true)
        {
            yield return wait;

            if (callButton.interactable)
                continue;

            var op1 = pc1.GetStats();
            yield return op1;

            if (op1.IsError)
                continue;

            var report = op1.Value;
            if (dropdown.options.Count != report.Stats.Count)
            {
                var options = report.Stats.Select(pair => $"{pair.Value.Type}:{pair.Key}").ToList();
                dropdown.ClearOptions();
                dropdown.AddOptions(options);
                dropdown.interactable = true;
            }
            if (currentValue >= report.Stats.Count)
                continue;

            var stats = report.Stats.ElementAt(currentValue).Value;

            text.text = "Id:" + stats.Id + "\n";
            text.text += "Timestamp:" + stats.Timestamp + "\n";
            text.text += stats.Dict.Aggregate(string.Empty, (str, next) =>
                    str + next.Key + ":" + (next.Value == null ? string.Empty : next.Value.ToString()) + "\n");
            text.interactable = true;

            report.Dispose();
        }
    }

    void OnAddIceCandidateSuccess(RTCPeerConnection pc)
    {
        Debug.Log($"{GetName(pc)} addIceCandidate success");
    }

    void OnAddIceCandidateError(RTCPeerConnection pc, RTCError error)
    {
        Debug.Log($"{GetName(pc)} failed to add ICE Candidate: ${error}");
    }

    void OnCreateSessionDescriptionError(RTCError e)
    {

    }
}
