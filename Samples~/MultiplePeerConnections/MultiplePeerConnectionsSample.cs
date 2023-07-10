using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.WebRTC.Samples
{
    class MultiplePeerConnectionsSample : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Button startButton;
        [SerializeField] private Button callButton;
        [SerializeField] private Button hangUpButton;
        [SerializeField] private Camera cam;
        [SerializeField] private RawImage sourceImage;
        [SerializeField] private RawImage receiveImage1;
        [SerializeField] private RawImage receiveImage2;
        [SerializeField] private AudioSource sourceAudio;
        [SerializeField] private AudioSource receiveAudio1;
        [SerializeField] private AudioSource receiveAudio2;
        [SerializeField] private Transform rotateObject;
        [SerializeField] private AudioClip audioclipStereoSample;
#pragma warning restore 0649

        private static RTCConfiguration configuration = new RTCConfiguration
        {
            iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } }
        };

        private RTCPeerConnection pc1Local, pc1Remote, pc2Local, pc2Remote;
        private MediaStream sourceStream;

        private void Start()
        {
            StartCoroutine(WebRTC.Update());

            startButton.onClick.AddListener(Setup);
            callButton.onClick.AddListener(Call);
            hangUpButton.onClick.AddListener(HangUp);

            startButton.interactable = true;
            callButton.interactable = false;
            hangUpButton.interactable = false;
        }

        private void Update()
        {
            if (rotateObject != null)
            {
                rotateObject.Rotate(1, 2, 3);
            }
        }

        private void Setup()
        {
            Debug.Log("Set up source/receive streams");
            sourceStream = new MediaStream();

            var videoTrack = cam.CaptureStreamTrack(WebRTCSettings.StreamSize.x, WebRTCSettings.StreamSize.y);
            sourceStream.AddTrack(videoTrack);
            sourceImage.texture = cam.targetTexture;

            sourceAudio.clip = audioclipStereoSample;
            sourceAudio.loop = true;
            sourceAudio.Play();
            var audioTrack = new AudioStreamTrack(sourceAudio);
            sourceStream.AddTrack(audioTrack);

            startButton.interactable = false;
            callButton.interactable = true;
        }

        private void Call()
        {
            Debug.Log("Starting calls");

            pc1Local = new RTCPeerConnection(ref configuration);
            pc1Remote = new RTCPeerConnection(ref configuration);
            pc1Remote.OnTrack = e =>
            {
                if (e.Track is VideoStreamTrack videoTrack)
                {
                    videoTrack.OnVideoReceived += tex =>
                    {
                        receiveImage1.texture = tex;
                    };
                }

                if (e.Track is AudioStreamTrack audioTrack)
                {
                    receiveAudio1.SetTrack(audioTrack);
                    receiveAudio1.loop = true;
                    receiveAudio1.Play();
                }
            };
            pc1Local.OnIceCandidate = candidate => pc1Remote.AddIceCandidate(candidate);
            pc1Remote.OnIceCandidate = candidate => pc1Local.AddIceCandidate(candidate);
            Debug.Log("pc1: created local and remote peer connection object");

            pc2Local = new RTCPeerConnection(ref configuration);
            pc2Remote = new RTCPeerConnection(ref configuration);
            pc2Remote.OnTrack = e =>
            {
                if (e.Track is VideoStreamTrack videoTrack)
                {
                    videoTrack.OnVideoReceived += tex =>
                    {
                        receiveImage2.texture = tex;
                    };
                }

                if (e.Track is AudioStreamTrack audioTrack)
                {
                    receiveAudio2.SetTrack(audioTrack);
                    receiveAudio2.loop = true;
                    receiveAudio2.Play();
                }
            };
            pc2Local.OnIceCandidate = candidate => pc2Remote.AddIceCandidate(candidate);
            pc2Remote.OnIceCandidate = candidate => pc2Local.AddIceCandidate(candidate);
            Debug.Log("pc2: created local and remote peer connection object");

            var pc1VideoSenders = new List<RTCRtpSender>();
            var pc2VideoSenders = new List<RTCRtpSender>();
            foreach (var track in sourceStream.GetTracks())
            {
                var pc1Sender = pc1Local.AddTrack(track, sourceStream);
                var pc2Sender = pc2Local.AddTrack(track, sourceStream);

                if (track.Kind == TrackKind.Video)
                {
                    pc1VideoSenders.Add(pc1Sender);
                    pc2VideoSenders.Add(pc2Sender);
                }
            }

            if (WebRTCSettings.UseVideoCodec != null)
            {
                var codecs = new[] { WebRTCSettings.UseVideoCodec };
                foreach (var transceiver in pc1Local.GetTransceivers())
                {
                    if (pc1VideoSenders.Contains(transceiver.Sender))
                    {
                        transceiver.SetCodecPreferences(codecs);
                    }
                }

                foreach (var transceiver in pc2Local.GetTransceivers())
                {
                    if (pc2VideoSenders.Contains(transceiver.Sender))
                    {
                        transceiver.SetCodecPreferences(codecs);
                    }
                }
            }

            Debug.Log("Adding local stream to pc1Local/pc2Local");

            StartCoroutine(NegotiationPeer(pc1Local, pc1Remote));
            StartCoroutine(NegotiationPeer(pc2Local, pc2Remote));

            callButton.interactable = false;
            hangUpButton.interactable = true;
        }

        private void HangUp()
        {
            foreach (var track in sourceStream.GetTracks())
            {
                track.Dispose();
            }
            sourceStream.Dispose();
            sourceStream = null;
            pc1Local.Close();
            pc1Remote.Close();
            pc2Local.Close();
            pc2Remote.Close();
            pc1Local.Dispose();
            pc1Remote.Dispose();
            pc2Local.Dispose();
            pc2Remote.Dispose();
            pc1Local = null;
            pc1Remote = null;
            pc2Local = null;
            pc2Remote = null;

            sourceImage.texture = null;
            sourceAudio.Stop();
            sourceAudio.clip = null;
            receiveImage1.texture = null;
            receiveAudio1.Stop();
            receiveAudio1.clip = null;
            receiveImage2.texture = null;
            receiveAudio2.Stop();
            receiveAudio2.clip = null;

            startButton.interactable = true;
            callButton.interactable = false;
            hangUpButton.interactable = false;
        }

        private static void OnCreateSessionDescriptionError(RTCError error)
        {
            Debug.LogError($"Failed to create session description: {error.message}");
        }

        private static IEnumerator NegotiationPeer(RTCPeerConnection localPeer, RTCPeerConnection remotePeer)
        {
            var opCreateOffer = localPeer.CreateOffer();
            yield return opCreateOffer;

            if (opCreateOffer.IsError)
            {
                OnCreateSessionDescriptionError(opCreateOffer.Error);
                yield break;
            }

            var offerDesc = opCreateOffer.Desc;
            yield return localPeer.SetLocalDescription(ref offerDesc);
            Debug.Log($"Offer from LocalPeer \n {offerDesc.sdp}");
            yield return remotePeer.SetRemoteDescription(ref offerDesc);

            var opCreateAnswer = remotePeer.CreateAnswer();
            yield return opCreateAnswer;

            if (opCreateAnswer.IsError)
            {
                OnCreateSessionDescriptionError(opCreateAnswer.Error);
                yield break;
            }

            var answerDesc = opCreateAnswer.Desc;
            yield return remotePeer.SetLocalDescription(ref answerDesc);
            Debug.Log($"Answer from RemotePeer \n {answerDesc.sdp}");
            yield return localPeer.SetRemoteDescription(ref answerDesc);
        }
    }
}
