


Shader "VRShaders/TextureDissolve" {
	Properties{
		_MainTex("Texture ", 2D) = "white"{}
		_NormalMap("Normal", 2D) = "bump" {}
		_StainTex("Stain (RGB)", 2D) = "white" {}
		_SecondStainTex("Second Stain (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Dissolve("Dissolve", Range(0.0, 1.0)) = 0.5



	}
		SubShader{
		   Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		  Cull Off
		  CGPROGRAM
		  #pragma surface surf Lambert  
		  struct Input {

			  float2 uv_MainTex;
			  float2 uv_StainTex;
			  float2 uv_SecondStainTex;
			  float2 _NormalMap;

		  };

	

			sampler2D _MainTex;
			sampler2D _NormalMap;
			sampler2D _StainTex;
			sampler2D _SecondStainTex;
			fixed4 _Color;
			float _Dissolve;

		  void surf(Input IN, inout SurfaceOutput o) {

			  float3 main = tex2D(_MainTex, IN.uv_MainTex).rgb;


			  float3 stain = tex2D(_StainTex, IN.uv_StainTex).rgb;
			  float3 stain2 = tex2D(_SecondStainTex, IN.uv_SecondStainTex).rgb;

			  stain = lerp(stain, 1, _Dissolve);
			  stain2 = lerp(stain2, 1, _Dissolve);


			  float3 Out = main * stain*stain2;

			  float3 normal= UnpackNormal(tex2D(_NormalMap, IN._NormalMap));
			  o.Normal = normalize(normal);

			  o.Albedo = Out * _Color;


		  }
		  ENDCG
		}
			Fallback "Bumped Diffuse"
}