Shader "Custom/RepeatCellTextureND" {
  Properties{
    _Color("Colorize", Color) = (1.0,1.0,1.0,1.0)
    _CellTex("Texture", 3D) = "" {}
    _ShadowDist("ShadowDist", Float) = 40.0
    _Ambient("Ambient", Float) = 0.6
    _SpecularMul("SpecularMul", Float) = 0.0
    _SpecularPow("SpecularPow", Float) = 0.0
    _OffsetX("Offset X", Float) = 0.0
    _OffsetY("Offset Y", Float) = 0.0
    _OffsetZ("Offset Z", Float) = 0.0
    _OffsetW("Offset W", Float) = 0.0
  }

  //Opaque shader
  SubShader{
    Tags{
      "Queue" = "Geometry"
      "LightMode" = "ForwardBase"
      "RenderType" = "Opaque"
    }

    Pass {
      Cull Off
      ZTest LEqual
    
      CGPROGRAM
      #define PROC_TEXTURE
      #define CELL_AO
      float _OffsetX;
      float _OffsetY;
      float _OffsetZ;
      float _OffsetW;
      #define apply_proc_tex4D() \
        float3 indexer =  ((i.uv.xyz + float3(0, 0, 0))) * 2.0 + float3(_OffsetX, _OffsetY, _OffsetZ); \
        color.rgb *= tex3D(_CellTex, indexer);
      #define apply_proc_tex5D()
      sampler3D _CellTex;
      #include_with_pragmas "CoreND.cginc"
      ENDCG
    }
  }
  CustomEditor "GeneralEditor"
}
