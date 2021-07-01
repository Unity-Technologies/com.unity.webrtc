using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Unity.WebRTC.Samples
{
    class AudioSpectrumView : MonoBehaviour
    {
        [SerializeField] AudioSource target;
        [SerializeField] LineRenderer line;
        [SerializeField] Color[] lineColors;
        [SerializeField] RectTransform rectTransform;
        [SerializeField] float xRatio = 1f;
        [SerializeField] float yRatio = 1f;

        const int positionCount = 256;
        float[] spectrum = new float[2048];

        NativeArray<Vector3> array;
        List<LineRenderer> lines = new List<LineRenderer>();

        void Start()
        {
            array = new NativeArray<Vector3>(positionCount, Allocator.Persistent);

            // This line object is used as a template.
            if(line.gameObject.activeInHierarchy)
                line.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            array.Dispose();
        }

        void ResetLines(int channelCount)
        {
            foreach (var line in lines)
            {
                Object.Destroy(line.gameObject);
            }
            lines.Clear();
            for (int i = 0; i < channelCount; i++)
            {
                var line_ = GameObject.Instantiate(line, line.transform.parent);
                line_.gameObject.SetActive(true);
                line_.positionCount = positionCount;
                line_.startColor = lineColors[i];
                line_.endColor = lineColors[i];
                lines.Add(line_);
            }
        }

        void Update()
        {
            if (target.clip == null)
            {
                if(lines.Count > 0)
                    ResetLines(0);
                return;
            }
            int channelCount = target.clip.channels;
            if (channelCount != lines.Count)
            {
                ResetLines(channelCount);
            }
            for (int channelIndex = 0; channelIndex < channelCount; channelIndex++)
            {
                target.GetSpectrumData(spectrum, channelIndex, FFTWindow.Rectangular);
                for (int i = 1; i < array.Length; i++)
                {
                    float x = rectTransform.rect.width * i / array.Length * xRatio;
                    float y = rectTransform.rect.height * Mathf.Log(spectrum[i] + 1) * yRatio;
                    array[i] = new Vector3(x, y, 0);
                }
                lines[channelIndex].SetPositions(array);
            }
        }
    }
}
