Shader "Custom/Water" {
	Properties {
		_ScaleFactor ("Scale Factor", float) = 0
		
		_FGWavesSpeed ("Front Waves Speed", float) = 0
		_FGWavesColor ("Front Waves Color", Color) = (0,0,0,0)
		_BGWavesSpeed ("Back Waves Speed", float) = 0
		_BGWavesColor ("Back Waves Color", Color) = (0,0,0,0)
		
		_BGColor ("Background Color", Color) = (0,0,0,0)
		_WaveTex ("Background Texture", 2D) = "black" {}
	}
	SubShader {
		Tags { "Queue" = "Transparent" }
		Pass {
			ZWrite Off

			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM 

			#pragma vertex vert 
			#pragma fragment frag

			float _ScaleFactor;

			float _FGWavesSpeed;
			float4 _FGWavesColor;
			float _BGWavesSpeed;
			float4 _BGWavesColor;
			
			float4 _BGColor;
			sampler2D _WaveTex;
			
			struct vertexInput {
				float4 pos : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			
			struct fragInput {
				float4 svpos : SV_POSITION;
				float4 texcoord : TEXCOORD0;
			};

			fragInput vert(vertexInput i) 
			{
				fragInput f;
				f.svpos = mul(UNITY_MATRIX_MVP, i.pos);
				f.texcoord = mul(_Object2World, i.pos);
				return f;
			}

			float4 frag(fragInput f) : COLOR 
			{
				float2 fgOffset = float2 (0.0, _Time.y * _FGWavesSpeed);
				float2 bgOffset = float2 (0.0, _Time.y * _BGWavesSpeed);
				float4 fgWaves = tex2D(_WaveTex,f.texcoord.zx * _ScaleFactor + fgOffset);
				float4 bgWaves = tex2D(_WaveTex,f.texcoord.xz * _ScaleFactor + bgOffset);
				float3 lerped = lerp(_BGColor.rgb,_BGWavesColor.rgb,bgWaves.a * _BGWavesColor.a);
				lerped = lerp(lerped,_FGWavesColor.rgb,fgWaves.a * _FGWavesColor.a);
				return float4 (lerped, _BGColor.a); 
			}

			ENDCG  
		}
	}
}
