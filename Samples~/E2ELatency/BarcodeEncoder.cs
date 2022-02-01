using System;
using UnityEngine;
using UnityEngine.UI;

class BarcodeEncoder : MonoBehaviour
{
    [SerializeField] int Row = 5;
    [SerializeField] int Column = 5;
    [SerializeField] Shader Shader;
    Material material_;

    public Material Material => material_;

    private void Awake()
    {
        if (Shader == null)
            throw new InvalidOperationException("Shader is null");
        if (Row * Column > 32)
            throw new InvalidOperationException("Not supported over 32bit numbers");

        material_ = new Material(Shader);
        GetComponent<RawImage>().material = material_;

        material_.SetInt("_Row", Row);
        material_.SetInt("_Column", Column);
    }

    public void SetValue(int value)
    {
        material_.SetInt("_Value", value);
    }
}
