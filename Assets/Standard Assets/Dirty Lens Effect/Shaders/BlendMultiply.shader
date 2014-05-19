Shader "Hidden/BlendModesOverlay" {
	Properties {
		_MainTex ("Screen Blended", 2D) = "" {}
		_Mask ("Mask", 2D) = "" {}
		_Overlay1 ("Color", 2D) = "grey" {}
		_Color ("Main Color, Alpha", Color) = (1,1,1,1)
		_Vignette ("Vignette", 2D) = "" {}
		_Chrom("Chrom", 2D) = "" {}
	}
	
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv[2] : TEXCOORD0;
	};
	
	sampler2D _MainTex;		
	sampler2D _Overlay1;
	sampler2D _Mask;
	sampler2D _Vignette;
	sampler2D _Chrom;
	float4 _Color;
	half _Intensity1;
	half4 _MainTex_TexelSize;
		
	v2f vert( appdata_img v ) { 
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv[0] =  v.texcoord.xy;
		
		#if UNITY_UV_STARTS_AT_TOP
		if(_MainTex_TexelSize.y<0.0)
			o.uv[0].y = 1.0-o.uv[0].y;
		#endif
		
		o.uv[1] =  v.texcoord.xy ;	
		o.uv[0].xy = 1.0 - o.uv[0].xy;
		
		return o;
	}
	

	half4 fragMultiply (v2f i) : COLOR {
		half2 coords = (i.uv[1]-0.5)*3.3;		
		half coordDot = dot (coords,coords);
		
		half4 mainTex = tex2D(_MainTex, i.uv[1]);
		
		half4 mask = tex2D(_Mask, i.uv[1]) * 0.7;
		half4 maskFlipped = tex2D(_Mask, i.uv[0]);
		
		half4 toBlend1 = maskFlipped * tex2D(_Overlay1, i.uv[1]) * _Intensity1;
		half4 glare1 = tex2D(_Vignette, i.uv[1] + (2 - coordDot) * 1.5) * 15 * _Color;
		
		
		return mainTex + mask + toBlend1 + toBlend1 * glare1;
	}	
			


	


	ENDCG 
	
Subshader {
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }  
      ColorMask RGB  
  		  	
 Pass {    

      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma vertex vert
      #pragma fragment fragMultiply
      ENDCG
  }  
}

Fallback off
	
} // shader