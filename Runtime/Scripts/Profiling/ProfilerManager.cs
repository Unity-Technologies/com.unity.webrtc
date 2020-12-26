#if UNITY_WEBRTC_ENABLE_PROFILING_CORE

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Unity.WebRTC.Profiling
{
    public class ProfilerManager
    {
        public static readonly ProfilerCategory ProfilerCategory = ProfilerCategory.Scripts;

        //private readonly Func<IEnumerator, Coroutine> _startCoroutine;
        private static readonly List<RTCStatsReportAsyncOperation> _list =
            new List<RTCStatsReportAsyncOperation>();

        private static readonly Hashtable _counters = new Hashtable();

        //public ProfilerManager(Func<IEnumerator, Coroutine> coroutine)
        //{
        //    _startCoroutine = coroutine;
        //}

        // Update is called once per frame
        public static void Update()
        {
            var peerList = WebRTC.PeerList;

            if (peerList == null)
            {
                return;
            }

            foreach (var weakReference in peerList)
            {
                if (!weakReference.TryGetTarget(out var peer))
                {
                    continue;
                }
                var op = peer.GetStats();
                if (_list.Contains(op))
                    continue;
                _list.Add(op);

                for (int i = _list.Count - 1; i >= 0; i--)
                {
                    var _op = _list[i];
                    if (!_op.IsDone)
                        continue;
                    _list.RemoveAt(i);
                    OnStats(_op.Value);
                }
            }
        }

        //IEnumerator WaitStats(RTCStatsReportAsyncOperation op)
        //{
        //    yield return op;
        //    if (!op.IsError)
        //    {
        //        OnStats(op.Value);

        //        //var peerId = peer.GetHashCode();
        //        //if (!m_peerConnenctionDataStore.ContainsKey(peerId))
        //        //{
        //        //    m_peerConnenctionDataStore[peerId] = new PeerConnectionRecord(peer.GetConfiguration());
        //        //}

        //        //m_peerConnenctionDataStore[peerId].Update(op.Value);
        //    }
        //}

        static void OnStats(RTCStatsReport report)
        {
            if (report == null)
                return;
            foreach (var _pair in report.Stats)
            {
                foreach (var stat in _pair.Value.Dict)
                {
                    if (_counters.Contains(stat.Key))
                    {
                        switch (stat.Value)
                        {
                            case ulong value:
                            {

                                var counter = _counters[stat.Key] as ProfilerCounter<ulong>?;
                                counter?.Sample(value);
                                break;
                            }
                            case long value:
                            {
                                var counter = _counters[stat.Key] as ProfilerCounter<long>?;
                                counter?.Sample(value);
                                break;
                            }
                            case uint value:
                            {
                                //var counter = _counters[stat.Key] as ProfilerCounter<uint>?;
                                //counter?.Sample(value);
                                if (stat.Key == "packetsReceived")
                                {
                                    ProfilerCounterCollection.packetsReceived.Sample(value);
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        switch (stat.Value)
                        {
                            case ulong value:
                            {
                                Debug.Log(stat.Key);
                                var counter = new ProfilerCounter<ulong>(ProfilerCategory,
                                    stat.Key, ProfilerMarkerDataUnit.Undefined);
                                _counters.Add(stat.Key, counter);
                                break;
                            }
                            case long value:
                            {
                                Debug.Log(stat.Key);
                                var counter = new ProfilerCounter<long>(ProfilerCategory,
                                    stat.Key, ProfilerMarkerDataUnit.Undefined);
                                _counters.Add(stat.Key, counter);
                                break;
                            }
                            case uint value:
                            {
                                Debug.Log(stat.Key);
                                var counter = new ProfilerCounter<uint>(ProfilerCategory,
                                    stat.Key, ProfilerMarkerDataUnit.Undefined);
                                _counters.Add(stat.Key, counter);
                                break;
                            }
                        }
                    }
                    //new ProfilerCounter<float>(ProfilerCategory,
                    //    stat.Key, ProfilerMarkerDataUnit.Undefined);
                }
                //RTCInboundRTPStreamStats.packetsReceived.Sample(Mathf.Sin(Time.time) * 10);

                //switch (pair.Value.Type)
                //{
                //    RTCInboundRTPStreamStats.packetsReceived.Sample(Mathf.Sin(Time.time) * 10);
                //}
            }
        }
    }
}
#endif
