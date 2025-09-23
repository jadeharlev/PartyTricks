// Volumetric Clouds 3D Shader (With Peak Shape Control)
// Added a "_PeakShape" property to control the hardness/softness of the cloud peaks.
// Values < 1 create soft, rounded peaks. Values > 1 create sharp peaks.

Shader "World/VolumetricClouds3D_Final"
{
    Properties
    {
        [Header(Cloud Colors)]
        _LightColor ("Light Color (Near)", Color) = (0.505, 0.505, 0.823, 1.0)
        _DarkColor ("Dark Color (Far)", Color) = (0.2, 0.2, 0.35, 1.0)

        [Header(Cloud Shape and Scale)]
        _DisplacementIntensity ("Displacement Intensity", Range(0.0, 1000.0)) = 10.0
        
        // --- NEW PROPERTY ADDED HERE ---
//        [Tooltip("Controls the shape of the peaks")]
        _PeakShape ("Peak Shape", Range(0.1, 4.0)) = 1.0 // 1 is neutral, < 1 is soft

        _NoiseScale ("Cloud Scale", Range(1.0, 50.0)) = 10.0
        _Octaves("Detail Octaves", Range(1, 8)) = 6

        [Header(Animation)]
        _ScrollSpeedX ("Scroll Speed X", Range(-2.0, 2.0)) = 0.1
        _ScrollSpeedY ("Scroll Speed Y", Range(-2.0, 2.0)) = 0.05
        _EvolutionSpeed ("Cloud Evolution Speed", Range(0.0, 1.0)) = 0.2
        _ModulationAmount ("Peak Animation Amount", Range(-1.0, 1.0)) = 0.3 // Changed range to allow +/-
        _ModulationSpeed ("Peak Animation Speed", Range(0.0, 5.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            // Properties
            fixed4 _LightColor; fixed4 _DarkColor; float _DisplacementIntensity;
            float _PeakShape; // --- NEW VARIABLE DECLARED ---
            float _NoiseScale; int _Octaves; float _ScrollSpeedX; float _ScrollSpeedY;
            float _EvolutionSpeed; float _ModulationAmount; float _ModulationSpeed;

            // --- Noise Library (unchanged) ---
            float3 mod289(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float4 mod289(float4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float4 permute(float4 x) { return mod289(((x*34.0)+1.0)*x); }
            float4 taylorInvSqrt(float4 r) { return 1.79284291400159 - 0.85373472095314 * r; }
            float snoise(float3 v){const float2 C=float2(1.0/6.0,1.0/3.0);const float4 D=float4(0.0,0.5,1.0,2.0);float3 i=floor(v+dot(v,C.yyy));float3 x0=v-i+dot(i,C.xxx);float3 g=step(x0.yzx,x0.xyz);float3 l=1.0-g;float3 i1=min(g.xyz,l.zxy);float3 i2=max(g.xyz,l.zxy);float3 x1=x0-i1+C.xxx;float3 x2=x0-i2+C.yyy;float3 x3=x0-D.yyy;i=mod289(i);float4 p=permute(permute(permute(i.z+float4(0.0,i1.z,i2.z,1.0))+i.y+float4(0.0,i1.y,i2.y,1.0))+i.x+float4(0.0,i1.x,i2.x,1.0));float n_=0.142857142857;float3 ns=n_*D.wyz-D.xzx;float4 j=p-49.0*floor(p*ns.z*ns.z);float4 x_=floor(j*ns.z);float4 y_=floor(j-7.0*x_);float4 x=x_*ns.x+ns.yyyy;float4 y=y_*ns.x+ns.yyyy;float4 h=1.0-abs(x)-abs(y);float4 b0=float4(x.xy,y.xy);float4 b1=float4(x.zw,y.zw);float4 s0=floor(b0)*2.0+1.0;float4 s1=floor(b1)*2.0+1.0;float4 sh=-step(h,float4(0,0,0,0));float4 a0=b0.xzyw+s0.xzyw*sh.xxyy;float4 a1=b1.xzyw+s1.xzyw*sh.zzww;float3 p0=float3(a0.xy,h.x);float3 p1=float3(a0.zw,h.y);float3 p2=float3(a1.xy,h.z);float3 p3=float3(a1.zw,h.w);float4 norm=taylorInvSqrt(float4(dot(p0,p0),dot(p1,p1),dot(p2,p2),dot(p3,p3)));p0*=norm.x;p1*=norm.y;p2*=norm.z;p3*=norm.w;float4 m=max(0.6-float4(dot(x0,x0),dot(x1,x1),dot(x2,x2),dot(x3,x3)),0.0);m=m*m;return 42.0*dot(m*m,float4(dot(p0,x0),dot(p1,x1),dot(p2,x2),dot(p3,x3)));}
            float fbm(float3 p){float value=0.0;float amplitude=0.5;float frequency=1.0;for(int i=0;i<_Octaves;i++){value+=amplitude*snoise(p*frequency);amplitude*=0.5;frequency*=2.0;}return value;}

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 vertex : SV_POSITION; float height : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                float2 positionInput = v.vertex.xz; 
                float2 scrollVec = float2(_ScrollSpeedX, _ScrollSpeedY);
                float3 noiseCoords = float3(
                    (positionInput.x / _NoiseScale) + _Time.y * scrollVec.x,
                    (positionInput.y / _NoiseScale) + _Time.y * scrollVec.y,
                    _Time.y * _EvolutionSpeed
                );
                
                float noiseValue = fbm(noiseCoords);
                float height = saturate((noiseValue + 1.0) * 0.5);

                // --- LOGIC CHANGED HERE ---
                // We now use a sine wave from -1 to 1 for a smoother pulse.
                float modulation = sin(_Time.y * _ModulationSpeed); // Now goes from -1 to 1

                // The final exponent is our base shape plus the animated offset.
                float finalExponent = _PeakShape + modulation * _ModulationAmount;

                // Apply the pow() function with our new controllable exponent.
                // We use max(0.01, ...) to prevent the exponent from becoming zero or negative.
                height = pow(height, max(0.01, finalExponent));
                o.height = height;

                float3 displacedPosition = v.vertex.xyz + v.normal * height * _DisplacementIntensity;
                o.vertex = UnityObjectToClipPos(displacedPosition);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return lerp(_DarkColor, _LightColor, i.height);
            }
            ENDHLSL
        }
    }
}