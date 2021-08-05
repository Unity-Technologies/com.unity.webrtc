using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Unity.WebRTC.Samples
{
    class MultiAudioReceiveSample : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Dropdown dropdownAudioclip;
        [SerializeField] private Button callButton;
        [SerializeField] private Button hangUpButton;
        [SerializeField] private Button addAudioObjectButton;
        [SerializeField] private Button addTracksButton;
        [SerializeField] private Transform sourceObjectParent;
        [SerializeField] private Transform receiveObjectParent;
        [SerializeField] private List<AudioSource> sourceObjectList;
        [SerializeField] private List<AudioSource> receiveObjectList;
        [SerializeField] private AudioSource audioObjectPrefab;
        [SerializeField] private List<AudioClip> sourceAudioClips;
#pragma warning restore 0649

        private RTCPeerConnection _pc1, _pc2;
        private List<AudioStreamTrack> audioStreamTrackList;
        private List<RTCRtpSender> sendingSenderList;
        private DelegateOnIceCandidate pc1OnIceCandidate;
        private DelegateOnIceCandidate pc2OnIceCandidate;
        private DelegateOnTrack pc2Ontrack;
        private DelegateOnNegotiationNeeded pc1OnNegotiationNeeded;
        private DelegateOnNegotiationNeeded pc2OnNegotiationNeeded;
        private int objectIndex = 0;
        private int audioIndex = 0;

        private void Awake()
        {
            WebRTC.Initialize(WebRTCSettings.EncoderType, WebRTCSettings.LimitTextureSize);
            StartCoroutine(WebRTC.Update());
            callButton.onClick.AddListener(Call);
            hangUpButton.onClick.AddListener(HangUp);
            addAudioObjectButton.onClick.AddListener(AddVideoObject);
            addTracksButton.onClick.AddListener(AddTracks);
        }

        private void OnDestroy()
        {
            WebRTC.Dispose();
        }

        private void Start()
        {
            audioStreamTrackList = new List<AudioStreamTrack>();
            sendingSenderList = new List<RTCRtpSender>();
            callButton.interactable = true;
            hangUpButton.interactable = false;
            dropdownAudioclip.options = sourceAudioClips.Select(clip => new Dropdown.OptionData(clip.name)).ToList();

            pc1OnIceCandidate = candidate => { OnIceCandidate(_pc1, candidate); };
            pc2OnIceCandidate = candidate => { OnIceCandidate(_pc2, candidate); };
            pc2Ontrack = e =>
            {
                if (e.Track is AudioStreamTrack track)
                {
                    var outputAudioSource = receiveObjectList[audioIndex];
                    track.OnAudioReceived += clip =>
                    {
                        outputAudioSource.clip = clip;
                        outputAudioSource.loop = true;
                        outputAudioSource.Play();
                    };
                    audioIndex++;
                }
            };

            pc1OnNegotiationNeeded = () => { StartCoroutine(PeerNegotiationNeeded(_pc1)); };
            pc2OnNegotiationNeeded = () => { StartCoroutine(PeerNegotiationNeeded(_pc2)); };
        }

        private static RTCConfiguration GetSelectedSdpSemantics()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};

            return config;
        }

        IEnumerator PeerNegotiationNeeded(RTCPeerConnection pc)
        {
            Debug.Log($"{GetName(pc)} createOffer start");
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

        private void AddVideoObject()
        {
            var newSource = Instantiate(audioObjectPrefab, sourceObjectParent, false);
            newSource.name = $"SourceAudioObject{objectIndex}";
            newSource.loop = true;
            newSource.clip = sourceAudioClips[dropdownAudioclip.value];
            newSource.Play();

            sourceObjectList.Add(newSource);
            var newReceive = Instantiate(audioObjectPrefab, receiveObjectParent, false);
            newReceive.name = $"ReceiveAudioObject{objectIndex}";
            receiveObjectList.Add(newReceive);

            try
            {
                audioStreamTrackList.Add(new AudioStreamTrack(newSource));
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                HangUp();
                return;
            }

            objectIndex++;
            addTracksButton.interactable = true;
        }

        private void Call()
        {
            callButton.interactable = false;
            hangUpButton.interactable = true;
            addAudioObjectButton.interactable = true;
            addTracksButton.interactable = false;

            Debug.Log("GetSelectedSdpSemantics");
            var configuration = GetSelectedSdpSemantics();
            _pc1 = new RTCPeerConnection(ref configuration);
            Debug.Log("Created local peer connection object pc1");
            _pc1.OnIceCandidate = pc1OnIceCandidate;
            _pc1.OnNegotiationNeeded = pc1OnNegotiationNeeded;

            _pc2 = new RTCPeerConnection(ref configuration);
            Debug.Log("Created remote peer connection object pc2");
            _pc2.OnIceCandidate = pc2OnIceCandidate;
            _pc2.OnTrack = pc2Ontrack;
            _pc2.OnNegotiationNeeded = pc2OnNegotiationNeeded;
        }

        private void AddTracks()
        {
            Debug.Log("Add not added tracks");
            foreach (var track in audioStreamTrackList.Where(x =>
                !sendingSenderList.Exists(y => y.Track.Id == x.Id)))
            {
                var sender = _pc1.AddTrack(track);
                sendingSenderList.Add(sender);
            }
        }

        private void HangUp()
        {
            foreach (var audioSource in sourceObjectList.Concat(receiveObjectList))
            {
                DestroyImmediate(audioSource.gameObject);
            }

            sourceObjectList.Clear();
            receiveObjectList.Clear();

            foreach (var track in audioStreamTrackList)
            {
                track.Dispose();
            }

            audioStreamTrackList.Clear();
            sendingSenderList.Clear();

            _pc1.Close();
            _pc2.Close();
            Debug.Log("Close local/remote peer connection");
            _pc1.Dispose();
            _pc2.Dispose();
            _pc1 = null;
            _pc2 = null;
            audioIndex = 0;
            objectIndex = 0;
            callButton.interactable = true;
            hangUpButton.interactable = false;
            addAudioObjectButton.interactable = false;
            addTracksButton.interactable = false;
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

            Debug.Log($"{GetName(otherPc)} createAnswer start");
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

        static void OnSetSessionDescriptionError(ref RTCError error)
        {
            Debug.LogError($"Error Detail Type: {error.message}");
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
}
