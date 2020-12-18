﻿Shader "CG_Lecture/DisplacementMapShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_DisplacementExtension("Terrain Scale", Range(0, 1)) = 0.01
		_LiquidStartingPoint("Liquid threshold", Range(0, 1)) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Overlay" }
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _DisplacementExtension;
			float _LiquidStartingPoint;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 col : COLOR;
			};

			float _MaxDepth;

			// VERTEX SHADER
			v2f vert(appdata_full v)
			{
				v2f o;

				// Farben aus der Textur extrahieren --> Aus Übung 3.3 #Es gibt keine Tutorials dafür...
				fixed4 texVal = tex2Dlod(_MainTex, float4(v.texcoord.xy, 0, 0));

				if (texVal.y < _LiquidStartingPoint) {
					v.vertex.xyz += v.normal * _LiquidStartingPoint * _DisplacementExtension;
				}
				else {
					// displace z value of vertex by texture value multiplied with Scale
					v.vertex.xyz += v.normal * _DisplacementExtension * texVal.y;
				}

				// Convert Vertex Data from Object to Clip Space
				o.vertex = UnityObjectToClipPos(v.vertex);

				// set texture value as color.
				o.col = texVal;

				return o;
			}

			// FRAGMENT / PIXEL SHADER
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = i.col;
				return col;
			}
			ENDCG
		}
	}
}
