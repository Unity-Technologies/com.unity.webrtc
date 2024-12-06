using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.WebRTC
{
    /// <summary>
    /// Provides configuration options for the data channel.
    /// </summary>
    /// <seealso cref="RTCPeerConnection.CreateDataChannel(string, RTCDataChannelInit)"/>
    public class RTCDataChannelInit
    {
        /// <summary>
        /// Indicates whether or not the data channel guarantees in-order delivery of messages.
        /// </summary>
        public bool? ordered;
        /// <summary>
        /// Represents the maximum number of milliseconds that attempts to transfer a message may take in unreliable mode..
        /// </summary>
        /// <remarks>
        /// Cannot be set along with <see cref="RTCDataChannelInit.maxRetransmits"/>.
        /// </remarks>
        /// <seealso cref="RTCDataChannelInit.maxRetransmits"/>
        public int? maxPacketLifeTime;
        /// <summary>
        /// Represents the maximum number of times the user agent should attempt to retransmit a message which fails the first time in unreliable mode.
        /// </summary>
        /// <remarks>
        /// Cannot be set along with <see cref="RTCDataChannelInit.maxPacketLifeTime"/>.
        /// </remarks>
        /// <seealso cref="RTCDataChannelInit.maxPacketLifeTime"/>
        public int? maxRetransmits;
        /// <summary>
        /// Provides the name of the sub-protocol being used on the RTCDataChannel.
        /// </summary>
        public string protocol;
        /// <summary>
        /// Indicates whether the RTCDataChannel's connection is negotiated by the Web app or by the WebRTC layer.
        /// </summary>
        public bool? negotiated;
        /// <summary>
        /// Indicates a 16-bit numeric ID for the channel.
        /// </summary>
        public int? id;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCDataChannelInitInternal
    {
        public OptionalBool ordered;
        public OptionalInt maxRetransmitTime;
        public OptionalInt maxRetransmits;
        [MarshalAs(UnmanagedType.LPStr)]
        public string protocol;
        public OptionalBool negotiated;
        public OptionalInt id;

        public static explicit operator RTCDataChannelInitInternal(RTCDataChannelInit origin)
        {
            RTCDataChannelInitInternal dst = new RTCDataChannelInitInternal
            {
                ordered = origin.ordered,
                maxRetransmitTime = origin.maxPacketLifeTime,
                maxRetransmits = origin.maxRetransmits,
                protocol = origin.protocol,
                negotiated = origin.negotiated,
                id = origin.id
            };
            return dst;
        }
    }

    /// <summary>
    /// Represents type of delegate to be called when WebRTC open event is sent.
    /// </summary>
    /// <remarks>
    /// The WebRTC open event is sent to an RTCDataChannel object's onopen event handler when the underlying transport used to send and receive the data channel's messages is opened or reopened.
    /// This event is not cancelable and does not bubble.
    /// </remarks>
    /// <seealso cref="RTCDataChannel.OnOpen"/>
    public delegate void DelegateOnOpen();

    /// <summary>
    /// Represents type of delegate to be called when RTCDataChannel close event is sent.
    /// </summary>
    /// <remarks>
    /// The close event is sent to the onclose event handler on an RTCDataChannel instance when the data transport for the data channel has closed.
    /// Before any further data can be transferred using RTCDataChannel, a new 'RTCDataChannel' instance must be created.
    /// This event is not cancelable and does not bubble.
    /// </remarks>
    /// <seealso cref="RTCDataChannel.OnClose"/>
    public delegate void DelegateOnClose();

    /// <summary>
    /// Represents type of delegate to be called when RTCDataChannel message event is sent.
    /// </summary>
    /// <remarks>
    /// The WebRTC message event is sent to the onmessage event handler on an RTCDataChannel object when a message has been received from the remote peer.
    /// </remarks>
    /// <param name="bytes"></param>
    /// <seealso cref="RTCDataChannel.OnMessage"/>
    public delegate void DelegateOnMessage(byte[] bytes);

    /// <summary>
    /// Represents type of delegate to be called when RTCPeerConnection datachannel event is sent.
    /// </summary>
    /// <remarks>
    /// A datachannel event is sent to an RTCPeerConnection instance when an RTCDataChannel has been added to the connection,
    /// as a result of the remote peer calling RTCPeerConnection.createDataChannel().
    /// </remarks>
    /// <param name="channel"></param>
    /// <seealso cref="RTCPeerConnection.OnDataChannel"/>
    public delegate void DelegateOnDataChannel(RTCDataChannel channel);

    /// <summary>
    /// Delegate to be called when RTCPeerConnection error event is sent.
    /// </summary>
    /// <remarks>
    /// A WebRTC error event is sent to an RTCDataChannel object's onerror event handler when an error occurs on the data channel.
    /// The RTCErrorEvent object provides details about the error that occurred; see that article for details.
    /// This event is not cancelable and does not bubble.
    /// </remarks>
    /// <seealso cref="RTCDataChannel.OnError"/>
    public delegate void DelegateOnError(RTCError error);

    /// <summary>
    /// Represents a network channel which can be used for bidirectional peer-to-peer transfers of arbitrary data.
    /// </summary>
    /// <remarks>
    /// RTCDataChannel interface represents a network channel which can be used for bidirectional peer-to-peer transfers of arbitrary data.
    /// Every data channel is associated with an RTCPeerConnection, and each peer connection can have up to a theoretical maximum of 65,534 data channels.
    ///
    /// To create a data channel and ask a remote peer to join you, call the RTCPeerConnection's createDataChannel() method.
    /// The peer being invited to exchange data receives a datachannel event (which has type RTCDataChannelEvent) to let it know the data channel has been added to the connection.
    /// </remarks>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         var initOption = new RTCDataChannelInit();
    ///         var peerConnection = new RTCPeerConnection();
    ///         var dataChannel = peerConnection.createDataChannel("test channel", initOption);
    ///
    ///         dataChannel.OnMessage = (event) => {
    ///             Debug.LogFormat("Received: {0}.",${event.data});
    ///         };
    ///
    ///         dataChannel.OnOpen = () => {
    ///             Debug.Log("DataChannel opened.");
    ///         };
    ///
    ///         dataChannel.OnClose = () => {
    ///             Debug.Log("DataChannel closed.");
    ///         };
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCPeerConnection.CreateDataChannel(string, RTCDataChannelInit)"/>
    public class RTCDataChannel : RefCountedObject
    {
        private DelegateOnMessage onMessage;
        private DelegateOnOpen onOpen;
        private DelegateOnClose onClose;
        private DelegateOnError onError;

        /// <summary>
        /// Delegate to be called when a message has been received from the remote peer.
        /// </summary>
        /// <remarks>
        /// The WebRTC message event is sent to the onmessage event handler on an RTCDataChannel object when a message has been received from the remote peer.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelMessageExample : MonoBehaviour
        ///         {
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 dataChannel.OnMessage = (e) => {
        ///                     Debug.LogFormat("Received: {0}.", e.data);
        ///                 };
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public DelegateOnMessage OnMessage
        {
            get => onMessage;
            set => onMessage = value;
        }

        /// <summary>
        /// Delegate to be called when the data channel's messages is opened or reopened.
        /// </summary>
        /// <remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelOpenExample : MonoBehaviour
        ///         {
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 dataChannel.OnOpen = () => {
        ///                     Debug.Log("DataChannel opened.");
        ///                 };
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public DelegateOnOpen OnOpen
        {
            get => onOpen;
            set => onOpen = value;
        }

        /// <summary>
        /// Delegate to be called when the data channel's messages is closed.
        /// </summary>
        /// <remarks>
        /// The close event is sent to the onclose event handler on an RTCDataChannel instance when the data transport for the data channel has closed.
        /// Before any further data can be transferred using RTCDataChannel, a new 'RTCDataChannel' instance must be created.
        /// This event is not cancelable and does not bubble.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelCloseExample : MonoBehaviour
        ///         {
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 dataChannel.OnClose = () => {
        ///                     Debug.Log("DataChannel closed.");
        ///                 };
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public DelegateOnClose OnClose
        {
            get => onClose;
            set => onClose = value;
        }

        /// <summary>
        /// Delegate to be called when the errors occur.
        /// </summary>
        /// <remarks>
        /// A WebRTC error event is sent to an RTCDataChannel object's onerror event handler when an error occurs on the data channel.
        /// The RTCErrorEvent object provides details about the error that occurred; see that article for details.
        /// This event is not cancelable and does not bubble.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelErrorExample : MonoBehaviour
        ///         {
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 dataChannel.OnError = (e) => {
        ///                     Debug.LogError("DataChannel error: " + e.message);
        ///                 };
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public DelegateOnError OnError
        {
            get => onError;
            set => onError = value;
        }

        /// <summary>
        /// Returns an ID number (between 0 and 65,534) which uniquely identifies the RTCDataChannel.
        /// </summary>
        /// <remarks>
        /// This ID is set at the time the data channel is created, either by the user agent (if RTCDataChannel.negotiated is false) or by the site or app script (if negotiated is true).
        /// Each RTCPeerConnection can therefore have up to a theoretical maximum of 65,534 data channels on it.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelIdExample : MonoBehaviour
        ///         {
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 Debug.Log("DataChannel ID: " + dataChannel.Id);
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public int Id => NativeMethods.DataChannelGetID(GetSelfOrThrow());

        /// <summary>
        /// Returns a string containing a name describing the data channel which are not required to be unique.
        /// </summary>
        /// <remarks>
        /// You may use the label as you wish; you could use it to identify all the channels that are being used for the same purpose, by giving them all the same name.
        /// Or you could give each channel a unique label for tracking purposes. It's entirely up to the design decisions made when building your site or app.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelLabelExample : MonoBehaviour
        ///         {
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 Debug.Log("DataChannel Label: " + dataChannel.Label);
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public string Label => NativeMethods.DataChannelGetLabel(GetSelfOrThrow()).AsAnsiStringWithFreeMem();

        /// <summary>
        /// Returns a string containing a name describing the data channel. These labels are not required to be unique.
        /// </summary>
        /// <remarks>
        /// You may use the label as you wish; you could use it to identify all the channels that are being used for the same purpose, by giving them all the same name.
        /// Or you could give each channel a unique label for tracking purposes. It's entirely up to the design decisions made when building your site or app.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelProtocolExample : MonoBehaviour
        ///         {
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 Debug.Log("DataChannel Protocol: " + dataChannel.Protocol);
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public string Protocol => NativeMethods.DataChannelGetProtocol(GetSelfOrThrow()).AsAnsiStringWithFreeMem();

        /// <summary>
        /// Returns the maximum number of times the browser should try to retransmit a message before giving up.
        /// </summary>
        /// <remarks>
        /// As set when the data channel was created, or null, which indicates that there is no maximum.
        /// This can only be set when the RTCDataChannel is created by calling RTCPeerConnection.createDataChannel(), using the maxRetransmits field in the specified options.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelMaxRetransmitsExample : MonoBehaviour
        ///         {
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit
        ///                 {
        ///                     MaxRetransmits = 10
        ///                 };
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 Debug.Log("DataChannel MaxRetransmits: " + dataChannel.MaxRetransmits);
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public ushort MaxRetransmits => NativeMethods.DataChannelGetMaxRetransmits(GetSelfOrThrow());

        /// <summary>
        /// Returns the amount of time, in milliseconds, the browser is allowed to take to attempt to transmit a message, as set when the data channel was created, or null.
        /// </summary>
        /// <remarks>
        /// This limits how long the browser can continue to attempt to transmit and retransmit the message before giving up.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelMaxRetransmitTimeExample : MonoBehaviour
        ///         {
        ///             private void Start()
        ///             {
        ///                 // Create an instance of RTCDataChannelInit with a specific MaxRetransmitTime
        ///                 var initOption = new RTCDataChannelInit
        ///                 {
        ///                     MaxRetransmitTime = 5000 // Set the maximum retransmit time in milliseconds
        ///                 };
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 // Log the MaxRetransmitTime of the data channel
        ///                 Debug.Log("DataChannel MaxRetransmitTime: " + dataChannel.MaxRetransmitTime);
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public ushort MaxRetransmitTime => NativeMethods.DataChannelGetMaxRetransmitTime(GetSelfOrThrow());

        /// <summary>
        /// Indicates whether or not the data channel guarantees in-order delivery of messages.
        /// </summary>
        /// <remarks>
        /// The default is true, which indicates that the data channel is indeed ordered.
        /// This is set when the RTCDataChannel is created, by setting the ordered property on the object passed as RTCPeerConnection.createDataChannel()'s options parameter.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelOrderedExample : MonoBehaviour
        ///         {
        ///             private void Start()
        ///             {
        ///                 // Create an instance of RTCDataChannelInit with the Ordered property set
        ///                 var initOption = new RTCDataChannelInit
        ///                 {
        ///                     Ordered = false // Set to false if you don't require reliable and ordered delivery
        ///                 };
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 // Log the Ordered property of the data channel
        ///                 Debug.Log("DataChannel Ordered: " + dataChannel.Ordered);
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public bool Ordered => NativeMethods.DataChannelGetOrdered(GetSelfOrThrow());

        /// <summary>
        /// Returns the number of bytes of data currently queued to be sent over the data channel.
        /// </summary>
        /// <remarks>
        /// The queue may build up as a result of calls to the send() method.
        /// This only includes data buffered by the user agent itself;
        /// it doesn't include any framing overhead or buffering done by the operating system or network hardware.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelBufferedAmountExample : MonoBehaviour
        ///         {
        ///             private RTCDataChannel dataChannel;
        ///
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 // Periodically check the BufferedAmount
        ///                 InvokeRepeating("CheckBufferedAmount", 1.0f, 1.0f);
        ///             }
        ///
        ///             private void CheckBufferedAmount()
        ///             {
        ///                 // Log the BufferedAmount of the data channel
        ///                 Debug.Log("DataChannel BufferedAmount: " + dataChannel.BufferedAmount);
        ///             }
        ///
        ///             private void OnDestroy()
        ///             {
        ///                 // Ensure to clean up the data channel
        ///                 if (dataChannel != null)
        ///                 {
        ///                     dataChannel.Close();
        ///                 }
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public ulong BufferedAmount => NativeMethods.DataChannelGetBufferedAmount(GetSelfOrThrow());

        /// <summary>
        /// Indicates whether the RTCDataChannel's connection is negotiated by the Web app or by the WebRTC layer.
        /// </summary>
        /// <remarks>
        /// True is for Web App and the False is for WebRTC layer. The default is false.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelNegotiatedExample : MonoBehaviour
        ///         {
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit
        ///                 {
        ///                     Negotiated = true // Set this to true if manually negotiating the channel
        ///                 };
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 // Log the Negotiated property of the data channel
        ///                 Debug.Log("DataChannel Negotiated: " + dataChannel.Negotiated);
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public bool Negotiated => NativeMethods.DataChannelGetNegotiated(GetSelfOrThrow());

        /// <summary>
        /// Returns an enum of the <c>RTCDataChannelState</c> which shows
        /// the state of the channel.
        /// </summary>
        /// <remarks>
        /// <see cref="Send(string)"/> method must be called when the state is <b>Open</b>.
        /// </remarks>
        /// <seealso cref="RTCDataChannelState"/>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelReadyStateExample : MonoBehaviour
        ///         {
        ///             private RTCDataChannel dataChannel;
        ///
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 // Log the initial ReadyState of the data channel
        ///                 Debug.Log("DataChannel ReadyState: " + dataChannel.ReadyState);
        ///
        ///                 // Optionally, you can periodically check the ReadyState
        ///                 InvokeRepeating("CheckReadyState", 1.0f, 1.0f);
        ///             }
        ///
        ///             private void CheckReadyState()
        ///             {
        ///                 // Log the current ReadyState of the data channel
        ///                 Debug.Log("DataChannel ReadyState: " + dataChannel.ReadyState);
        ///             }
        ///
        ///             private void OnDestroy()
        ///             {
        ///                 // Ensure to clean up the data channel
        ///                 if (dataChannel != null)
        ///                 {
        ///                     dataChannel.Close();
        ///                 }
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public RTCDataChannelState ReadyState => NativeMethods.DataChannelGetReadyState(GetSelfOrThrow());

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnMessage))]
        static void DataChannelNativeOnMessage(IntPtr ptr, byte[] msg, int size)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCDataChannel channel)
                {
                    channel.onMessage?.Invoke(msg);
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnOpen))]
        static void DataChannelNativeOnOpen(IntPtr ptr)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCDataChannel channel)
                {
                    channel.onOpen?.Invoke();
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnClose))]
        static void DataChannelNativeOnClose(IntPtr ptr)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCDataChannel channel)
                {
                    channel.onClose?.Invoke();
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnError))]
        static void DataChannelNativeOnError(IntPtr ptr, RTCErrorType errorType, byte[] message, int size)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCDataChannel channel)
                {
                    channel.onError?.Invoke(new RTCError() { errorType = errorType, message = System.Text.Encoding.UTF8.GetString(message) });
                }
            });
        }


        internal RTCDataChannel(IntPtr ptr, RTCPeerConnection peerConnection)
            : base(ptr)
        {
            WebRTC.Table.Add(self, this);
            WebRTC.Context.DataChannelRegisterOnMessage(self, DataChannelNativeOnMessage);
            WebRTC.Context.DataChannelRegisterOnOpen(self, DataChannelNativeOnOpen);
            WebRTC.Context.DataChannelRegisterOnClose(self, DataChannelNativeOnClose);
            WebRTC.Context.DataChannelRegisterOnError(self, DataChannelNativeOnError);
        }

        /// <summary>
        ///
        /// </summary>
        ~RTCDataChannel()
        {
            this.Dispose();
        }

        /// <summary>
        /// Release all the resources RTCDataChannel instance has allocated.
        /// </summary>
        /// <remarks>
        /// The Dispose method leaves the RTCDataChannel in an unusable state.
        /// After calling Dispose, you must release all references to the RTCDataChannel
        /// so the garbage collector can reclaim the memory that the RTCDataChannel was occupying.
        ///
        /// Note: Always call Dispose before you release your last reference to the
        /// RTCDataChannel. Otherwise, the resources it is using will not be freed
        /// until the garbage collector calls the Finalize method of the object.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelDisposeExample : MonoBehaviour
        ///         {
        ///             private RTCDataChannel dataChannel;
        ///
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 // Log creation of the data channel
        ///                 Debug.Log("DataChannel created.");
        ///
        ///                 // Simulate some operations
        ///                 Invoke("CleanUp", 5.0f); // Automatically clean up after 5 seconds
        ///             }
        ///
        ///             private void CleanUp()
        ///             {
        ///                 // Clean up resources
        ///                 if (dataChannel != null)
        ///                 {
        ///                     dataChannel.Close(); // Close the channel
        ///                     dataChannel.Dispose(); // Explicitly dispose the channel
        ///                     Debug.Log("DataChannel disposed.");
        ///                 }
        ///             }
        ///
        ///             private void OnDestroy()
        ///             {
        ///                 // Ensure cleanup on destruction
        ///                 CleanUp();
        ///             }
        ///         }
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
                Close();
                WebRTC.Context.DeleteDataChannel(self);
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }

        /// <summary>
        /// Sends data across the data channel to the remote peer.
        /// </summary>
        /// <remarks>
        /// This can be done any time except during the initial process of creating the underlying transport channel.
        /// Data sent before connecting is buffered if possible (or an error occurs if it's not possible),
        /// and is also buffered if sent while the connection is closing or closed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The method throws <c>InvalidOperationException</c> when <see cref="ReadyState"/>
        ///  is not <b>Open</b>.
        /// </exception>
        /// <param name="msg"></param>
        /// <seealso cref="ReadyState"/>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelSendExample : MonoBehaviour
        ///         {
        ///             private RTCDataChannel dataChannel;
        ///
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 dataChannel.OnOpen = () =>
        ///                 {
        ///                     Debug.Log("DataChannel opened.");
        ///                     SendMessage("Hello, WebRTC!");
        ///                 };
        ///
        ///                 dataChannel.OnMessage = (e) =>
        ///                 {
        ///                     Debug.Log("Received message: " + e.data);
        ///                 };
        ///             }
        ///
        ///             private void SendMessage(string message)
        ///             {
        ///                 if (dataChannel.ReadyState == RTCDataChannelState.Open)
        ///                 {
        ///                     dataChannel.Send(message);
        ///                     Debug.Log("Sent message: " + message);
        ///                 }
        ///                 else
        ///                 {
        ///                     Debug.LogWarning("DataChannel is not open. Cannot send message.");
        ///                 }
        ///             }
        ///
        ///             private void OnDestroy()
        ///             {
        ///                 if (dataChannel != null)
        ///                 {
        ///                     dataChannel.Close();
        ///                     dataChannel.Dispose();
        ///                 }
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public void Send(string msg)
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            NativeMethods.DataChannelSend(GetSelfOrThrow(), msg);
        }

        /// <summary>
        /// Sends data across the data channel to the remote peer.
        /// </summary>
        /// <remarks>
        /// This can be done any time except during the initial process of creating the underlying transport channel.
        /// Data sent before connecting is buffered if possible (or an error occurs if it's not possible),
        /// and is also buffered if sent while the connection is closing or closed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The method throws <c>InvalidOperationException</c> when <see cref="ReadyState"/>
        ///  is not <b>Open</b>.
        /// </exception>
        /// <param name="msg"></param>
        /// <seealso cref="ReadyState"/>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelSendByteArrayExample : MonoBehaviour
        ///         {
        ///             private RTCDataChannel dataChannel;
        ///
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 dataChannel.OnOpen = () =>
        ///                 {
        ///                     Debug.Log("DataChannel opened.");
        ///                     SendMessage(new byte[] { 0x01, 0x02, 0x03 });
        ///                 };
        ///
        ///                 dataChannel.OnMessage = (e) =>
        ///                 {
        ///                     if (e.binary)
        ///                     {
        ///                         Debug.Log("Received binary message of length: " + e.data.Length);
        ///                     }
        ///                     else
        ///                     {
        ///                         Debug.Log("Received message: " + e.data);
        ///                     }
        ///                 };
        ///             }
        ///
        ///             public void SendMessage(byte[] message)
        ///             {
        ///                 if (dataChannel.ReadyState == RTCDataChannelState.Open)
        ///                 {
        ///                     dataChannel.Send(message);
        ///                     Debug.Log("Sent binary message of length: " + message.Length);
        ///                 }
        ///                 else
        ///                 {
        ///                     Debug.LogWarning("DataChannel is not open. Cannot send message.");
        ///                 }
        ///             }
        ///
        ///             private void OnDestroy()
        ///             {
        ///                 if (dataChannel != null)
        ///                 {
        ///                     dataChannel.Close();
        ///                     dataChannel.Dispose();
        ///                 }
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public void Send(byte[] msg)
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            NativeMethods.DataChannelSendBinary(GetSelfOrThrow(), msg, msg.Length);
        }

        /// <summary>
        /// Sends data across the data channel to the remote peer.
        /// </summary>
        /// <remarks>
        /// This can be done any time except during the initial process of creating the underlying transport channel.
        /// Data sent before connecting is buffered if possible (or an error occurs if it's not possible),
        /// and is also buffered if sent while the connection is closing or closed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The method throws <c>InvalidOperationException</c> when <see cref="ReadyState"/>
        ///  is not <b>Open</b>.
        /// </exception>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using System;
        ///         using Unity.Collections;
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelSendNativeArrayExample : MonoBehaviour
        ///         {
        ///             private RTCDataChannel dataChannel;
        ///
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 dataChannel.OnOpen = () =>
        ///                 {
        ///                     Debug.Log("DataChannel opened.");
        ///                     var nativeArray = new NativeArray<byte>(new byte[] { 0x01, 0x02, 0x03 }, Allocator.Temp);
        ///                     Send(nativeArray);
        ///                     nativeArray.Dispose();
        ///                 };
        ///
        ///                 dataChannel.OnMessage = (e) =>
        ///                 {
        ///                     if (e.binary)
        ///                     {
        ///                         Debug.Log("Received binary message of length: " + e.data.Length);
        ///                     }
        ///                 };
        ///             }
        ///
        ///             public unsafe void Send<T>(NativeArray<T> msg) where T : struct
        ///             {
        ///                 if (dataChannel.ReadyState == RTCDataChannelState.Open)
        ///                 {
        ///                     void* ptr = msg.GetUnsafePtr();
        ///                     byte[] bytes = new byte[msg.Length * UnsafeUtility.SizeOf<T>()];
        ///                     System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), bytes, 0, bytes.Length);
        ///                     dataChannel.Send(bytes);
        ///                     Debug.Log("Sent binary message of length: " + bytes.Length);
        ///                 }
        ///                 else
        ///                 {
        ///                     Debug.LogWarning("DataChannel is not open. Cannot send message.");
        ///                 }
        ///             }
        ///
        ///             private void OnDestroy()
        ///             {
        ///                 if (dataChannel != null)
        ///                 {
        ///                     dataChannel.Close();
        ///                     dataChannel.Dispose();
        ///                 }
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public unsafe void Send<T>(NativeArray<T> msg)
            where T : struct
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            if (!msg.IsCreated)
            {
                throw new ArgumentException("Message array has not been created.", nameof(msg));
            }
            NativeMethods.DataChannelSendPtr(GetSelfOrThrow(), new IntPtr(msg.GetUnsafeReadOnlyPtr()), msg.Length * UnsafeUtility.SizeOf<T>());
        }

        /// <summary>
        /// Sends data across the data channel to the remote peer.
        /// </summary>
        /// <remarks>
        /// This can be done any time except during the initial process of creating the underlying transport channel.
        /// Data sent before connecting is buffered if possible (or an error occurs if it's not possible),
        /// and is also buffered if sent while the connection is closing or closed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The method throws <c>InvalidOperationException</c> when <see cref="ReadyState"/>
        ///  is not <b>Open</b>.
        /// </exception>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using System;
        ///         using Unity.Collections;
        ///         using Unity.Collections.LowLevel.Unsafe;
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelSendNativeSliceExample : MonoBehaviour
        ///         {
        ///             private RTCDataChannel dataChannel;
        ///
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 dataChannel.OnOpen = () =>
        ///                 {
        ///                     Debug.Log("DataChannel opened.");
        ///                     var nativeArray = new NativeArray<byte>(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }, Allocator.Temp);
        ///                     var nativeSlice = new NativeSlice<byte>(nativeArray, 1, 3); // Slice from index 1 to 3
        ///                     Send(nativeSlice);
        ///                     nativeArray.Dispose();
        ///                 };
        ///
        ///                 dataChannel.OnMessage = (e) =>
        ///                 {
        ///                     if (e.binary)
        ///                     {
        ///                         Debug.Log("Received binary message of length: " + e.data.Length);
        ///                     }
        ///                 };
        ///             }
        ///
        ///             public unsafe void Send<T>(NativeSlice<T> msg) where T : struct
        ///             {
        ///                 if (dataChannel.ReadyState == RTCDataChannelState.Open)
        ///                 {
        ///                     void* ptr = msg.GetUnsafeReadOnlyPtr();
        ///                     byte[] bytes = new byte[msg.Length * UnsafeUtility.SizeOf<T>()];
        ///                     System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), bytes, 0, bytes.Length);
        ///                     dataChannel.Send(bytes);
        ///                     Debug.Log("Sent binary message of length: " + bytes.Length);
        ///                 }
        ///                 else
        ///                 {
        ///                     Debug.LogWarning("DataChannel is not open. Cannot send message.");
        ///                 }
        ///             }
        ///
        ///             private void OnDestroy()
        ///             {
        ///                 if (dataChannel != null)
        ///                 {
        ///                     dataChannel.Close();
        ///                     dataChannel.Dispose();
        ///                 }
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>

        public unsafe void Send<T>(NativeSlice<T> msg)
            where T : struct
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            NativeMethods.DataChannelSendPtr(GetSelfOrThrow(), new IntPtr(msg.GetUnsafeReadOnlyPtr()), msg.Length * UnsafeUtility.SizeOf<T>());
        }

#if UNITY_2020_1_OR_NEWER // ReadOnly support was introduced in 2020.1

        /// <summary>
        /// Sends data across the data channel to the remote peer.
        /// </summary>
        /// <remarks>
        /// This can be done any time except during the initial process of creating the underlying transport channel.
        /// Data sent before connecting is buffered if possible (or an error occurs if it's not possible),
        /// and is also buffered if sent while the connection is closing or closed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The method throws <c>InvalidOperationException</c> when <see cref="ReadyState"/>
        ///  is not <b>Open</b>.
        /// </exception>        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using System;
        ///         using Unity.Collections;
        ///         using Unity.Collections.LowLevel.Unsafe;
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelSendNativeArrayReadOnlyExample : MonoBehaviour
        ///         {
        ///             private RTCDataChannel dataChannel;
        ///
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 dataChannel.OnOpen = () =>
        ///                 {
        ///                     Debug.Log("DataChannel opened.");
        ///                     var nativeArray = new NativeArray<byte>(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }, Allocator.Temp);
        ///                     var readOnlyArray = nativeArray.AsReadOnly();
        ///                     Send(readOnlyArray);
        ///                     nativeArray.Dispose();
        ///                 };
        ///
        ///                 dataChannel.OnMessage = (e) =>
        ///                 {
        ///                     if (e.binary)
        ///                     {
        ///                         Debug.Log("Received binary message of length: " + e.data.Length);
        ///                     }
        ///                 };
        ///             }
        ///
        ///             public unsafe void Send<T>(NativeArray<T>.ReadOnly msg) where T : struct
        ///             {
        ///                 if (dataChannel.ReadyState == RTCDataChannelState.Open)
        ///                 {
        ///                     void* ptr = msg.GetUnsafeReadOnlyPtr();
        ///                     byte[] bytes = new byte[msg.Length * UnsafeUtility.SizeOf<T>()];
        ///                     System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), bytes, 0, bytes.Length);
        ///                     dataChannel.Send(bytes);
        ///                     Debug.Log("Sent binary message of length: " + bytes.Length);
        ///                 }
        ///                 else
        ///                 {
        ///                     Debug.LogWarning("DataChannel is not open. Cannot send message.");
        ///                 }
        ///             }
        ///
        ///             private void OnDestroy()
        ///             {
        ///                 if (dataChannel != null)
        ///                 {
        ///                     dataChannel.Close();
        ///                     dataChannel.Dispose();
        ///                 }
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public unsafe void Send<T>(NativeArray<T>.ReadOnly msg)
            where T : struct
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            NativeMethods.DataChannelSendPtr(GetSelfOrThrow(), new IntPtr(msg.GetUnsafeReadOnlyPtr()), msg.Length * UnsafeUtility.SizeOf<T>());
        }
#endif

        /// <summary>
        /// Sends data across the data channel to the remote peer.
        /// </summary>
        /// <remarks>
        /// This can be done any time except during the initial process of creating the underlying transport channel.
        /// Data sent before connecting is buffered if possible (or an error occurs if it's not possible),
        /// and is also buffered if sent while the connection is closing or closed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The method throws <c>InvalidOperationException</c> when <see cref="ReadyState"/>
        ///  is not <b>Open</b>.
        /// </exception>
        /// <param name="msgPtr"></param>
        /// <param name="length"></param>
        public unsafe void Send(void* msgPtr, int length)
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            NativeMethods.DataChannelSendPtr(GetSelfOrThrow(), new IntPtr(msgPtr), length);
        }

        /// <summary>
        /// Sends data across the data channel to the remote peer.
        /// </summary>
        /// <remarks>
        /// This can be done any time except during the initial process of creating the underlying transport channel.
        /// Data sent before connecting is buffered if possible (or an error occurs if it's not possible),
        /// and is also buffered if sent while the connection is closing or closed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The method throws <c>InvalidOperationException</c> when <see cref="ReadyState"/>
        ///  is not <b>Open</b>.
        /// </exception>        /// <param name="msgPtr"></param>
        /// <param name="length"></param>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using System;
        ///         using Unity.Collections;
        ///         using Unity.Collections.LowLevel.Unsafe;
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelSendVoidPointerExample : MonoBehaviour
        ///         {
        ///             private RTCDataChannel dataChannel;
        ///
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 dataChannel.OnOpen = () =>
        ///                 {
        ///                     Debug.Log("DataChannel opened.");
        ///
        ///                     byte[] data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        ///                     unsafe
        ///                     {
        ///                         fixed (byte* dataPtr = data)
        ///                         {
        ///                             Send(dataPtr, data.Length);
        ///                         }
        ///                     }
        ///                 };
        ///
        ///                 dataChannel.OnMessage = (e) =>
        ///                 {
        ///                     if (e.binary)
        ///                     {
        ///                         Debug.Log("Received binary message of length: " + e.data.Length);
        ///                     }
        ///                 };
        ///             }
        ///
        ///
        ///             private void OnDestroy()
        ///             {
        ///                 if (dataChannel != null)
        ///                 {
        ///                     dataChannel.Close();
        ///                     dataChannel.Dispose();
        ///                 }
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public void Send(IntPtr msgPtr, int length)
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            if (msgPtr != IntPtr.Zero && length > 0)
            {
                NativeMethods.DataChannelSendPtr(GetSelfOrThrow(), msgPtr, length);
            }
        }

        /// <summary>
        /// Closes the RTCDataChannel. Either peer is permitted to call this method to initiate closure of the channel.
        /// </summary>
        /// <remarks>
        /// Closure of the data channel is not instantaneous. Most of the process of closing the connection is handled asynchronously;
        /// you can detect when the channel has finished closing by watching for a close event on the data channel.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         using UnityEngine;
        ///         using Unity.WebRTC;
        ///
        ///         public class DataChannelCloseExample : MonoBehaviour
        ///         {
        ///             private RTCDataChannel dataChannel;
        ///
        ///             private void Start()
        ///             {
        ///                 var initOption = new RTCDataChannelInit();
        ///                 var peerConnection = new RTCPeerConnection();
        ///                 dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
        ///
        ///                 dataChannel.OnOpen = () =>
        ///                 {
        ///                     Debug.Log("DataChannel opened.");
        ///                 };
        ///
        ///                 dataChannel.OnClose = () =>
        ///                 {
        ///                     Debug.Log("DataChannel closed.");
        ///                 };
        ///
        ///                 // Assume some operation has been completed and we need to close the data channel
        ///                 Invoke("CloseDataChannel", 5.0f); // Close the channel after 5 seconds
        ///             }
        ///
        ///             private void CloseDataChannel()
        ///             {
        ///                 if (dataChannel != null)
        ///                 {
        ///                     dataChannel.Close();
        ///                     Debug.Log("DataChannel has been closed manually.");
        ///                 }
        ///             }
        ///
        ///             private void OnDestroy()
        ///             {
        ///                 // Clean up the data channel when the GameObject is destroyed
        ///                 if (dataChannel != null)
        ///                 {
        ///                     dataChannel.Close();
        ///                 }
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public void Close()
        {
            NativeMethods.DataChannelClose(GetSelfOrThrow());
        }
    }
}
