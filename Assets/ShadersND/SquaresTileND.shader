Shader "Custom/SquaresTileND" {
  Properties{
    _Color("Colorize", Color) = (1.0,1.0,1.0,1.0)
    _SquareColor("Square Color", Color) = (0.0,0.0,0.0,1.0)
    _Color2("Line Color", Color) = (0.0,0.0,0.0,1.0)
    _ShadowDist("ShadowDist", Float) = 40.0
    _Ambient("Ambient", Float) = 0.6
    _SpecularMul("SpecularMul", Float) = 0.0
    _SpecularPow("SpecularPow", Float) = 0.0
    _GridFreq("Grid Freq", Float) = 1.0
    _DitherDist("Dither Distance", Float) = 3.0
    _DitherRadius("Dither Radius", Float) = 2.0
  }

  //Opaque shader
  SubShader{
    Tags{
      "Queue" = "Geometry"
      "LightMode" = "ForwardBase"
      "RenderType" = "Opaque"
    }

    Pass {
      Cull Back
      ZTest LEqual
    
      CGPROGRAM
      #define PROC_TEXTURE
//#if !defined(IS_EDITOR)
//      #define USE_DITHER
//#endif
      #define LOCAL_UV
      #define W 0.05
      float4 _Color2;
      float4 _SquareColor;
      float _GridFreq;
      #define R(U,n,s) (   \
          float(   b.x > 0. && b.y > 0.    \
                && 1.-s/n < U.y            \
                && U.y < 1.+(1.-s)/n ) )


      #define apply_proc_tex4D() \
        float detail = saturate(_GridFreq * length(i.viewDir) / (_ScreenParams.x * (0.05 + abs(dot(vd, n))))); \
        detail = 1.0 - (1.0 - detail) * (1.0 - detail); \
        float4 f = _GridFreq * i.uv + 0.5; \
        f = f - floor(f); \
        float4 q = max(f - 1.0 + detail, 0.0) + max(W - f, 0.0); \
        q = min(q, min(W, detail)); \
        float grid = max(max(q.x, q.y), max(q.z, q.w)) / detail; \
        color.rgb = lerp(color.rgb, _Color2.rgb, grid); \
        float4 uvc = frac((i.uv ) * 1.5 + float4(0.25, 0, 0.25, 0.25)) - 0.25; \
        float dist = max(max(abs(uvc.x), abs(uvc.y)), max(abs(uvc.z), abs(uvc.w))); \
        float show = step(0.0, 0.3 - dist); \
        color.rgb = lerp(color.rgb, _SquareColor, show);
      #define apply_proc_tex5D() \
        float detail = saturate(_GridFreq * Length5(i.viewDir, i.v_nud.z) / (_ScreenParams.x * (0.05 + abs(Dot5(vd, vd_v, n, n_v))))); \
        detail = 1.0 - (1.0 - detail) * (1.0 - detail); \
        float4 f = _GridFreq * i.uv + 0.5; \
        float fv = _GridFreq * i.v_nud.y + 0.5; \
        f = f - floor(f); \
        fv = fv - floor(fv); \
        float4 q = max(f - 1.0 + detail, 0.0) + max(W - f, 0.0); \
        float qv = max(fv - 1.0 + detail, 0.0) + max(W - fv, 0.0); \
        q = min(q, min(W, detail)); \
        qv = min(qv, min(W, detail)); \
        float grid = max(max(max(q.x, q.y), max(q.z, q.w)), qv) / detail; \
        color.rgb = lerp(color.rgb, _Color2.rgb, grid);
      #include_with_pragmas "CoreND.cginc"
        /*float detail = saturate(40.0 * abs(dot(vd, n)) / (0.01 + length(i.viewDir))); \
        float4 twave = i.uv * _GridFreq; \
        twave = abs(1.0 - 2.0*(twave - floor(twave))); \
        float grid = max(max(twave.x, twave.y), max(twave.z, twave.w)); \
        float lowerBound = lerp(0.96, 0.0, 0.2 * (1.0 - detail)); \
        grid = smoothstep(lowerBound, 0.99, grid); \
        grid = lerp(0.05, grid, 1.0 - (1.0 - detail) * (1.0 - detail)); \
        color.rgb = lerp(color.rgb, _Color2.rgb, grid);*/
      ENDCG
    }
  }
}
