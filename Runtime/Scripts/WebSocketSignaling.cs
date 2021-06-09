#if UNITY_WEBGL
using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.WebRTC.Signaling
{
    public class WebSocketSignaling
    {
        public delegate void dlgSignalingOpen();
        public static event dlgSignalingOpen OnOpen;

        public delegate void dlgSignalingMessage(string msg);
        public static event dlgSignalingMessage OnMessage;

        public delegate void dlgSignalingClose(string reason, int code);
        public static event dlgSignalingClose OnClose;

        public delegate void dlgSignalingWSError();
        public static event dlgSignalingWSError OnError;

        private const string Lib = "__Internal";

        [DllImport(Lib)]
        private static extern void WebSocketSignalingInit(
            Action onOpen,
            Action<string> onMessage,
            Action<string, int> onClose,
            Action onError);

        [DllImport(Lib)]
        private static extern void WebSocketSignalingConnect(string url);

        [DllImport(Lib)]
        private static extern void WebSocketSignalingSend(string data);

        [DllImport(Lib)]
        private static extern void WebSocketSignalingClose();

        public static void Init(string url, string myId = null)
        {
            WebSocketSignalingInit(
                OnWSOpen,
                OnWSMessage,
                OnWSClose,
                OnWSError);
        }

        public static void Connect(string url)
        {
            WebSocketSignalingConnect(url);
        }

        [MonoPInvokeCallback(typeof(Action))]
        private static void OnWSOpen()
        {
            OnOpen.Invoke();
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnWSMessage(string msg)
        {
            OnMessage.Invoke(msg);
        }

        [MonoPInvokeCallback(typeof(Action<string, int>))]
        private static void OnWSClose(string reason, int code)
        {
            OnClose.Invoke(reason, code);
        }

        [MonoPInvokeCallback(typeof(Action))]
        private static void OnWSError()
        {
            OnError.Invoke();
        }

        public static void Send(string msg)
        {
            WebSocketSignalingSend(msg);
        }
    }
}
#endif
