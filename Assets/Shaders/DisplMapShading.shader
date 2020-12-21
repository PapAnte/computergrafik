Shader "DisplMapShading"
{
	Properties
	{
		// Definiere _DisplacementExtension, dieser Wert regelt den Grad des Displacements
		_DisplacementExtension("Terrain Scale", Range(0, 1)) = 0.5
		
		// Definiere _LiquidStartingPoint, dieser Wert legt fest, bei welcher Höhe nur 
		// noch Flüssigkeit angezeigt werden soll
		_LiquidStartingPoint("Liquid threshold", Range(0, 1)) = 0.7

		// Definiere _HeightMap, _MoistureMap, und _ColorMap, diese können über einen 
		// Input in der GUI zugewiesen werden
		_HeightMap("Height Map", 2D) = "normal" {}
		_MoistureMap("Moisture Map", 2D) = "normal" {}
		_ColorMapLand("Color Map Land", 2D) = "normal" {}
		_ColorMapWater("Color Map Water", 2D) = "normal" {}

		// Definiere Hautfarbe, Reflexion Ambienten Licht, Reflexion Diffusen Licht, 
		// Reflexion Spekular, Glanzgrad
		_Color("Base Color", Color) = (1,1,1,1)
		_Ka("Ambient Reflectance", Range(0, 1)) = 0.5
		_Kd("Diffuse Reflectance", Range(0, 1)) = 0.5
		_Ks("Specular Reflectance", Range(0, 1)) = 0.5
		_Shininess("Shininess", Range(0, 100)) = 100

		_Speed("Speed", Range(0, 5)) = 2
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
				float4 color : COLOR;
				half3 texValMoisture : TEXCOORD4;
				half3 texVal : TEXCOORD5;
				half3 tspace0 : TEXCOORD7;
				half3 tspace1 : TEXCOORD8;
				half3 tspace2 : TEXCOORD9;
			};

			float _MaxDepth;
			float _Ka, _Kd, _Ks, _Shininess;

			sampler2D _NormalMap1;
			sampler2D _NormalMap2;

			float4 _NormalMap1_ST;
			float4 _NormalMap2_ST;

			float _Speed;

			// VERTEX SHADER
			v2f vert(appdata_full vertInput)
			{
				v2f vertOutput;

				// Farben aus der Textur extrahieren
				vertOutput.texVal = tex2Dlod(_HeightMap, float4(vertInput.texcoord.xy, 0, 0));
				vertOutput.texValMoisture = tex2Dlod(_MoistureMap, float4(vertInput.texcoord.xy, 0, 0));

				// Da die Heightmap nur Werte zwischen 0 und 1 besitzt, kann hier darauf geprüft 
				// werden, ob der "Höhenwert" eines Pixels unterhalb unserer Flüssigkeitsschwelle 
				// liegt
				if (vertOutput.texVal.y <= _LiquidStartingPoint) {
					// Hier wird das Displacement angewandt, je Höher der "Höhenwert" des Pixels 
					// ist, desto häher erscheint der vertex auf dem Objekt, 
					// alle Pixel die unter oder auf dem Schwellenwert liegen, erhalten 
					// denselben Wert
					vertInput.vertex.xyz += vertInput.normal * _LiquidStartingPoint * _DisplacementExtension;
					vertOutput.vertex = UnityObjectToClipPos(vertInput.vertex);

					vertOutput.worldNormal = UnityObjectToWorldNormal(vertInput.normal);
					vertOutput.worldViewDir = normalize(WorldSpaceViewDir(vertInput.vertex));
				}
				else {
					// Hier wird das Displacement angewandt, je Höher der "Höhenwert" 
					// des Pixels ist, desto häher erscheint der vertex auf dem Objekt
					// alle vertex die über dem Schwellwert liegen, erhalten einen neuen Höhenwert, 
					// abhängig von dem Höhenwert des Pixels
					vertInput.vertex.xyz += vertInput.normal * _DisplacementExtension * vertOutput.texVal.y;
					vertOutput.vertex = UnityObjectToClipPos(vertInput.vertex);	

					vertOutput.worldNormal = UnityObjectToWorldNormal(vertInput.normal);
				}

				half3 normalMapWorldNormal = UnityObjectToWorldNormal(vertInput.normal);
				half3 normalMapWorldTangent = UnityObjectToWorldDir(vertInput.tangent);
				half3 normalMapWorldBitangent = cross(normalMapWorldNormal, normalMapWorldTangent);

				vertOutput.tspace0 = half3(normalMapWorldTangent.x, normalMapWorldBitangent.x, 
											normalMapWorldNormal.x);
				vertOutput.tspace1 = half3(normalMapWorldTangent.y, normalMapWorldBitangent.y, 
											normalMapWorldNormal.y);
				vertOutput.tspace2 = half3(normalMapWorldTangent.z, normalMapWorldBitangent.z, 
											normalMapWorldNormal.z);

				vertOutput.uv = TRANSFORM_TEX(vertInput.texcoord, _NormalMap1);
				vertOutput.uv += TRANSFORM_TEX(vertInput.texcoord, _NormalMap2);
				
				vertOutput.uv += _Speed * _Time.x;

				return vertOutput;
			}

			// FRAGMENT / PIXEL SHADER
			fixed4 frag(v2f fragInput) : SV_Target
			{
				fixed4 color;

				//Variablen
				half re;
				half nl;
				float4 ambientLight;
				float4 diffuseLight;
				float3 worldSpaceReflection;
				float4 spec;

				if (fragInput.texVal.y <= _LiquidStartingPoint)
				{
					float texValHeight = (_LiquidStartingPoint - fragInput.texVal.y) / (_LiquidStartingPoint);
					fragInput.color = tex2Dlod(_ColorMapWater, float4(fragInput.texValMoisture.y, texValHeight, 0, 0));

					half3 normalizedNormalMaps = normalize(UnpackNormal(tex2D(_NormalMap1, fragInput.uv)) 
									+ UnpackNormal(tex2D(_NormalMap2, fragInput.uv2)));
					half3 normal;
					normal.x = dot(fragInput.tspace0, normalizedNormalMaps);
					normal.y = dot(fragInput.tspace1, normalizedNormalMaps);
					normal.z = dot(fragInput.tspace2, normalizedNormalMaps);

					ambientLight = float4(ShadeSH9(half4(normal,1)),1);
					
					nl = max(0, dot(normal, _WorldSpaceLightPos0.xyz));

					float4 diffuseLight = nl * _LightColor0;

					worldSpaceReflection = 
							reflect(normalize(-_WorldSpaceLightPos0.xyz), normal);
					re = pow(max(dot(worldSpaceReflection, fragInput.worldViewDir), 0), _Shininess);
					spec = re * _LightColor0;
					fragInput.color *= (_Ka* ambientLight +  _Kd* diffuseLight);
					fragInput.color += _Ks * spec;
				}
				else
				{
					float texValHeight = (fragInput.texVal.y - _LiquidStartingPoint) / (1 - _LiquidStartingPoint);
					fragInput.color = tex2Dlod(_ColorMapLand, float4(fragInput.texValMoisture.y, texValHeight, 0, 0));
					
					// Lambert-Shading
					// Schafft einen übergang von hell nach dunkel
					ambientLight = float4(ShadeSH9(half4(fragInput.worldNormal, 1)), 1);
					nl = max(0, dot(fragInput.worldNormal, _WorldSpaceLightPos0.xyz));
					diffuseLight = nl * _LightColor0;

					// Farbe wird mit dem diffusen und ambiente Licht anteilig verrechnet
					fragInput.color *= (_Ka * ambientLight +  _Kd * diffuseLight);
				}
				color = fragInput.color;
				return color;
			}
			ENDCG
		}
	}
}
