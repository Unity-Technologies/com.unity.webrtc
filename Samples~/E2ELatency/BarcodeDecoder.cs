using UnityEngine;

class BarcodeDecoder : MonoBehaviour
{
    [SerializeField] int Row;
    [SerializeField] int Column;
    [SerializeField] ComputeShader Shader;

#if UNITY_2020_1_OR_NEWER
    GraphicsBuffer readbackBuffer_;
#else
    ComputeBuffer readbackBuffer_;
#endif
    int kernelIndex_;
    Color[] data_;

    private void Awake()
    {
        int count = Row * Column;
        int stride = sizeof(float) * 4;
#if UNITY_2020_1_OR_NEWER
        readbackBuffer_ =
            new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, stride);
#else
        readbackBuffer_ =
            new ComputeBuffer(count, stride, ComputeBufferType.Structured);
#endif
        data_ = new Color[count];
        kernelIndex_ = Shader.FindKernel("Read");
    }

    private void OnDestroy()
    {
        readbackBuffer_.Dispose();
    }

    public int GetValue(Texture texture)
    {
        return Decode(texture);
    }

    int Decode(Texture source)
    {
        Shader.SetTexture(kernelIndex_, "Source", source);
        Shader.SetInt("Row", Row);
        Shader.SetInt("Column", Column);
        Shader.SetBuffer(kernelIndex_, "Result", readbackBuffer_);
        Shader.Dispatch(kernelIndex_, Row, Column, 1);

        readbackBuffer_.GetData(data_);

        int value = 0;
        for (int i = 0; i < data_.Length; i++)
        {
            if (data_[i].grayscale > 0.5)
                value += 1 << i;
        }
        return value;
    }
}
