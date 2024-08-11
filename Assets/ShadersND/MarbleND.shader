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
   
        float noise_fbm(float4 p, float detail, float roughness, float lacunarity, bool normalize)
        {
            float fscale = 1.0; 
            float amp = 1.0; 
            float maxamp = 0.0; 
            float sum = 0.0; 
    
            for (int i = 0; i <= int(detail); i++) { 
              float t = snoise(fscale * p); 
              sum += t * amp; 
              maxamp += amp; 
              amp *= roughness; 
              fscale *= lacunarity; 
            } 
            float rmd = detail - floor(detail); 
            if (rmd != 0.0) { 
              float t = snoise(fscale * p); 
              float sum2 = sum + t * amp; 
              return normalize ? 
                         lerp(0.5 * sum / maxamp + 0.5, 0.5 * sum2 / (maxamp + amp) + 0.5, rmd) : 
                         lerp(sum, sum2, rmd); 
            } 
            else { 
              return normalize ? 0.5 * sum / maxamp + 0.5 : sum; 
            } 
        }

        float3 hsv2rgb(float3 c)
        {
            float3 rgb = clamp(abs(fmod(c.x * 6.0 + float3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);

            return c.z * lerp(float3(1.0, 1.0, 1.0), rgb, c.y);
        }

        float3 magic(float3 p, float scale, float distortion)
        {
          float dist = distortion;

          float a = fmod(p.x * scale, 2 * 3.1415926);
          float b = fmod(p.y * scale, 2 * 3.1415926);
          float c = fmod(p.z * scale, 2 * 3.1415926);

          float x = sin((a + b + c) * 5.0);
          float y = cos((-a + b - c) * 5.0);
          float z = -cos((-a - b + c) * 5.0);

          
            x *= dist;
            y *= dist;
            z *= dist;
            y = -cos(x - y + z);
            y *= dist;
          

          if (dist != 0.0) {
            dist *= 2.0;
            x /= dist;
            y /= dist;
            z /= dist;
          }

          return float3(0.5 - x, 0.5 - y, 0.5 - z);
        }

        static const float4x4 rot = float4x4(
          0.36, 0.48, -0.8, 0.5,
          -0.8, 0.6, 0.0, -0.5,
          0.48, 0.64, 0.6, 0.0,
          0.0, 0.0, 0.0, 0.0);

        float4 random_vector4_offset(float4 v)
        {
            return float4(100.0 + mul(rot, v) * 100.0);
        }


        float3 mbgTex(float4 pos)
        {
            pos /= 1000;
            float3 magicVec = magic(pos.xyz, 0.150, 1.7);
            float magicFac = (magicVec.x + magicVec.y + magicVec.z) / 3.0;
            float4 noiseInp = float4(pos.xyz, magicFac * 8.54);
            float4 distort = float4(snoise(noiseInp + random_vector4_offset(pos.xyzw)) * 0.42,
                snoise(noiseInp + random_vector4_offset(pos.yzxw)) * 0.42, snoise(noiseInp + random_vector4_offset(pos.xzyw)) * 0.42,
            snoise(noiseInp + random_vector4_offset(pos.ywzx)) * 0.42);
            noiseInp += distort;
            float noiseFac = frac(noise_fbm(noiseInp, 1.0, 1.0, 2.0, false) * 2.91);
    
            float f = noiseFac;
            //float f2 = 0.036;
            //float c1 = lerp(0.036, 0.0, noiseFac / 0.048);
            //float c2 = lerp(0.0, 0.833, (noiseFac - 0.190) / 0.048);
            //float c3 = lerp(0.833, 0.677, (noiseFac - 0.333) / 0.048);
            //float c4 = lerp(0.667, 0.5, (noiseFac - 0.476) / 0.048);
            //float c5 = lerp(0.5, 0.333, (noiseFac - 0.619) / 0.048);
            //float c6 = lerp(0.333, 0.167, (noiseFac - 0.714) / 0.048);
            //float c7 = lerp(0.167, 0.036, (noiseFac - 0.810) / 0.048);
            //f2 = step(0.048, noiseFac) * step(noiseFac, 0.095) * c1 + 
            //    step(0.190, noiseFac) * step(noiseFac, 0.238) * c2 +
            //    step(0.333, noiseFac) * step(noiseFac, 0.381) * c3 +
            //    step(0.476, noiseFac) * step(noiseFac, 0.524) * c4 +
            //    step(0.619, noiseFac) * step(noiseFac, 0.667) * c5 +
            //    step(0.762, noiseFac) * step(noiseFac, 0.810) * c6 +
            //    step(0.905, noiseFac) * step(noiseFac, 0.952) * c7;
            // f2 = c2;
            //float f = noiseFac;
            //float f2 = 0.036;
            //f2 = f2 + 0.036 * (1.0 - lerp(0.0, 0.048, f));
            //f2 = f2 + 0.833 * (1.0 - lerp(0.190, 0.238, f));
            //f2 = f2 + 0.667 * lerp(0.333, 0.381, f);
            //f2 = f2 + 0.5 * lerp(0.476, 0.524, f);
            //f2 = f2 + 0.333 * lerp(0.619, 0.667, f);
            //f2 = f2 + 0.167 * lerp(0.714, 0.762, f);
            //f2 = f2 + 0.036 * lerp(0.810, 0.857, f);
            float f2 = 0.02;
    
            f2 = f2 + 0.08 * (clamp(noiseFac, 0.08, 0.12) - 0.08 )/ (0.04);
            f2 = f2 + 0.08 * (clamp(noiseFac, 0.21, 0.25) - 0.21 )/ (0.04);
            f2 = f2 + 0.06 * (clamp(noiseFac, 0.33, 0.37) - 0.33 )/ (0.04);
            f2 = f2 + 0.23 * (clamp(noiseFac, 0.46, 0.50) - 0.46 )/ (0.04);
            f2 = f2 + 0.08 * (clamp(noiseFac, 0.58, 0.62) - 0.58 )/ (0.04);
            f2 = f2 + 0.12 * (clamp(noiseFac, 0.71, 0.75) - 0.71 )/ (0.04);
            f2 = f2 + 0.12 * (clamp(noiseFac, 0.83, 0.87) - 0.83 )/ (0.04);
            f2 = f2 + 0.23 * (clamp(noiseFac, 0.96, 1.00) - 0.96 )/ (0.04);
    
            //f2 = f2 + 0.08 * smoothstep(0.08, 0.12, noiseFac);
            //f2 = f2 + 0.08 * smoothstep(0.21, 0.25, noiseFac);
            //f2 = f2 + 0.06 * smoothstep(0.33, 0.37, noiseFac);
            //f2 = f2 + 0.23 * smoothstep(0.46, 0.50, noiseFac);
            //f2 = f2 + 0.08 * smoothstep(0.58, 0.62, noiseFac);
            //f2 = f2 + 0.12 * smoothstep(0.71, 0.75, noiseFac);
            //f2 = f2 + 0.12 * smoothstep(0.83, 0.87, noiseFac);
            //f2 = f2 + 0.23 * smoothstep(0.96, 1.00, noiseFac);
    
            float3 col = hsv2rgb(float3(f2, 1.0, 1.0));
            return col;
        }

        //float fBm(float4 p)
        //{
        //    float sum = 0.0;
        //    float amplitude = 1.0;
        //    for (int i = 0; i < 2; i++)
        //    {
        //        sum += amplitude * snoise(p);
        //        amplitude *= 0.5;
        //        p *= 2.0;
        //    }
        //    return sum;
        //}

        //float3 getColor(float4 pos)
        //{
        //    float4 inp = pos;
        //    // Apply noise
        //    // pos.w = sin(frac(pos.w) * 6.28);
        //    pos = fmod(pos, 5.0) / 5.0;
        //    float4 uv2 = pos + mul(rot, pos);//  snoise(pos * 1.2);
        //    float f = snoise(2 * uv2);
        //    f = frac(f * 2.0);
        //    //f = f - floor(f);
        //    float f2 = 0.036;
        //    f2 = f2 + 0.036 * (1.0 - lerp(0.0, 0.048, f));
        //    f2 = f2 + 0.833 * (1.0 - lerp(0.190, 0.238, f));
        //    f2 = f2 + 0.667 * lerp(0.333, 0.381, f);
        //    f2 = f2 + 0.5 * lerp(0.476, 0.524, f);
        //    f2 = f2 + 0.333 * lerp(0.619, 0.667, f);
        //    f2 = f2 + 0.167 * lerp(0.714, 0.762, f);
        //    f2 = f2 + 0.036 * lerp(0.810, 0.857, f);
        //    //f2 = f2 + 0.08 * smoothstep(0.08, 0.12, f);
        //    //f2 = f2 + 0.08 * smoothstep(0.21, 0.25, f);
        //    //f2 = f2 + 0.06 * smoothstep(0.33, 0.37, f);
        //    //f2 = f2 + 0.23 * smoothstep(0.46, 0.50, f);
        //    //f2 = f2 + 0.08 * smoothstep(0.58, 0.62, f);
        //    //f2 = f2 + 0.12 * smoothstep(0.71, 0.75, f);
        //    //f2 = f2 + 0.12 * smoothstep(0.83, 0.87, f);
        //    //f2 = f2 + 0.23 * smoothstep(0.96, 1.00, f);
            
        //    float3 col = hsv2rgb(float3(f2, 1.0, 1.0));
        //    return col;
        //    // return uv.xyz;
        //}

      #define apply_proc_tex4D() \
        color.rgb = mbgTex(i.uv);
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
