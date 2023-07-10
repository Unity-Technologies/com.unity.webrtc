using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC;
using Unity.WebRTC.Samples;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

class TrickleIceSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button addServerButton;
    [SerializeField] private Button removeServerButton;
    [SerializeField] private Button resetToDefaultButton;
    [SerializeField] private Button gatherCandidatesButton;
    [SerializeField] private InputField urlInputField;
    [SerializeField] private InputField usernameInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private GameObject optionElement;
    [SerializeField] private Transform optionParent;
    [SerializeField] private GameObject candidateElement;
    [SerializeField] private Transform candidateParent;
    [SerializeField] private ToggleGroup iceTransportOption;
    [SerializeField] private Slider candidatePoolSizeSlider;
    [SerializeField] private Text candidatePoolSizeText;

#pragma warning restore 0649

    private RTCPeerConnection _pc1;
    private RTCRtpTransceiver _transceiver;

    private float beginTime = 0f;
    private Dictionary<GameObject, RTCIceServer> iceServers
        = new Dictionary<GameObject, RTCIceServer>();

    private GameObject selectedOption = null;

    private void Awake()
    {
        addServerButton.onClick.AddListener(OnAddServer);
        removeServerButton.onClick.AddListener(OnRemoveServer);
        resetToDefaultButton.onClick.AddListener(OnResetToDefault);
        gatherCandidatesButton.onClick.AddListener(OnGatherCandidate);
        candidatePoolSizeSlider.onValueChanged.AddListener(OnChangedCandidatePoolSize);
    }

    private void Start()
    {
        OnResetToDefault();
    }

    void OnAddServer()
    {
        string url = urlInputField.text;
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        string scheme = url.Split(':')[0];

        if (scheme != "stun" && scheme != "turn" && scheme != "turns")
        {
            Debug.LogError(
                $"URI scheme `{scheme}` is not valid parameter. \n" +
                $"ex. `stun:192.168.11.1`, `turn:192.168.11.2:3478?transport=udp`");
            return;
        }
        AddServer(url, username, password);
    }

    void AddServer(string url, string username = null, string password = null)
    {
        // Store the ICE server as a stringified JSON object in option.value.
        GameObject option = Instantiate(optionElement, optionParent);
        Text optionText = option.GetComponentInChildren<Text>();
        Button optionButton = option.GetComponentInChildren<Button>();

        RTCIceServer iceServer = new RTCIceServer
        {
            urls = new[] { url },
            username = usernameInputField.text,
            credential = passwordInputField.text
        };

        optionText.text = url;

        if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
        {
            optionText.text += $"[{username}:{password}]";
        }
        optionButton.onClick.AddListener(() => OnSelectServer(option));
        iceServers.Add(option, iceServer);

        urlInputField.text = string.Empty;
        usernameInputField.text = string.Empty;
        passwordInputField.text = string.Empty;
    }

    void OnRemoveServer()
    {
        if (selectedOption == null)
            return;

        iceServers.Remove(selectedOption);
        Destroy(selectedOption);
        selectedOption = null;
    }

    void OnResetToDefault()
    {
        const string url = "stun:stun.l.google.com:19302";

        foreach (Transform child in optionParent)
        {
            Destroy(child.gameObject);
        }
        iceServers.Clear();
        AddServer(url);
    }

    void OnSelectServer(GameObject option)
    {
        selectedOption = option;
    }

    private RTCConfiguration GetSelectedSdpSemantics()
    {
        List<Toggle> toggles = iceTransportOption.ActiveToggles().ToList();
        int index = toggles.FindIndex(toggle => toggle.isOn);
        RTCIceTransportPolicy policy = 0 == index ? RTCIceTransportPolicy.All : RTCIceTransportPolicy.Relay;

        RTCConfiguration config = default;
        config.iceServers = iceServers.Values.ToArray();
        config.iceTransportPolicy = policy;
        config.iceCandidatePoolSize = (int)candidatePoolSizeSlider.value;

        return config;
    }


    IEnumerator CreateOffer(RTCPeerConnection pc)
    {
        var op = pc.CreateOffer();
        yield return op;

        if (!op.IsError)
        {
            if (pc.SignalingState != RTCSignalingState.Stable)
            {
                Debug.LogError($"signaling state is not stable.");
                yield break;
            }
            beginTime = Time.realtimeSinceStartup;
            yield return StartCoroutine(OnCreateOfferSuccess(pc, op.Desc));
        }
        else
        {
            OnCreateSessionDescriptionError(op.Error);
        }
    }

    private void OnChangedCandidatePoolSize(float value)
    {
        int value_ = (int)value;
        candidatePoolSizeText.text = value_.ToString();
    }

    private void OnGatherCandidate()
    {
        foreach (Transform child in candidateParent)
        {
            Destroy(child.gameObject);
        }
        gatherCandidatesButton.interactable = false;

        var configuration = GetSelectedSdpSemantics();
        _pc1 = new RTCPeerConnection(ref configuration);
        _pc1.OnIceCandidate = OnIceCandidate;
        _pc1.OnIceGatheringStateChange = OnIceGatheringStateChange;
        _transceiver = _pc1.AddTransceiver(TrackKind.Video);
        StartCoroutine(CreateOffer(_pc1));
    }

    private void OnIceCandidate(RTCIceCandidate candidate)
    {
        GameObject newCandidate = Instantiate(candidateElement, candidateParent);

        Text[] texts = newCandidate.GetComponentsInChildren<Text>();
        foreach (Text text in texts)
        {
            switch (text.name)
            {
                case "Time":
                    text.text = (Time.realtimeSinceStartup - beginTime).ToString("F");
                    break;
                case "Component":
                    text.text = candidate.Component.Value.ToString();
                    break;
                case "Type":
                    text.text = candidate.Type.Value.ToString();
                    break;
                case "Foundation":
                    text.text = candidate.Foundation;
                    break;
                case "Protocol":
                    text.text = candidate.Protocol.Value.ToString();
                    break;
                case "Address":
                    text.text = candidate.Address;
                    break;
                case "Port":
                    text.text = candidate.Port.ToString();
                    break;
                case "Priority":
                    text.text = FormatPriority(candidate.Priority);
                    break;
            }
        }
    }

    private void OnIceGatheringStateChange(RTCIceGatheringState state)
    {
        if (state != RTCIceGatheringState.Complete)
        {
            return;
        }

        string elapsed = (Time.realtimeSinceStartup - beginTime).ToString("F");
        GameObject newCandidate = Instantiate(candidateElement, candidateParent);

        Text[] texts = newCandidate.GetComponentsInChildren<Text>();
        foreach (Text text in texts)
        {
            switch (text.name)
            {
                case "Time":
                    text.text = (Time.realtimeSinceStartup - beginTime).ToString("F");
                    break;
                case "Priority":
                    text.text = "Done";
                    break;
                default:
                    text.text = string.Empty;
                    break;
            }
        }

        _transceiver.Dispose();
        _pc1.Close();
        _pc1 = null;
        gatherCandidatesButton.interactable = true;
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
    }

    private void OnSetLocalSuccess(RTCPeerConnection pc)
    {
        Debug.Log("SetLocalDescription complete");
    }

    static void OnSetSessionDescriptionError(ref RTCError error)
    {
        Debug.LogError($"Error Detail Type: {error.message}");
    }

    static string FormatPriority(uint priority)
    {
        return $"{priority >> 24} | {(priority >> 8) & 0xFFFF} | {priority & 0xFF}";
    }

    private static void OnCreateSessionDescriptionError(RTCError error)
    {
        Debug.LogError($"Error Detail Type: {error.message}");
    }
}
