Shader "UI/StrangerThingsReveal"
{
    Properties
    {
        _MainTex ("Top Texture", 2D) = "white" {}
        _RevealTex ("Reveal Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _MousePos ("Mouse Pos", Vector) = (0.5,0.5,0,0)
        
        // These defaults will be overwritten by the script
        _Radius ("Radius", Float) = 0.15
        _Softness ("Edge Softness", Float) = 0.03
        
        _GlowStrength ("Glow Strength", Float) = 2.0
        _DistortStrength ("Distortion", Float) = 0.02
        _TimeScale ("Pulse Speed", Float) = 2.0
        
        _Aspect ("Aspect Ratio", Float) = 1.0 
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _RevealTex;
            sampler2D _NoiseTex;

            float4 _MousePos;
            float _Radius;
            float _Softness;
            float _GlowStrength;
            float _DistortStrength;
            float _TimeScale;
            float _Aspect; 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Noise & Distortion
                float2 noiseUV = i.uv + _Time.y * 0.1;
                float noise = tex2D(_NoiseTex, noiseUV).r;
                float2 offset = (noise - 0.5) * _DistortStrength;
                float2 distortedUV = i.uv + offset;

                fixed4 top = tex2D(_MainTex, distortedUV);
                fixed4 reveal = tex2D(_RevealTex, distortedUV);
                
                // --- 2. ASPECT CORRECTION ---
                // We scale the X-axis by the calculated Aspect from the script
                float2 aspectScale = float2(_Aspect, 1.0);

                float2 uvScaled = distortedUV * aspectScale;
                float2 mouseScaled = _MousePos.xy * aspectScale;

                float dist = distance(uvScaled, mouseScaled);
                // ------------------------------

                // 3. Mask & Glow
                float mask = smoothstep(_Radius, _Radius - _Softness, dist);

                float pulse = sin(_Time.y * _TimeScale) * 0.5 + 0.5;
                float glow = smoothstep(_Radius + 0.02, _Radius - 0.01, dist);
                glow *= pulse * _GlowStrength;

                // 4. Combine
                fixed4 col = lerp(top, reveal, mask);
                col.rgb += float3(0.8, 0.1, 0.1) * glow; 

                return col;
            }
            ENDCG
        }
    }
}