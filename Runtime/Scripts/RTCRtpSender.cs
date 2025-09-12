using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.WebRTC
{
    /// <summary>
    ///     Provides the ability to control and access to details on encoding and sending a MediaStreamTrack to a remote peer.
    /// </summary>
    /// <remarks>
    ///     `RTCRtpSender` class allows customization of media encoding and transmission to a remote peer.
    ///     It provides access to the device's media capabilities and supports sending DTMF tones for telephony interactions.
    /// </remarks>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         var senders = peerConnection.GetSenders();
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCPeerConnection" />
    public class RTCRtpSender : RefCountedObject
    {
        private RTCPeerConnection peer;
        private RTCRtpTransform transform;

        internal RTCRtpSender(IntPtr ptr, RTCPeerConnection peer) : base(ptr)
        {
            WebRTC.Table.Add(self, this);
            this.peer = peer;
        }

        /// <summary>
        ///     Finalizer for RTCRtpSender.
        /// </summary>
        /// <remarks>
        ///     Ensures that resources are released by calling the `Dispose` method
        /// </remarks>
        ~RTCRtpSender()
        {
            this.Dispose();
        }

        /// <summary>
        ///     Disposes of RTCRtpSender.
        /// </summary>
        /// <remarks>
        ///     `Dispose` method disposes of the `RTCRtpSender` and releases the associated resources. 
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         sender.Dispose();
        ///     ]]></code>
        /// </example>
        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }

        /// <summary>
        ///     Provides a `RTCRtpCapabilities` object describing the codec and header extension capabilities.
        /// </summary>
        /// <remarks>
        ///     `GetCapabilities` method provides a `RTCRtpCapabilities` object that describes the codec and header extension capabilities supported by `RTCRtpSender`.
        /// </remarks>
        /// <param name="kind">`TrackKind` value indicating the type of media.</param>
        /// <returns>`RTCRtpCapabilities` object contains an array of `RTCRtpCodecCapability` objects.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpCapabilities capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
        ///         RTCRtpTransceiver transceiver = peerConnection.GetTransceivers().First();
        ///         RTCErrorType error = transceiver.SetCodecPreferences(capabilities.codecs);
        ///         if (error.errorType != RTCErrorType.None)
        ///         {
        ///             Debug.LogError($"Failed to set codec preferences: {error.message}");
        ///         }
        ///     ]]></code>
        /// </example>
        public static RTCRtpCapabilities GetCapabilities(TrackKind kind)
        {
            WebRTC.Context.GetSenderCapabilities(kind, out IntPtr ptr);
            RTCRtpCapabilitiesInternal capabilitiesInternal =
                Marshal.PtrToStructure<RTCRtpCapabilitiesInternal>(ptr);
            RTCRtpCapabilities capabilities = new RTCRtpCapabilities(capabilitiesInternal);
            Marshal.FreeHGlobal(ptr);
            return capabilities;
        }

        /// <summary>
        ///     Asynchronously requests statistics about outgoing traffic on the RTCPeerConnection associated with the RTCRtpSender.
        /// </summary>
        /// <remarks>
        ///     `GetStats` method asynchronously requests an `RTCStatsReport` containing statistics about the outgoing traffic for the `RTCPeerConnection` associated with the `RTCRtpSender`.
        /// </remarks>
        /// <returns>`RTCStatsReportAsyncOperation` object containing `RTCStatsReport` object.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCStatsReportAsyncOperation asyncOperation = sender.GetStats();
        ///         yield return asyncOperation;
        ///         
        ///         if (!asyncOperation.IsError)
        ///         {
        ///             RTCStatsReport statsReport = asyncOperation.Value;
        ///             RTCStats stats = statsReport.Stats.ElementAt(0).Value;
        ///             string statsText = "Id:" + stats.Id + "\n";
        ///             statsText += "Timestamp:" + stats.Timestamp + "\n";
        ///             statsText += stats.Dict.Aggregate(string.Empty, (str, next) =>
        ///                 str + next.Key + ":" + (next.Value == null ? string.Empty : next.Value.ToString()) + "\n");
        ///             Debug.Log(statsText);
        ///             statsReport.Dispose();
        ///         }
        ///     ]]></code>
        /// </example>
        public RTCStatsReportAsyncOperation GetStats()
        {
            return peer.GetStats(this);
        }

        /// <summary>
        ///      <see cref="MediaStreamTrack"/> managed by RTCRtpSender. If it is null, no transmission occurs.
        /// </summary>
        public MediaStreamTrack Track
        {
            get
            {
                IntPtr ptr = NativeMethods.SenderGetTrack(GetSelfOrThrow());
                if (ptr == IntPtr.Zero)
                    return null;
                return WebRTC.FindOrCreate(ptr, MediaStreamTrack.Create);
            }
        }

        /// <summary>
        ///     <see cref="RTCRtpScriptTransform"/> used to insert a transform stream in a worker thread into the sender pipeline,
        ///     enabling transformations on encoded video and audio frames after output by a codec but before transmission.
        /// </summary>
        public RTCRtpTransform Transform
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // cache reference
                transform = value;
                NativeMethods.SenderSetTransform(GetSelfOrThrow(), value.self);
            }
            get
            {
                return transform;
            }
        }

        /// <summary>
        ///     Indicates if the video track's framerate is synchronized with the application's framerate.
        /// </summary>
        public bool SyncApplicationFramerate
        {
            get
            {
                if (Track is VideoStreamTrack videoTrack)
                {
                    if (videoTrack.m_source == null)
                    {
                        throw new InvalidOperationException("This track doesn't have a video source.");
                    }
                    return videoTrack.m_source.SyncApplicationFramerate;
                }
                else
                {
                    throw new InvalidOperationException("This track is not VideoStreamTrack.");
                }
            }
            set
            {
                if (Track is VideoStreamTrack videoTrack)
                {
                    if (videoTrack.m_source == null)
                    {
                        throw new InvalidOperationException("This track doesn't have a video source.");
                    }
                    videoTrack.m_source.SyncApplicationFramerate = value;
                }
                else
                {
                    throw new InvalidOperationException("This track is not VideoStreamTrack.");
                }
            }
        }

        /// <summary>
        ///     Retrieves the current configuration of the RTCRtpSender.
        /// </summary>
        /// <remarks>
        ///     `GetParameters` method retrieves `RTCRtpSendParameters` object describing the current configuration of the `RTCRtpSender`.
        /// </remarks>
        /// <returns>`RTCRtpSendParameters` object containing the current configuration of the `RTCRtpSender`.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpSender sender = peerConnection.GetSenders().First();
        ///         RTCRtpSendParameters parameters = sender.GetParameters();
        ///         parameters.encodings[0].maxBitrate = bandwidth * 1000;
        ///         RTCError error = sender.SetParameters(parameters);
        ///         if (error.errorType != RTCErrorType.None)
        ///         {
        ///             Debug.LogError($"Failed to set parameters: {error.message}");
        ///         }
        ///     ]]></code>
        /// </example>
        /// <seealso cref="SetParameters" />
        public RTCRtpSendParameters GetParameters()
        {
            NativeMethods.SenderGetParameters(GetSelfOrThrow(), out var ptr);
            RTCRtpSendParametersInternal parametersInternal = Marshal.PtrToStructure<RTCRtpSendParametersInternal>(ptr);
            RTCRtpSendParameters parameters = new RTCRtpSendParameters(ref parametersInternal);
            Marshal.FreeHGlobal(ptr);
            return parameters;
        }

        /// <summary>
        ///     Updates the configuration of the sender's track.
        /// </summary>
        /// <remarks>
        ///     `SetParameters` method updates the configuration of the sender's `MediaStreamTrack`
        ///     by applying changes the RTP transmission and the encoding parameters for a specific outgoing media on the connection.
        /// </remarks>
        /// <param name="parameters">
        ///     A `RTCRtpSendParameters` object previously obtained by calling the sender's `GetParameters`,
        ///     includes desired configuration changes and potential codecs for encoding the sender's track.
        /// </param>
        /// <returns>`RTCError` value.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpSender sender = peerConnection.GetSenders().First();
        ///         RTCRtpSendParameters parameters = sender.GetParameters();
        ///         parameters.encodings[0].maxBitrate = bandwidth * 1000;
        ///         RTCError error = sender.SetParameters(parameters);
        ///         if (error.errorType != RTCErrorType.None)
        ///         {
        ///             Debug.LogError($"Failed to set parameters: {error.message}");
        ///         }
        ///     ]]></code>
        /// </example>
        /// <seealso cref="GetParameters" />
        public RTCError SetParameters(RTCRtpSendParameters parameters)
        {
            if (Track is VideoStreamTrack videoTrack)
            {
                foreach (var encoding in parameters.encodings)
                {
                    var scale = encoding.scaleResolutionDownBy;
                    if (!scale.HasValue)
                    {
                        continue;
                    }

                    var error = WebRTC.ValidateTextureSize((int)(videoTrack.Texture.width / scale),
                        (int)(videoTrack.Texture.height / scale), Application.platform);
                    if (error.errorType != RTCErrorType.None)
                    {
                        return error;
                    }
                }
            }

            parameters.CreateInstance(out RTCRtpSendParametersInternal instance);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(instance));
            Marshal.StructureToPtr(instance, ptr, false);
            RTCErrorType type = NativeMethods.SenderSetParameters(GetSelfOrThrow(), ptr);
            Marshal.FreeCoTaskMem(ptr);
            return new RTCError { errorType = type };
        }

        /// <summary>
        ///     Replaces the current source track with a new MediaStreamTrack.
        /// </summary>
        /// <remarks>
        ///    `ReplaceTrack` method replaces the track currently being used as the sender's source with a new `MediaStreamTrack`.
        ///    It is often used to switch between two cameras.
        /// </remarks>
        /// <param name="track">
        ///     A `MediaStreamTrack` to replace the current source track of the `RTCRtpSender`.
        ///     The new track must be the same type as the current one.
        /// </param>
        /// <returns>`true` if the track has been successfully replaced.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpTransceiver transceiver = peerConnection.GetTransceivers().First();
        ///         transceiver.Sender.ReplaceTrack(newTrack);
        ///     ]]></code>
        /// </example>
        public bool ReplaceTrack(MediaStreamTrack track)
        {
            IntPtr trackPtr = track?.GetSelfOrThrow() ?? IntPtr.Zero;
            return NativeMethods.SenderReplaceTrack(GetSelfOrThrow(), trackPtr);
        }
    }
}
