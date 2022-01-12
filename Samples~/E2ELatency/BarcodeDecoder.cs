using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarcodeDecoder : MonoBehaviour
{
    [SerializeField] int Row;
    [SerializeField] int Column;
    [SerializeField] ComputeShader Shader;

    Texture texture_;
    GraphicsBuffer readbackBuffer_;

    private void Awake()
    {
        int count = Row * Column;
        int stride = sizeof(float) * 4;
        readbackBuffer_ =
            new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, stride);
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
        Color[] array = new Color[Row * Column];
        readbackBuffer_.GetData(array);

        int value = 0; 
        for(int i = 0; i < array.Length; i++)
        {
            if(array[i].grayscale > 0.5)
                value += 1 << i;
        }
        return value;
    }
}
