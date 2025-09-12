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
        /// Determines whether the data channel ensures the delivery of messages in the order they were sent.
        /// </summary>
        public bool? ordered;

        /// <summary>
        /// Specifies the maximum number of transmission retries for a message when operating under non-guaranteed delivery conditions.
        /// </summary>
        /// <remarks>
        /// Cannot be set along with <see cref="RTCDataChannelInit.maxRetransmits"/>.
        /// </remarks>
        /// <seealso cref="RTCDataChannelInit.maxRetransmits"/>
        public int? maxPacketLifeTime;

        /// <summary>
        /// Specifies the maximum number of times the data channel will attempt to resend a message if initial transmission fails under unreliable conditions.
        /// </summary>
        /// <remarks>
        /// Cannot be set along with <see cref="RTCDataChannelInit.maxPacketLifeTime"/>.
        /// </remarks>
        /// <seealso cref="RTCDataChannelInit.maxPacketLifeTime"/>
        public int? maxRetransmits;

        /// <summary>
        /// Specifies the sub protocol being used by the data channel to transmit and process messages.
        /// </summary>
        public string protocol;

        /// <summary>
        /// Specifies whether the data channel's connection is manually negotiated by the application or automatically handled by WebRTC.
        /// </summary>
        public bool? negotiated;

        /// <summary>
        /// Specifies a unique 16-bit identifier for the data channel, allowing explicit channel setup during manual negotiation.
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
    /// Represents the method that will be invoked when the data channel is successfully opened and ready for communication.
    /// </summary>
    /// <remarks>
    /// The `DelegateOnOpen` is triggered when the data channel's transport layer becomes successfully established and ready for data transfer.
    /// This indicates that messages can now be sent and received over the channel, marking the transition from connecting to an operational state.
    /// Useful for initializing or signaling to the application that the channel setup is complete and ready for communication.
    /// This delegate is typically assigned to the <see cref="RTCDataChannel.OnOpen"/> property.
    /// </remarks>
    /// <seealso cref="RTCDataChannel.OnOpen"/>
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
    ///                 // Assign the delegate to handle the OnOpen event
    ///                 dataChannel.OnOpen = () =>
    ///                 {
    ///                     Debug.Log("DataChannel is now open and ready for communication.");
    ///                 };
    ///             }
    ///         }
    ///     ]]></code>
    /// </example>
    public delegate void DelegateOnOpen();

    /// <summary>
    /// Represents the method that will be invoked when the data channel has been closed and is no longer available for communication.
    /// </summary>
    /// <remarks>
    /// The `DelegateOnClose` is triggered when the data channel's underlying transport is terminated, signaling that no further messages can be sent or received.
    /// Useful for cleaning up resources or notifying the application that the data channel is no longer in use.
    /// This marks a transition to a non-operational state, and to resume communication, a new data channel must be established.
    /// This delegate is typically assigned to the <see cref="RTCDataChannel.OnClose"/> property.
    /// </remarks>
    /// <seealso cref="RTCDataChannel.OnClose"/>
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
    ///                 // Assign the delegate to handle the OnClose event
    ///                 dataChannel.OnClose = () =>
    ///                 {
    ///                     Debug.Log("DataChannel has been closed.");
    ///                 };
    ///             }
    ///         }
    ///     ]]></code>
    /// </example>
    public delegate void DelegateOnClose();

    /// <summary>
    /// Represents the method that will be invoked when a message is received from the remote peer over the data channel.
    /// </summary>
    /// <remarks>
    /// The `DelegateOnMessage` is executed when the data channel successfully receives a message from the remote peer, providing the message content as a parameter.
    /// This allows the application to process the incoming data, whether it's for updating the UI, triggering gameplay logic, or handling any response actions.
    /// The method receives the message as a byte array, making it flexible for both textual and binary data.
    /// This delegate is typically assigned to the <see cref="RTCDataChannel.OnMessage"/> property.
    /// </remarks>
    /// <param name="bytes">The message received as a byte array.</param>
    /// <seealso cref="RTCDataChannel.OnMessage"/>
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
    ///                 // Assign the delegate to handle the OnMessage event
    ///                 dataChannel.OnMessage = (bytes) =>
    ///                 {
    ///                     string message = System.Text.Encoding.UTF8.GetString(bytes);
    ///                     Debug.Log("Received message: " + message);
    ///                 };
    ///             }
    ///         }
    ///     ]]></code>
    /// </example>
    public delegate void DelegateOnMessage(byte[] bytes);

    /// <summary>
    /// Represents the method that will be invoked when a new data channel is added to the RTCPeerConnection.
    /// </summary>
    /// <remarks>
    /// The `DelegateOnDataChannel` is triggered when a new data channel is established, typically as a result of the remote peer creating a channel.
    /// This provides an opportunity to configure the new channel, such as setting message handlers or adjusting properties.
    /// Ensuring the application is prepared to handle the new data channel is crucial for seamless peer-to-peer communication.
    /// This delegate is typically assigned to the <see cref="RTCPeerConnection.OnDataChannel"/> property.
    /// </remarks>
    /// <param name="channel">The RTCDataChannel that has been added to the connection.</param>
    /// <seealso cref="RTCPeerConnection.OnDataChannel"/>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         using UnityEngine;
    ///         using Unity.WebRTC;
    ///
    ///         public class DataChannelHandlerExample : MonoBehaviour
    ///         {
    ///             private void Start()
    ///             {
    ///                 var peerConnection = new RTCPeerConnection();
    ///
    ///                 // Assign the delegate to handle the OnDataChannel event
    ///                 peerConnection.OnDataChannel = (channel) =>
    ///                 {
    ///                     Debug.Log("A new data channel has been added: " + channel.Label);
    ///                     channel.OnMessage = (bytes) =>
    ///                     {
    ///                         string message = System.Text.Encoding.UTF8.GetString(bytes);
    ///                         Debug.Log("Received message on new channel: " + message);
    ///                     };
    ///                 };
    ///             }
    ///         }
    ///     ]]></code>
    /// </example>
    public delegate void DelegateOnDataChannel(RTCDataChannel channel);

    /// <summary>
    /// Represents the method that will be invoked when an error occurs on the data channel.
    /// </summary>
    /// <remarks>
    /// The `DelegateOnError` is executed whenever an error arises within the data channel, allowing applications to handle various error scenarios gracefully.
    /// It provides detailed information about the error, enabling developers to implement corrective measures or issue notifications to users.
    /// Handling such errors is crucial for maintaining robust and reliable peer-to-peer communication.
    /// This delegate is typically assigned to the <see cref="RTCDataChannel.OnError"/> property.
    /// </remarks>
    /// <param name="error">The RTCError object that contains details about the error.</param>
    /// <seealso cref="RTCDataChannel.OnError"/>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         using UnityEngine;
    ///         using Unity.WebRTC;
    ///
    ///         public class DataChannelErrorHandlingExample : MonoBehaviour
    ///         {
    ///             private void Start()
    ///             {
    ///                 var initOption = new RTCDataChannelInit();
    ///                 var peerConnection = new RTCPeerConnection();
    ///                 var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
    ///
    ///                 // Assign the delegate to handle the OnError event
    ///                 dataChannel.OnError = (error) =>
    ///                 {
    ///                     Debug.LogError("DataChannel error occurred: " + error.message);
    ///                     // Additional error handling logic can be implemented here
    ///                 };
    ///             }
    ///         }
    ///     ]]></code>
    /// </example>

    public delegate void DelegateOnError(RTCError error);

    /// <summary>
    /// Creates a new RTCDataChannel for peer-to-peer data exchange, using the specified label and options.
    /// </summary>
    /// <remarks>
    /// The `CreateDataChannel` method establishes a bidirectional communication channel between peers, identified by a unique label.
    /// This channel allows for the transmission of arbitrary data, such as text or binary, directly between connected peers without the need for a traditional server.
    /// The optional parameters provide flexibility in controlling the behavior of the data channel, including options for reliability and ordering of messages.
    /// It's essential for applications to configure these channels according to their specific communication needs.
    /// </remarks>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         var initOption = new RTCDataChannelInit();
    ///         var peerConnection = new RTCPeerConnection();
    ///         var dataChannel = peerConnection.CreateDataChannel("test channel", initOption);
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
        /// The `OnMessage` delegate is invoked whenever a message is received over the data channel from the remote peer.
        /// This provides the application an opportunity to process the received data, which could include tasks such as updating the user interface, storing information, or triggering specific logic.
        /// The message is delivered as a byte array, offering flexibility to handle both text and binary data formats.
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
        /// Delegate to be called when the data channel's message transport mechanism is opened or reopened.
        /// </summary>
        /// <remarks>
        /// The `OnOpen` delegate is triggered when the data channel successfully establishes its underlying transport mechanism.
        /// This state transition indicates that the channel is ready for data transmission, providing an opportunity for the application to initialize any required states or notify the user that the channel is ready to use.
        /// It is a critical event for setting up initial data exchanges between peers.
        /// </remarks>
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
        /// Delegate to be called when the data channel's message transport mechanism is closed.
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
        /// Delegate to be called when errors occur.
        /// </summary>
        /// <remarks>
        /// The `OnClose` delegate is triggered when the data channel's transport layer is terminated, signifying the channel's transition to a closed state.
        /// This event serves as a cue for the application to release resources, update the user interface, or handle any clean-up operations necessary to gracefully end the communication session.
        /// Understanding this transition is vital for managing the lifecycle of data exchanges between peers.
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
        /// The `Id` property provides a unique identifier for the data channel, typically assigned during the channel's creation.
        /// This identifier is used internally to differentiate between multiple data channels associated with a single RTCPeerConnection.
        /// Understanding and referencing these IDs can be crucial when managing complex peer-to-peer communication setups where multiple channels are active.
        /// The ID is automatically generated unless explicitly set during manual channel negotiation.
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
        /// Returns a string description of the data channel, which is not required to be unique.
        /// </summary>
        /// <remarks>
        /// The `Label` property specifies a name for the data channel, which is set when the channel is created.
        /// This label is useful for identifying the purpose of the data channel, such as distinguishing between channels dedicated to different types of data or tasks.
        /// While labels are not required to be unique, they provide meaningful context within an application, aiding in organization and management of multiple channels.
        /// Developers can utilize labels to group channels by function or to describe their role in the communication process.
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
        /// Returns the subprotocol being used by the data channel to transmit and process messages.
        /// </summary>
        /// <remarks>
        /// The `Protocol` property retrieves the subprotocol negotiated for this data channel, which governs the rules for message format and communication behavior between peers.
        /// This property is critical for ensuring compatibility and understanding between different systems or applications using the channel, especially when custom protocols are used.
        /// If no protocol was specified during the data channel's creation, this property returns an empty string, indicating that no particular subprotocol is in effect.
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
        /// The `MaxRetransmits` property defines the upper limit on the number of times a message will be retransmitted if initial delivery fails.
        /// This setting is particularly valuable in conditions where reliable delivery is necessary, but the application is sensitive to potential delays caused by continuous retransmission attempts.
        /// By specifying a limit, developers can balance the need for message reliability with the potential impact on performance and latency.
        /// If no retransmit limit is set, the data channel may continue to attempt message delivery until it succeeds, which might not be suitable for all applications.        /// </remarks>
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
        /// The `MaxRetransmitTime` property sets the maximum duration, in milliseconds, that the data channel will attempt to retransmit a message in unreliable mode.
        /// This constraint ensures that if a message cannot be delivered within the specified time frame, the channel will cease retransmission attempts.
        /// It is particularly useful for applications where timing is critical, allowing developers to limit delays potentially caused by prolonged retransmission efforts.
        /// By defining this timeout, applications can maintain performance efficiency while handling network fluctuations.
        /// If not set, the retransmission will continue based on other reliability settings, possibly yielding variable delays.        /// </remarks>
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
        /// Determines whether the data channel ensures the delivery of messages in the order they were sent.
        /// </summary>
        /// <remarks>
        /// The `Ordered` property controls whether the data channel delivers messages in the sequence they were dispatched.
        /// If set to true, messages will arrive in the exact order sent, ensuring consistent data flow, which can be critical for applications where order is important.
        /// If false, the data channel allows out-of-order delivery to potentially enhance transmission speed but is best suited for applications where strict order isn't a concern.        /// </remarks>
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
        /// The `BufferedAmount` property indicates the number of bytes of data currently queued to be sent over the data channel.
        /// This value represents the amount of data buffered on the sender side that has not yet been transmitted to the network.
        /// Monitoring this property helps developers understand and manage flow control, allowing for adjustments to data transmission rates to avoid congestion.
        /// In scenarios where this value grows unexpectedly, it could indicate network congestion or slow peer processing, prompting the need to throttle data sending.
        /// Proper use of this property ensures that applications can maintain efficient data flow while mitigating potential bottlenecks.
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
        /// The `Negotiated` property indicates whether the data channel's connection parameters were explicitly negotiated by the application or automatically handled by the WebRTC implementation.
        /// When set to `true`, it allows developers to manually manage the channel setup including selecting the channel ID, offering greater control over communication specifics.
        /// This is especially useful in advanced scenarios where integration with complex signaling servers or custom negotiation processes are needed.
        /// If `false`, the WebRTC stack automatically negotiates the channel's configuration, simplifying the setup but providing less granular control.
        /// Proper switching between these modes ensures the application meets its communication requirements effectively.
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
        /// Finalizer for <see cref="RTCDataChannel"/>.
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
        /// <param name="msg">The string message to be sent to the remote peer.</param>
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
        /// <param name="msg">The byte array to be sent to the remote peer.</param>
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
        /// <typeparam name="T">The type of elements stored in the NativeArray, which must be a value type.</typeparam>
        /// <param name="msg">The NativeArray containing the data to be sent to the remote peer.
        /// </param>
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
        /// Thrown when the <see cref="ReadyState"/> is not <b>Open</b>.
        /// </exception>
        /// <typeparam name="T">The type of elements stored in the NativeSlice, which must be a value type.</typeparam>
        /// <param name="msg">The NativeSlice containing the data to be sent to the remote peer.</param>
        /// <seealso cref="RTCDataChannel.ReadyState"/>
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
        /// Thrown when the <see cref="ReadyState"/> is not <b>Open</b>.
        /// </exception>
        /// <typeparam name="T">The type of elements stored in the read-only NativeArray, which must be a value type.</typeparam>
        /// <param name="msg">The read-only NativeArray containing the data to be sent to the remote peer.</param>
        /// <seealso cref="RTCDataChannel.ReadyState"/>
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
        /// Thrown when the <see cref="ReadyState"/> is not <b>Open</b>.
        /// </exception>
        /// <param name="msgPtr">A pointer to the memory location containing the data to be sent.</param>
        /// <param name="length">The length of the data, in bytes, to be sent from the specified memory location.</param>
        /// <seealso cref="RTCDataChannel.ReadyState"/>
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
        /// Thrown when the <see cref="ReadyState"/> is not <b>Open</b>.
        /// </exception>
        /// <param name="msgPtr">A pointer to the memory location containing the data to be sent.</param>
        /// <param name="length">The length of the data, in bytes, to be sent from the specified memory location.</param>
        /// <seealso cref="RTCDataChannel.ReadyState"/>
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
