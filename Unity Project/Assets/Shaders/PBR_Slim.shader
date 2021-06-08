Shader "JigSpace/PBR_Slim" {

	Properties{

		//---Core Properties---//
		[Header(Core Properties)]
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Geometry"
				"RenderType" = "Opaque"
				"PerformanceChecks" = "False"
			}

			LOD 300
			ZWrite On
			ColorMask RGB

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows addshadow
		#pragma target 3.0

			//---Textures---//
			sampler2D _MainTex;

			//---Colours---//
			fixed4 _Color;

			//---UVs---//
			struct Input
			{
				float2 uv_MainTex;
			};

			//---Surf---///
			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				//---Colour Calulations---//
				half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				
				//---Outputs---//
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}
			ENDCG

		}
			FallBack "Diffuse"
}
