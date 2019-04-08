


Shader "VRShaders/TextureDissolve" {
	Properties{
		_MainTex("Texture ", 2D) = "white"{}
		_NormalMap("Normal", 2D) = "bump" {}
		_StainTexture("Stain (RGB)", 2D) = "white" {}
		_SoapTexture("Soap Texture (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Dissolve("Dissolve", Range(0.0, 1.0)) = 0.5
		_SoapValue("Soap Value", Range(0.0, 1.0)) = 0.5


	}
		SubShader{
		   Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		  Cull Off
		  CGPROGRAM
		  #pragma surface surf Lambert  
		  struct Input {

			  float2 uv_MainTex;
			  float2 uv_StainTexture;
			  float2 uv_SoapTexture;
			  float2 uv_NormalMap;

		  };

	

			sampler2D _MainTex;
			sampler2D _NormalMap;
			sampler2D _StainTexture;
			sampler2D _SoapTexture;
			fixed4 _Color;
			float _Dissolve;
			float _SoapValue;
		  void surf(Input IN, inout SurfaceOutput o) {

			  float3 main = tex2D(_MainTex, IN.uv_MainTex).rgb;


			  float3 stain = tex2D(_StainTexture, IN.uv_StainTexture).rgb;
			  float3 soap = tex2D(_SoapTexture, IN.uv_SoapTexture).rgb;

			  stain = lerp(stain, 1, _Dissolve);
			  soap = lerp(soap, 1, _SoapValue);

			  float3 Out = main * stain*soap;
			 
			  float3 normal= UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
			  o.Normal = normalize(normal);

			  o.Albedo = Out * _Color;


		  }
		  ENDCG
		}
			Fallback "Diffuse"
}