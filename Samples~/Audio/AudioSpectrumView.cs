using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Unity.WebRTC.Samples
{
    class AudioSpectrumView : MonoBehaviour
    {
        [SerializeField] AudioSource target;
        [SerializeField] LineRenderer line;
        [SerializeField] RectTransform rectTransform;
        [SerializeField] float xRatio = 1f;
        [SerializeField] float yRatio = 10000f;
        [SerializeField] private float yBase = 2.71828f;

        const int positionCount = 256;
        float[] spectrum = new float[positionCount];
        NativeArray<Vector3> array;

        void Start()
        {
            array = new NativeArray<Vector3>(positionCount, Allocator.Persistent);
            line.positionCount = positionCount;
        }

        void OnDestroy()
        {
            array.Dispose();
        }

        void Update()
        {
            target.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
            for(int i = 1; i < array.Length; i++)
            {
                float x = rectTransform.rect.width * i / array.Length * xRatio;
                float y = rectTransform.rect.height * Mathf.Log(spectrum[i] + 1, yBase) * yRatio;
                array[i] = new Vector3(x, y, 0);
            }
            line.SetPositions(array);
        }
    }
}
