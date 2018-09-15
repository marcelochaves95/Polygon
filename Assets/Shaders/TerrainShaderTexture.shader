Shader "Custom/TerrainShaderTexture" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Grass Texture", 2D) = "white" {}
		_SandTex ("Sand Texture", 2D) = "white" {}
		_SnowTex ("Snow Texture", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _SandTex;
		sampler2D _SnowTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_SandTex;
			float2 uv_SnowTex;
			float4 color : COLOR;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float4 grass = tex2D(_MainTex, IN.uv_MainTex);
			float4 sand = tex2D(_SandTex, IN.uv_SandTex);
			float4 snow = tex2D(_SnowTex, IN.uv_SnowTex);
			float4 resultColor = (IN.color.r * grass) + (IN.color.g * sand) + (IN.color.b * snow);
			o.Albedo = clamp(resultColor.rgb, 0.0, 1.0) * _Color;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}