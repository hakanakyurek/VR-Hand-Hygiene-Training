


Shader "VRShaders/TextureDissolve" {
	Properties{
		//_MainTex("Color (RGB) ", 2D) = "white"{}
		_Color("Color", Color) = (1,1,1,1)
		_BumpMap("Bumpmap", 2D) = "bump" {}
		
		_StainTex("Stain (RGB)", 2D) = "white" {}
		
		_Fuzziness("_Fuzziness", Range(-5.0, 10.0)) = 0.5
		_Range("_Range", Range(0.0, 2.0)) = 0.5
		_DissolveMult("_DissolveSpeed", Range(0.0, 1.0)) =1
	}
		SubShader{
		   Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		  Cull Off
		  CGPROGRAM
		  #pragma surface surf Lambert  
		  struct Input {

			 // float2 uv_MainTex;
			  float2 uv_StainTex;
			  float2 uv_BumpMap;
		
		  };


			//sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _StainTex;
			fixed4 _Color;
			float _Fuzziness;
			float _Range;
			float _DissolveMult;

		  void surf(Input IN, inout SurfaceOutput o) {
			 
			  float3 b = tex2D(_StainTex, IN.uv_StainTex).rgb;
			  float Distance = distance(_Color, b);
			  float3 Out = lerp(_Color, b*_Color , saturate((Distance - _Range* _DissolveMult) * max(_Fuzziness, 1e-5f)));

			 
			  o.Albedo =  Out;
			  o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			
		  }
		  ENDCG
	  }
		  Fallback "Diffuse"
}