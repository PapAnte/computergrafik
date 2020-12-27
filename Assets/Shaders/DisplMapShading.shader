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
				half3 matrixSpaceX : TEXCOORD7;
				half3 matrixSpaceY : TEXCOORD8;
				half3 matrixSpaceZ : TEXCOORD9;
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
				vertOutput.texValMoisture = tex2Dlod(_MoistureMap, 
						float4(vertInput.texcoord.xy, 0, 0));

				// Da die Heightmap nur Werte zwischen 0 und 1 besitzt, kann hier darauf geprüft 
				// werden, ob der "Höhenwert" eines Pixels unterhalb unserer Flüssigkeitsschwelle 
				// liegt
				if (vertOutput.texVal.y <= _LiquidStartingPoint) {
					// Hier wird das Displacement angewandt, je Höher der "Höhenwert" des Pixels 
					// ist, desto höher erscheint der vertex auf dem Objekt, 
					// alle Pixel die unter oder auf dem Schwellenwert liegen, erhalten 
					// denselben Wert
					vertInput.vertex.xyz += vertInput.normal * 
							_LiquidStartingPoint * _DisplacementExtension;
					// Start der Berechnungen für den Phong-Algorithmus, da die Pixel unterhalb
					// der Flüssigkeitsschwelle liegen
					// Vertices von Objekt-Koordinaten in Clip-Koordinaten transformiern
					vertOutput.vertex = UnityObjectToClipPos(vertInput.vertex);
					// Normalen-Vektoren transformieren in Welt-Koordinaten
					vertOutput.worldNormal = UnityObjectToWorldNormal(vertInput.normal);
					// Blickrichtung in Welt-Koordinaten berechnen
					vertOutput.worldViewDir = normalize(WorldSpaceViewDir(vertInput.vertex));
				}
				else {
					// Hier wird das Displacement angewandt, je Höher der "Höhenwert" 
					// des Pixels ist, desto häher erscheint der vertex auf dem Objekt
					// alle vertex die über dem Schwellwert liegen, erhalten einen neuen Höhenwert, 
					// abhängig von dem Höhenwert des Pixels
					vertInput.vertex.xyz += vertInput.normal * 
							_DisplacementExtension * vertOutput.texVal.y;
					// Start der Berechnung für den Lambert-Algorithmus, da die Pixel überhalb
					// der Flüssigkeitsschwelle liegen
					// Vertices von Objekt-Koordinaten in Clip-Koordinaten transformiern
					vertOutput.vertex = UnityObjectToClipPos(vertInput.vertex);	
					// Normalen-Vektoren transformieren in Welt-Koordinaten
					vertOutput.worldNormal = UnityObjectToWorldNormal(vertInput.normal);
				}

				// Normale, Tangente und Bitangente dieser Szene bestimmen
				half3 normalMapWorldNormal = UnityObjectToWorldNormal(vertInput.normal);
				half3 normalMapWorldTangent = UnityObjectToWorldDir(vertInput.tangent);
				half3 normalMapWorldBitangent = cross(normalMapWorldNormal, 
						normalMapWorldTangent);

				// Raum Matrix bestimmen mithilfe der Normalen, Tangenten und 
				// Bitangenten dieser Szene
				vertOutput.matrixSpaceX = half3(normalMapWorldTangent.x, normalMapWorldBitangent.x,
						normalMapWorldNormal.x);
				vertOutput.matrixSpaceY = half3(normalMapWorldTangent.y, normalMapWorldBitangent.y,
						normalMapWorldNormal.y);
				vertOutput.matrixSpaceZ = half3(normalMapWorldTangent.z, normalMapWorldBitangent.z,
						normalMapWorldNormal.z);

				// Pixel Koordinaten mittels der NormalMaps bestimmen
				vertOutput.uv = TRANSFORM_TEX(vertInput.texcoord, _NormalMap1);
				vertOutput.uv += TRANSFORM_TEX(vertInput.texcoord, _NormalMap2);
				
				// Bewegung der NormalMaps wird mittels der Zeit und dem variablen Speed bestimmt
				vertOutput.uv += _Speed * _Time.x;

				return vertOutput;
			}

			// FRAGMENT / PIXEL SHADER
			fixed4 frag(v2f fragInput) : SV_Target
			{
				fixed4 color;

				// Variablen deklarieren
				half reflection;
				half diffuse;
				float4 ambientLight;
				float4 diffuseLight;
				float3 worldSpaceReflection;
				float4 spec;

				if (fragInput.texVal.y <= _LiquidStartingPoint)
				{	
					// Hier muss der Wert der Heightmap,
					// der später als y-Wert auf der ColorMap genutzt wird,
					// angepasst werden, da wir 2 Colormaps nutzen
					float texValHeight = (_LiquidStartingPoint - fragInput.texVal.y) / 
							(_LiquidStartingPoint);
					// Hier wird sich die Farbe aus der ColorMap geholt
					// an der Stelle x (Wert der MoistureMap)
					// und der Stelle y (Wert der HeightMap)
					fragInput.color = tex2Dlod(_ColorMapWater, float4(fragInput.texValMoisture.y,
							texValHeight, 0, 0));

					// Beide NormalMaps zusammenfügen und Normalisieren
					half3 normalizedNormalMaps = 
							normalize(UnpackNormal(tex2D(_NormalMap1, fragInput.uv))
							+ UnpackNormal(tex2D(_NormalMap2, fragInput.uv2)));
					half3 normal;

					// Normalenvektor mithilfe der NormalMaps bestimmen
					normal.x = dot(fragInput.matrixSpaceX, normalizedNormalMaps);
					normal.y = dot(fragInput.matrixSpaceY, normalizedNormalMaps);
					normal.z = dot(fragInput.matrixSpaceZ, normalizedNormalMaps);

					// Durchführung der Berechnungen für den Phong-Algorithmus,
					// bekannt aus der Übung 5.1 - Lambert und Phong Beleuchtung
					// Berechnung der ambienten Licht Farbe
					ambientLight = float4(ShadeSH9(half4(normal,1)),1);

					// Standard Diffuse zwischen dem Normalen-Vektor 'normal' und
					// der Richtung der Beleutchtungsquelle '_WorldSpaceLightPos0'
					diffuse = max(0, dot(normal, _WorldSpaceLightPos0.xyz));

					// Verrechnung des Diffusen Licht 'diffuse' mit der Lichtfarbe '_LightColor0'
					float4 diffuseLight = diffuse * _LightColor0;
					worldSpaceReflection = 
							reflect(normalize(-_WorldSpaceLightPos0.xyz), normal);
					reflection = pow(max(dot(worldSpaceReflection, 
							fragInput.worldViewDir), 0), _Shininess);
					spec = reflection * _LightColor0;

					// Farbe wird mit dem diffusen und ambiente Licht anteilig verrechnet
					fragInput.color *= (_Ka* ambientLight +  _Kd* diffuseLight);
					fragInput.color += _Ks * spec;
				}
				else
				{
					// Hier muss der Wert der Heightmap,
					// der später als y-Wert auf der ColorMap genutzt wird,
					// angepasst werden, da wir 2 Colormaps nutzen
					float texValHeight = (fragInput.texVal.y - _LiquidStartingPoint) / 
							(1 - _LiquidStartingPoint);

					// Hier wird sich die Farbe aus der ColorMap geholt
					// an der Stelle x (Wert der MoistureMap)
					// und der Stelle y (Wert der HeightMap)
					fragInput.color = tex2Dlod(_ColorMapLand, float4(fragInput.texValMoisture.y,
							texValHeight, 0, 0));
					
					// Durchführung der Berechnungen für den Lambert-Algorithmus,
					// bekannt aus der Übung 5.1 - Lambert und Phong Beleuchtung
					// Berechnung der ambienten Licht Farbe
					ambientLight = float4(ShadeSH9(half4(fragInput.worldNormal, 1)), 1);

					// Standard Diffuse zwischen dem Normalen-Vektor 'normal' und
					// der Richtung der Beleutchtungsquelle '_WorldSpaceLightPos0'
					diffuse = max(0, dot(fragInput.worldNormal, _WorldSpaceLightPos0.xyz));

					// Verrechnung des Diffusen Licht 'diffuse' mit der Lichtfarbe '_LightColor0'
					diffuseLight = diffuse * _LightColor0;

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
