// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Scope Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Reticle Color", color) = (0, 0, 0, 1)
        _ReticleTex ("Reticle", 2D) = "transparent" {}
        _ViewportWidth ("Viewport Width", float) = 1
        _ReticleSize ("Reticle Size", float) = 1
        _DistanceFallof ("Distance Fallof", float) = 1
        _TextureFit("Texture scale", float) = 1
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
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float3 tangent : TANGENT;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 pos : TEXCOORD0;
                float3 dir : TEXCOORD1;
                float3 normal : NORMAL;
                float3 tangent : TANGENT;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.pos = UnityObjectToViewPos(v.vertex);
                o.dir = ObjSpaceViewDir(v.vertex);         //transform vertex into eye space
                o.normal = mul(UNITY_MATRIX_IT_MV, v.normal);   //transform normal into eye space
                o.tangent = mul(UNITY_MATRIX_IT_MV, v.tangent); //transform tangent into eye space
                return o;
            }

            struct Input {
                float2 uv_MainTex;
                float2 uv_MainTex2;
            };

            sampler2D _MainTex;
            sampler2D _ReticleTex;
            float4 _MainTex_ST;
            float _ViewportWidth;
            float _ReticleSize;
            float4 _Color;
            float _DistanceFallof;
            float _TextureFit;

            // v2f vert (appdata v)
            // {
            //     v2f o;
            //     o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            //     // o.vertex = UnityObjectToClipPos(v.vertex);
            //     // o.tangent = v.tangent; //transform tangent into eye space
            //     // o.normal = UnityObjectToClipPos(v.normal);
            //     UNITY_TRANSFER_FOG(o,o.vertex);
            //     return o;
            // }

            fixed4 frag (v2f i) : SV_Target {
                float3 normal = normalize(i.normal);    //get normal of fragment
                float3 tangent = normalize(i.tangent);  //get tangent
                float3 cameraDir = normalize(i.pos);    //get direction from camera to fragment, normalize(i.pos - float3(0, 0, 0))

                float3 offset = cameraDir + normal;     //calculate offset from two points on unit sphere, cameraDir - -normal

                float3x3 mat = float3x3 (
                    tangent,
                    cross(normal, tangent),
                    normal
                );

                offset = mul(mat, offset);

                float2 uv = offset.xy;

                fixed4 maintex_sample = tex2D(_MainTex, (uv + float2(0.5 / _TextureFit, 0.5 / _TextureFit)) * _TextureFit);
                // fixed4 maintex_sample = tex2D(_MainTex, uv);
                fixed4 reticle_sample = tex2D(_ReticleTex, uv / _ReticleSize + float2(0.5, 0.5));
                reticle_sample.xyz = dot(reticle_sample.xyz, float3(0.299, 0.587, 0.114)) * _Color.xyz;

                fixed4 col = lerp(maintex_sample, reticle_sample, reticle_sample.a);
                // fixed4 col = ;
                // col.xyz = tangent.xyz;

                float mask = pow(1 - (length(offset)) + _ViewportWidth - (length(i.dir) * _DistanceFallof), 100);

                mask = clamp(mask, 0, 1);

                if (uv.x < -0.5 || uv.x > 0.5 || uv.y < -0.5 || uv.y > 0.5) mask = 0;

                col *= mask;

                return col;
            }
            ENDCG
        }
    }
}
