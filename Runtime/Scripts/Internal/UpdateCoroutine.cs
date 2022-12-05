using System;
using System.Collections;
using UnityEngine;

namespace Unity.WebRTC
{
    public class UpdateCoroutine : MonoBehaviour
    {
        Coroutine _coroutine;

        public Func<IEnumerator> routine
        {
            set
            {
                if(_coroutine != null)
                    StopCoroutine(_coroutine);
                _coroutine = StartCoroutine(value());
            }
        }


        void OnDestroy()
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);
        }
    }
}
