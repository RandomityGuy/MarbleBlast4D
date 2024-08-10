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
      #include "noise.cginc"
      #define PROC_TEXTURE
      #define USE_DITHER
      #define LOCAL_UV
   
        float fBm(float4 p)
        {
            float sum = 0.0;
            float amplitude = 1.0;
            for (int i = 0; i < 2; i++)
            {
                sum += amplitude * snoise(p);
                amplitude *= 0.5;
                p *= 2.0;
            }
            return sum;
        }

        float3 hsv2rgb(float3 c)
        {
            float3 rgb = clamp(abs(fmod(c.x * 6.0 + float3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);

            return c.z * lerp(float3(1.0, 1.0, 1.0), rgb, c.y);
        }

        static const float4x4 rot = float4x4(
          0.36, 0.48, -0.8, 0.5,
          -0.8, 0.6, 0.0, -0.5,
          0.48, 0.64, 0.6, 0.0,
          0.0, 0.0, 0.0, 0.0);

        float3 getColor(float4 pos)
        {
            float4 inp = pos;
            // Apply noise
            // pos.w = sin(frac(pos.w) * 6.28);
            pos = fmod(pos, 5.0) / 5.0;
            float4 uv2 = pos + mul(rot, pos);//  snoise(pos * 1.2);
            float f = snoise(2 * uv2);
            f = frac(f * 2.0);
            //f = f - floor(f);
            float f2 = 0.02;
            f2 = f2 + 0.08 * smoothstep(0.08, 0.12, f);
            f2 = f2 + 0.08 * smoothstep(0.21, 0.25, f);
            f2 = f2 + 0.06 * smoothstep(0.33, 0.37, f);
            f2 = f2 + 0.23 * smoothstep(0.46, 0.50, f);
            f2 = f2 + 0.08 * smoothstep(0.58, 0.62, f);
            f2 = f2 + 0.12 * smoothstep(0.71, 0.75, f);
            f2 = f2 + 0.12 * smoothstep(0.83, 0.87, f);
            f2 = f2 + 0.23 * smoothstep(0.96, 1.00, f);
            
            float3 col = hsv2rgb(float3(f2, 1.0, 1.0));
            return col;
            // return uv.xyz;
        }

      #define apply_proc_tex4D() \
        color.rgb = getColor(i.uv);
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
