using UnityEngine;

public class BarcodeDecoder : MonoBehaviour
{
    [SerializeField] int Row;
    [SerializeField] int Column;
    [SerializeField] ComputeShader Shader;

    GraphicsBuffer readbackBuffer_;
    Color[] data_;

    private void Awake()
    {
        int count = Row * Column;
        int stride = sizeof(float) * 4;
        readbackBuffer_ =
            new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, stride);
        data_ = new Color[count];
    }

    public int GetValue(Texture texture)
    {
        return Decode(texture);
    }

    int Decode(Texture source)
    {
        Shader.SetTexture(0, "Source", source);
        Shader.SetInt("Row", Row);
        Shader.SetInt("Column", Column);
        Shader.SetBuffer(0, "Result", readbackBuffer_);
        Shader.Dispatch(0, Row, Column, 1);

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
