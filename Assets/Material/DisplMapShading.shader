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
		_ColorMap("Color Map", 2D) = "normal" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _HeightMap;
			float4 _HeightMap_ST;
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

				// Farben aus der Textur extrahieren --> Aus Übung 3.3 #Es gibt keine Tutorials dafür
				fixed4 texVal = tex2Dlod(_HeightMap, float4(v.texcoord.xy, 0, 0));

				// Da die Heightmap nur Werte zwischen 0 und 1 besitzt, kann hier darauf geprüft werden, ob der "Höhenwert" eines Pixels unterhalb unserer Flüssigkeitsschwelle liegt
				if (texVal.y <= _LiquidStartingPoint) {

					// Hier wird das Displacement angewandt, je Höher der "Höhenwert" des Pixels ist, desto häher erscheint der vertex auf dem Objekt, 
					// alle Pixel die unter oder auf dem Schwellenwert liegen, erhalten denselben Wert
					v.vertex.xyz += v.normal * _LiquidStartingPoint * _DisplacementExtension;
				}
				else {

					// Hier wird das Displacement angewandt, je Höher der "Höhenwert" des Pixels ist, desto häher erscheint der vertex auf dem Objekt
					// alle vertex die über dem Schwellwert liegen, erhalten einen neuen Höhenwert, abhängig von dem Höhenwert des Pixels
					v.vertex.xyz += v.normal * _DisplacementExtension * texVal.y;
				}

				// Vertices umwandeln mit der typischen Unity Funktion
				o.vertex = UnityObjectToClipPos(v.vertex);

				// Farbe des Objekts soll der der Map gleichen
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
