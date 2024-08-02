Shader "Custom/MarbleND" {
  Properties{
    _Color("Colorize", Color) = (1.0,1.0,1.0,1.0)
    _ShadowDist("ShadowDist", Float) = 40.0
    _Ambient("Ambient", Float) = 0.6
    _SpecularMul("SpecularMul", Float) = 0.0
    _SpecularPow("SpecularPow", Float) = 0.0
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
      #pragma shader_feature LOCAL_UV
      #define PROC_TEXTURE
      #define USE_DITHER
      #define hash(p) frac(sin(dot(p,float3(127.1,311.7, 74.7)))*43758.5453123)

      float noise(float3 p)
      {
          float3 i = floor(p);
          float3 f = frac(p);
          f = f * f * (3. - 2. * f); // smoothstep
      
          float v = lerp(lerp(lerp(hash(i+float3(0.0,0.0,0.0)), hash(i+float3(1.0,0.0,0.0)), f.x),
                             lerp(hash(i+float3(0.0,1.0,0.0)), hash(i+float3(1.0,1.0,0.0)), f.x), f.y),
                        lerp(lerp(hash(i+float3(0.0,0.0,1.0)), hash(i+float3(1.0,0.0,1.0)), f.x),
                             lerp(hash(i+float3(0.0,1.0,1.0)), hash(i+float3(1.0,1.0,1.0)), f.x), f.y), f.z);
          return v;
      }

       #define rot(a) float2x2(cos(a),-sin(a),sin(a),cos(a))
       
       float fbm(float3 p)
       {
           float v = 0., a = .5;
           float2x2 R = rot(.37);
       
        for (int i = 0; i < 9; i++, p *= 2., a /= 2.)
        {
            p.xy = mul(p.xy, R);
            p.yz = mul(p.yz, R);
        }
               v += a * noise(p);
       
           return v;
       }

      #define apply_proc_tex4D() \
        float4 O =  cos( 9.*fbm(i.uv.xyz)+ float4(0.0,23.0,21.0,0.0)); \
        color.rgb = O.rgb;
      #define apply_proc_tex5D() \
        float4 checker = floor(i.uv * 3.0 + 0.01); \
        float checker_V = floor(i.v_nud.y * 3.0 + 0.01); \
        color.rgb *= 0.9 + 0.2 * frac((checker.x + checker.y + checker.z + checker.w + checker_V) * 0.5);
      #include_with_pragmas "CoreND.cginc"
      ENDCG
    }
  }
  CustomEditor "GeneralEditor"
}
