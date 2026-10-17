// URP-compatible reimplementation of the legacy fixed-function Spine/Skeleton shader.
// Fixed-function passes (SetTexture/ColorMaterial) are not rendered by SRP (Unity 7 disables
// Built-in RP), so the pass is a plain CG vertex/fragment unlit pass with identical
// tags/cull/blend and the same texture * vertex-color math.
Shader "Spine/Skeleton" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		_MainTex ("Texture to blend", 2D) = "black" {}
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
		LOD 100

		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass {
			Name "Unlit"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata_t v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				fixed4 texcol = tex2D(_MainTex, i.texcoord);
				fixed4 c;
				c.rgb = texcol.rgb * i.color.rgb;
				c.a = texcol.a * i.color.a;
				return c;
			}
			ENDCG
		}
	}
}
