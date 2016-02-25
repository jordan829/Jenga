////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// Renders an object with diffuse and emissive color terms.
Shader "zSpace/Diffuse-Emissive" {
   Properties {
      _Color ("Main Color", Color) = (1,1,1,1)
      _MainTex ("Texture", 2D) = "white" {}
       _EmissiveMap ("EmissiveMap (RGB)", 2D) = "black" {}
   }
   SubShader {

      Tags { "RenderType" = "Opaque" }
      
      CGPROGRAM
      #pragma surface surf Lambert

      struct Input {
         float2 uv_MainTex;
         float3 viewDir;
      };

      sampler2D _MainTex;
      sampler2D _EmissiveMap;
      fixed4 _Color;

      void surf (Input IN, inout SurfaceOutput o) {
         half4 tex = tex2D(_MainTex, IN.uv_MainTex);
          o.Albedo = tex.rgb * _Color.rgb;
         o.Emission = tex2D(_EmissiveMap, IN.uv_MainTex).rgb;
      }
      ENDCG
   }
   
   Fallback "Specular"
}