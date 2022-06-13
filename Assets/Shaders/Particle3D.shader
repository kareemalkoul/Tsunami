

Shader "Custom/SPH3D" {
	Properties {
		_MainTex("Texture",         2D) = "black" {}
		_ParticleRadius("Particle Radius", Float) = 0.05
		_WaterColor("WaterColor", Color) = (1, 1, 1, 1)
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _WaterColor;
	uniform float iTime;
	uniform float3 iResolution;

	float  _ParticleRadius;
	float4x4 _InvViewMatrix;

	struct v2g {
		float4 pos   : SV_POSITION;
		float4 color : COLOR;
	};

	struct g2f {
		float4 pos   : SV_POSITION;
		float2 tex   : TEXCOORD0;
		float4 color : COLOR;
	};

	struct FluidParticle {
		float3 position;
		float3 velocity;
	};

	StructuredBuffer<FluidParticle> _ParticlesBuffer;

	float2 fract(float2 num){
		return num - floor(num);
	}

	float2 random(float2 block_index){
    float2 p = float2(block_index);
    float2 ret = fract(sin(float2(dot(p,float2(123.1,311.7)),dot(p,float2(269.5,183.3))))*4358.5453);
    
    return ret;
    //return vec2(0., 0.);
}

	float4 mix(float4 v1, float4 v2, float a) { return v1 * (1 - a) + v2 * a;}

	float2 hash2(float2 p ) {
   			return fract(sin(float2(dot(p, float2(123.4, 748.6)), dot(p, float2(547.3, 659.3))))*5232.85324);   
    	}
	float hash(float2 p) {
  		return fract(sin(dot(p, float2(43.232, 75.876)))*4526.3257);   
	}

	float voronoi(float2 p) {
    float2 n = floor(p);
    float2 f = fract(p);
    float md = 5.0;

    float2 m = float2(0.0, 0.0);

    for (int i = -1;i<=1;i++) {
        for (int j = -1;j<=1;j++) {
            float2 g = float2(i, j);
            float o = hash(n+g);
            o = 0.5+0.5*sin(unity_DeltaTime+5.038*o);
            float2 r = g + o - f;
            float d = dot(r, r);
            if (d<md) {
              md = d;
              m = n+g+o;
            }
        }
    }
    return md;
}

	float ov(float2 p) {
    float v = 0.0;
    float a = 0.4;
    for (int i = 0;i<3;i++) {
        v+= voronoi(p)*a;
        p*=2.0;
        a*=0.5;
    }
    return v;
}
	// --------------------------------------------------------------------
	// Vertex Shader
	// --------------------------------------------------------------------
	v2g vert(uint id : SV_VertexID) {

		v2g o = (v2g)0;
		o.pos = float4(_ParticlesBuffer[id].position.xyz, 1);
		o.color = float4(0, 0.1, 0.1, 1);
		return o;
	}

	// --------------------------------------------------------------------
	// Geometry Shader
	// --------------------------------------------------------------------

	[maxvertexcount(4)]
	void geom(point v2g IN[1], inout TriangleStream<g2f> triStream) {

		float size = _ParticleRadius * 2;
		float halfS = _ParticleRadius;

		g2f pIn = (g2f)0;

		for (int x = 0; x < 2; x++) {
			for (int y = 0; y < 2; y++) {
				float4x4 billboardMatrix = UNITY_MATRIX_V;
				billboardMatrix._m03 = billboardMatrix._m13 = billboardMatrix._m23 = billboardMatrix._m33 = 0;

				float2 uv = float2(x, y);

				pIn.pos = IN[0].pos + mul(float4((uv * 2 - float2(1, 1)) * halfS, 0, 1), billboardMatrix);

				pIn.pos = mul(UNITY_MATRIX_VP, pIn.pos);

				float4 a = float4(0.2, 0.4, 1.0, 1.0);
                float4 b = float4(0.85, 0.9, 1.0, 1.0);

				pIn.color = float4(mix(a, b, smoothstep(0.0, 0.5, ov(uv*5.0))));
				// _WaterColor = float4(mix(a, b, smoothstep(0.0, 0.5, ov(uv*5.0))));

				pIn.color = IN[0].color;
				pIn.tex = uv;
				

				triStream.Append(pIn);
			}
		}
		triStream.RestartStrip();

	}

	// --------------------------------------------------------------------
	// Fragment Shader
	// --------------------------------------------------------------------
	fixed4 frag(g2f input) : SV_Target {
		
		float2 uv = float2(input.pos.x, input.pos.y)/float2(2560,1600);
		// log(input.pos.x);
		float4 a = float4(0.2, 0.4, 1.0, 1.0);
        float4 b = float4(0.85, 0.9, 1.0, 1.0);

		_WaterColor = _WaterColor* float4(mix(a, b, smoothstep(0.0, 0.5, ov(uv*5.0))));
		return tex2D(_MainTex, input.tex)*_WaterColor;
	}

	ENDCG

	SubShader {
		Tags{ "RenderType" = "Transparent"  }
		LOD 300

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vert
			#pragma geometry geom
			#pragma fragment frag
			ENDCG
		}
	}
}