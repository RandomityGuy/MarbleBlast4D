Shader "Custom/DiffuseAlphaND" {
  Properties{
    _Color("Colorize", Color) = (1.0,1.0,1.0,1.0)
    _ShadowDist("ShadowDist", Float) = 40.0
    _Ambient("Ambient", Float) = 0.6
    _SpecularMul("SpecularMul", Float) = 0.0
    _SpecularPow("SpecularPow", Float) = 0.0
    _ColorAlpha("Alpha", Float) = 1.0
  }

  //Opaque shader
  SubShader{
    Tags{
      "Queue" = "Transparent"
      "LightMode" = "ForwardBase"
      "RenderType" = "Transparent"
    }

    ZWrite Off

    Pass {
      Cull Back
      ZTest LEqual
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
    
      CGPROGRAM
      float _ColorAlpha;
      #pragma vertex vert alpha:blend
      #pragma fragment frag alpha:blend
      #define USE_ALPHA
      #define SHADOW_ALPHA
      #define PROC_TEXTURE
      #pragma shader_feature VERTEX_AO
      #define apply_proc_tex4D() \
        color.rgb = UNITY_ACCESS_INSTANCED_PROP(Props, _Color).rgb; \
        color.a = _ColorAlpha;
      #define apply_proc_tex5D() \
        color.rgb = UNITY_ACCESS_INSTANCED_PROP(Props, _Color).rgb; \
        color.a = _ColorAlpha;

      #include_with_pragmas "CoreND.cginc"
      ENDCG
    }
  }
  CustomEditor "GeneralEditor"
}
