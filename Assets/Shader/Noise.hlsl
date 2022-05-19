﻿
float rand1(float2 co)
{
    float a = 12.9898;
    float b = 78.233;
    float c = 43758.5453;
    float dt = dot(co.xy, float2(a, b));
    float sn = dt % 3.14;
    return frac(sin(sn) * c);
}

inline       float ValueNoise(float3 pos)
{
    float3 Noise_skew = pos + 0.2127 + pos.x * pos.y * pos.z * 0.3713;
    float3 Noise_rnd = 4.789 * sin(489.123 * (Noise_skew));
    return frac(Noise_rnd.x * Noise_rnd.y * Noise_rnd.z * (1.0 + Noise_skew.x));
}

float2 hash22(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
    return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}
			
float2 hash21(float2 p)
{
    float h = dot(p, float2(127.1, 311.7));
    return -1.0 + 2.0 * frac(sin(h) * 43758.5453123);
}

			//perlin
float perlin_noise(float2 p)
{
    float2 pi = floor(p);
    float2 pf = p - pi;
    float2 w = pf * pf * (3.0 - 2.0 * pf);
    return lerp(lerp(dot(hash22(pi + float2(0.0, 0.0)), pf - float2(0.0, 0.0)),
					dot(hash22(pi + float2(1.0, 0.0)), pf - float2(1.0, 0.0)), w.x),
					lerp(dot(hash22(pi + float2(0.0, 1.0)), pf - float2(0.0, 1.0)),
						dot(hash22(pi + float2(1.0, 1.0)), pf - float2(1.0, 1.0)), w.x), w.y);
}

			//value
float value_noise(float2 p)
{
    float2 pi = floor(p);
    float2 pf = p - pi;
    float2 w = pf * pf * (3.0 - 2.0 * pf);
    return lerp(lerp(hash21(pi + float2(0.0, 0.0)), hash21(pi + float2(1.0, 0.0)), w.x),
					lerp(hash21(pi + float2(0.0, 1.0)), hash21(pi + float2(1.0, 1.0)), w.x), w.y);
}

			//simplex
float simplex_noise(float2 p)
{
    float k1 = 0.366025404;
    float k2 = 0.211324865;
    float2 i = floor(p + (p.x + p.y) * k1);
    float2 a = p - (i - (i.x + i.y) * k2);
    float2 o = (a.x < a.y) ? float2(0.0, 1.0) : float2(1.0, 0.0);
    float2 b = a - o + k2;
    float2 c = a - 1.0 + 2.0 * k2;
    float3 h = max(0.5 - float3(dot(a, a), dot(b, b), dot(c, c)), 0.0);
    float3 n = h * h * h * h * float3(dot(a, hash22(i)), dot(b, hash22(i + o)), dot(c, hash22(i + 1.0)));
    return dot(float3(70.0, 70.0, 70.0), n);
}

			//fbm分形叠加
float noise_sum(float2 p)
{
    float f = 0.0;
    p = p * 4.0;
    f += 1.0 * perlin_noise(p);
    p = 2.0 * p;
    f += 0.5 * perlin_noise(p);
    p = 2.0 * p;
    f += 0.25 * perlin_noise(p);
    p = 2.0 * p;
    f += 0.125 * perlin_noise(p);
    p = 2.0 * p;
    f += 0.0625 * perlin_noise(p);
    return f;
}

float noise_sum_value(float2 p)
{
    float f = 0.0;
    p = p * 4.0;
    f += 1.0 * value_noise(p);
    p = 2.0 * p;
    f += 0.5 * value_noise(p);
    p = 2.0 * p;
    f += 0.25 * value_noise(p);
    p = 2.0 * p;
    f += 0.125 * value_noise(p);
    p = 2.0 * p;
    f += 0.0625 * value_noise(p);
    return f;
}

float noise_sum_simplex(float2 p)
{
    float f = 0.0;
    p = p * 4.0;
    f += 1.0 * simplex_noise(p);
    p = 2.0 * p;
    f += 0.5 * simplex_noise(p);
    p = 2.0 * p;
    f += 0.25 * simplex_noise(p);
    p = 2.0 * p;
    f += 0.125 * simplex_noise(p);
    p = 2.0 * p;
    f += 0.0625 * simplex_noise(p);
    return f;
}

			//turbulence
float noise_sum_abs(float2 p)
{
    float f = 0.0;
    p = p * 7.0;
    f += 1.0 * abs(perlin_noise(p));
    p = 2.0 * p;
    f += 0.5 * abs(perlin_noise(p));
    p = 2.0 * p;
    f += 0.25 * abs(perlin_noise(p));
    p = 2.0 * p;
    f += 0.125 * abs(perlin_noise(p));
    p = 2.0 * p;
    f += 0.0625 * abs(perlin_noise(p));
    return f;
}

float noise_sum_abs_value(float2 p)
{
    float f = 0.0;
    p = p * 7.0;
    f += 1.0 * abs(value_noise(p));
    p = 2.0 * p;
    f += 0.5 * abs(value_noise(p));
    p = 2.0 * p;
    f += 0.25 * abs(value_noise(p));
    p = 2.0 * p;
    f += 0.125 * abs(value_noise(p));
    p = 2.0 * p;
    f += 0.0625 * abs(value_noise(p));
    return f;
}

float noise_sum_abs_simplex(float2 p)
{
    float f = 0.0;
    p = p * 7.0;
    f += 1.0 * abs(simplex_noise(p));
    p = 2.0 * p;
    f += 0.5 * abs(simplex_noise(p));
    p = 2.0 * p;
    f += 0.25 * abs(simplex_noise(p));
    p = 2.0 * p;
    f += 0.125 * abs(simplex_noise(p));
    p = 2.0 * p;
    f += 0.0625 * abs(simplex_noise(p));
    return f;
}

			//turbulence_sin
float noise_sum_abs_sin(float2 p)
{
    float f = 0.0;
    p = p * 16.0;
    f += 1.0 * abs(perlin_noise(p));
    p = 2.0 * p;
    f += 0.5 * abs(perlin_noise(p));
    p = 2.0 * p;
    f += 0.25 * abs(perlin_noise(p));
    p = 2.0 * p;
    f += 0.125 * abs(perlin_noise(p));
    p = 2.0 * p;
    f += 0.0625 * abs(perlin_noise(p));
    p = 2.0 * p;
    f = sin(f + p.x / 32.0);
    return f;
}

float noise_sum_abs_sin_value(float2 p)
{
    float f = 0.0;
    p = p * 16.0;
    f += 1.0 * abs(value_noise(p));
    p = 2.0 * p;
    f += 0.5 * abs(value_noise(p));
    p = 2.0 * p;
    f += 0.25 * abs(value_noise(p));
    p = 2.0 * p;
    f += 0.125 * abs(value_noise(p));
    p = 2.0 * p;
    f += 0.0625 * abs(value_noise(p));
    p = 2.0 * p;
    f = sin(f + p.x / 32.0);
    return f;
}

float noise_sum_abs_sin_simplex(float2 p)
{
    float f = 0.0;
    p = p * 16.0;
    f += 1.0 * abs(simplex_noise(p));
    p = 2.0 * p;
    f += 0.5 * abs(simplex_noise(p));
    p = 2.0 * p;
    f += 0.25 * abs(simplex_noise(p));
    p = 2.0 * p;
    f += 0.125 * abs(simplex_noise(p));
    p = 2.0 * p;
    f += 0.0625 * abs(simplex_noise(p));
    p = 2.0 * p;
    f = sin(f + p.x / 32.0);
    return f;
}
