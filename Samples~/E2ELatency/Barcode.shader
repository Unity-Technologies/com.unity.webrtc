Shader "Unlit/Barcode"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color1 ("Color1", Color) = (0,0,0,1)
        _Color2 ("Color2", Color) = (1,1,1,1)
        _Row ("Row", int) = 5
        _Column("Column", int) = 5
        _Value ("Value", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            fixed4 _Color1;
            fixed4 _Color2;
            int _Value;
            int _Row;
            int _Column;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = float4(v.texcoord1.xy, 0, 0);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                int x = floor(i.uv.x * _Row);
                int y = floor(i.uv.y * _Column);
                int bit = x + y * _Row;

                if ((_Value >> bit) & 1)
                    return _Color2;
                return _Color1;
            }
            ENDCG
        }
    }
}
