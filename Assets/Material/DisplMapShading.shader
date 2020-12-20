Shader "DisplacementMapShader"
{
	Properties
	{
		// Definiere _DisplacementExtension, dieser Wert regelt den Grad des Displacements
		_DisplacementExtension("Terrain Scale", Range(0, 1)) = 0
		
		// Definiere _LiquidStartingPoint, dieser Wert legt fest, bei welcher Höhe nur noch Flüssigkeit angezeigt werden soll
		_LiquidStartingPoint("Liquid threshold", Range(0, 1)) = 0

		// Definiere _HeightMap, _MoistureMap, und _ColorMap, diese können über einen Input in der GUI zugewiesen werden
		_HeightMap("Height Map", 2D) = "normal" {}
		_MoistureMap("Moisture Map", 2D) = "normal" {}
		_ColorMapLand("Color Map Land", 2D) = "normal" {}
		_ColorMapWater("Color Map Water", 2D) = "normal" {}

		// Definiere Hautfarbe, Reflexion Ambienten Licht, Reflexion Diffusen Licht, Reflexion Spekular, Glanzgrad
		_Color("Base Color", Color) = (1,1,1,1)
		_Ka("Ambient Reflectance", Range(0, 1)) = 0.5
		_Kd("Diffuse Reflectance", Range(0, 1)) = 0.5
		_Ks("Specular Reflectance", Range(0, 1)) = 0.5
		_Shininess("Shininess", Range(0, 100)) = 100

		_Speed("Speed", Range(0, 1)) = 0.5
		_NormalMap1("Normal Map 1", 2D) = "bump" {}
		_NormalMap2("Normal Map 2", 2D) = "bump" {}

	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		// Level of detail for shaders https://docs.unity3d.com/Manual/SL-ShaderLOD.html
		LOD 100

		Pass
		{

			Tags {"LightMode"="ForwardBase"}

			CGPROGRAM


			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			sampler2D _HeightMap;
			sampler2D _MoistureMap;
			sampler2D _ColorMapLand;
			sampler2D _ColorMapWater;
			float4 _HeightMap_ST;
			float _DisplacementExtension;
			float _LiquidStartingPoint;

			float checkLiquidThreshold;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half3 worldNormal : TEXCOORD1;
				half3 worldViewDir : TEXCOORD2;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD6;
				//half3 mapikusnormalis : TEXCOORD3;
				float4 col : COLOR;
				half3 texValMoisture : TEXCOORD4;
				half3 texVal : TEXCOORD5;
			};

			float _MaxDepth;
			float _Ka, _Kd, _Ks, _Shininess;

			sampler2D _NormalMap1;
			sampler2D _NormalMap2;

			float4 _NormalMap1_ST;
			float4 _NormalMap2_ST;

			float _Speed;

			// VERTEX SHADER
			v2f vert(appdata_full v)
			{
				

				v2f o;

				// Farben aus der Textur extrahieren --> Aus Übung 3.3 #Es gibt keine Tutorials dafür
				o.texVal = tex2Dlod(_HeightMap, float4(v.texcoord.xy, 0, 0));
				o.texValMoisture = tex2Dlod(_MoistureMap, float4(v.texcoord.xy, 0, 0));

				// Da die Heightmap nur Werte zwischen 0 und 1 besitzt, kann hier darauf geprüft werden, ob der "Höhenwert" eines Pixels unterhalb unserer Flüssigkeitsschwelle liegt
				if (o.texVal.y <= _LiquidStartingPoint) {
					// Hier wird das Displacement angewandt, je Höher der "Höhenwert" des Pixels ist, desto häher erscheint der vertex auf dem Objekt, 
					// alle Pixel die unter oder auf dem Schwellenwert liegen, erhalten denselben Wert
					v.vertex.xyz += v.normal * _LiquidStartingPoint * _DisplacementExtension;
					o.vertex = UnityObjectToClipPos(v.vertex);

					o.worldNormal = UnityObjectToWorldNormal(v.normal);
					o.worldViewDir = normalize(WorldSpaceViewDir(v.vertex));
				}
				else {
					// Hier wird das Displacement angewandt, je Höher der "Höhenwert" des Pixels ist, desto häher erscheint der vertex auf dem Objekt
					// alle vertex die über dem Schwellwert liegen, erhalten einen neuen Höhenwert, abhängig von dem Höhenwert des Pixels
					v.vertex.xyz += v.normal * _DisplacementExtension * o.texVal.y;
					o.vertex = UnityObjectToClipPos(v.vertex);	

					o.worldNormal = UnityObjectToWorldNormal(v.normal);
				}

				// Farbe des Objekts soll der der Map gleichen
				//o.col = texVal;

				return o;
			}

			// FRAGMENT / PIXEL SHADER
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col;

				//Variablen
				half re;
				half nl;
				float4 ambientLight;
				float4 diffuseLight;
				float3 worldSpaceReflection;
				float4 spec;

				if (i.texVal.y <= _LiquidStartingPoint)
				{
					float BRA = (_LiquidStartingPoint - i.texVal.y) / (_LiquidStartingPoint);
					i.col = tex2Dlod(_ColorMapWater, float4(i.texValMoisture.y, BRA, 0, 0));
					
					// Phong-Shading
					ambientLight = float4(ShadeSH9(half4(i.worldNormal, 1)), 1);
					nl = max(0, dot(i.worldNormal, _WorldSpaceLightPos0.xyz));
					diffuseLight = nl * _LightColor0;
					worldSpaceReflection = reflect(normalize(-_WorldSpaceLightPos0.xyz), i.worldNormal);
					re = pow(max(dot(worldSpaceReflection, i.worldViewDir), 0), _Shininess);
					spec = re * _LightColor0;
					// Farbe wird mit dem diffusen und ambiente Licht anteilig verrechnet
					// Zudem muss die Farbe noch mit der Reflexion der Oberfläche verrechnet werden
					i.col *= _Ka * ambientLight + _Kd * diffuseLight;
					i.col += _Ks * spec;
					
				}
				else
				{
					float BRA = (i.texVal.y - _LiquidStartingPoint) / (1 - _LiquidStartingPoint);
					i.col = tex2Dlod(_ColorMapLand, float4(i.texValMoisture.y, BRA, 0, 0));
					
					// Lambert-Shading
					// Schafft einen übergang von hell nach dunkel
					ambientLight = float4(ShadeSH9(half4(i.worldNormal, 1)), 1);
					nl = max(0, dot(i.worldNormal, _WorldSpaceLightPos0.xyz));
					diffuseLight = nl * _LightColor0;

					// Farbe wird mit dem diffusen und ambiente Licht anteilig verrechnet
					i.col *= (_Ka * ambientLight +  _Kd * diffuseLight);
					
				}
				col = i.col;
				return col;
			}
			ENDCG
		}
	}
}
