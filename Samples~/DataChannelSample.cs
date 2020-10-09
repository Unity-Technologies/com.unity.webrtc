using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.WebRTC;
using System;

public class DataChannelSample : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Button callButton;
    [SerializeField] private Button sendButton;
    [SerializeField] private InputField textSend;
    [SerializeField] private InputField textReceive;
    #pragma warning restore 0649

    private RTCPeerConnection pc1, pc2;
    private RTCDataChannel dataChannel, remoteDataChannel;
    private Coroutine sdpCheck;
    private string msg;
    private DelegateOnIceConnectionChange pc1OnIceConnectionChange;
    private DelegateOnIceConnectionChange pc2OnIceConnectionChange;
    private DelegateOnIceCandidate pc1OnIceCandidate;
    private DelegateOnIceCandidate pc2OnIceCandidate;
    private DelegateOnMessage onDataChannelMessage;
    private DelegateOnOpen onDataChannelOpen;
    private DelegateOnClose onDataChannelClose;
    private DelegateOnDataChannel onDataChannel;

    private RTCOfferOptions OfferOptions = new RTCOfferOptions
    {
        iceRestart = false,
        offerToReceiveAudio = true,
        offerToReceiveVideo = false
    };

    private RTCAnswerOptions AnswerOptions = new RTCAnswerOptions
    {
        iceRestart = false,
    };

    private void Awake()
    {
        WebRTC.Initialize();
        callButton.onClick.AddListener(() => { StartCoroutine(Call()); });
    }

    private void OnDestroy()
    {
        WebRTC.Dispose();
    }

    private void Start()
    {
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
        onDataChannelMessage = bytes => { textReceive.text = System.Text.Encoding.UTF8.GetString(bytes); };
        onDataChannelOpen = ()=> { sendButton.interactable = true; };
        onDataChannelClose = () => { sendButton.interactable = false; };
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
    void Pc1OnIceConnectinChange(RTCIceConnectionState state)
    {
        OnIceConnectionChange(pc1, state);
    }
    void Pc2OnIceConnectionChange(RTCIceConnectionState state)
    {
        OnIceConnectionChange(pc2, state);
    }

    void Pc1OnIceCandidate(RTCIceCandidate candidate)
    {
        OnIceCandidate(pc1, candidate);
    }
    void Pc2OnIceCandidate(RTCIceCandidate candidate)
    {
        OnIceCandidate(pc2, candidate);
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

        RTCDataChannelInit conf = new RTCDataChannelInit(true);
        dataChannel = pc1.CreateDataChannel("data", ref conf);
        dataChannel.OnOpen = onDataChannelOpen;

        Debug.Log("pc1 createOffer start");
        var op = pc1.CreateOffer(ref OfferOptions);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pc"></param>
    /// <param name="streamEvent"></param>
    void OnIceCandidate(RTCPeerConnection pc, RTCIceCandidate candidate)
    {
        GetOtherPc(pc).AddIceCandidate(ref candidate);
        Debug.Log($"{GetName(pc)} ICE candidate:\n {candidate.candidate}");
    }

    public void SendMsg()
    {
        dataChannel.Send(textSend.text);
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

        var op3 = pc2.CreateAnswer(ref AnswerOptions);
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
            yield return new WaitForSeconds(1f);

            if (!sendButton.interactable)
                continue;

            var op1 = pc1.GetStats();
            var op2 = pc2.GetStats();

            yield return op1;
            yield return op2;

            Debug.Log("pc1");
            foreach (var stat in op1.Value.Stats.Values)
            {
                Debug.Log(stat.Type.ToString());
            }
            Debug.Log("pc2");
            foreach (var stat in op2.Value.Stats.Values)
            {
                Debug.Log(stat.Type.ToString());
            }
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
