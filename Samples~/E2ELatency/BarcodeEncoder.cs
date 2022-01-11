using System;
using UnityEngine;
using UnityEngine.UI;

public class BarcodeEncoder : MonoBehaviour
{
    [SerializeField] int Row = 5;
    [SerializeField] int Column = 5;
    Material material_;

    public Material Material => material_;

    private void Awake()
    {
        material_ = GetComponent<RawImage>().material;

        if(Row * Column > 32)
            throw new InvalidOperationException("Not supported over 32bit numbers");

        material_.SetInt("_Row", Row);
        material_.SetInt("_Column", Column);
    }

    public void SetValue(int value)
    {
        material_.SetInt("_Value", value);
    }
}
