#version 460

//? #include "LightingConstants.glsl"

struct LightingTerms_t
{
    vec3 DiffuseDirect;
    vec3 DiffuseIndirect;
    vec3 SpecularDirect;
    vec3 SpecularIndirect;
    vec3 TransmissiveDirect;
    float SpecularOcclusion; // Lightmap AO
};

LightingTerms_t InitLighting()
{
    return LightingTerms_t(vec3(0), vec3(0), vec3(0), vec3(0), vec3(0), 1.0);
}

vec3 GetEnvLightDirection(uint nLightIndex)
{
    return normalize(mat3(g_matLightToWorld[nLightIndex]) * vec3(-1, 0, 0));
}

vec3 GetLightPositionWs(uint nLightIndex)
{
    return g_vLightPosition_Type[nLightIndex].xyz;
}

bool IsDirectionalLight(uint nLightIndex)
{
    return g_vLightPosition_Type[nLightIndex].a == 0.0;
}

vec3 GetLightDirection(vec3 vPositionWs, uint nLightIndex)
{
    if (IsDirectionalLight(nLightIndex))
    {
        return GetEnvLightDirection(nLightIndex);
    }

    vec3 lightPosition = GetLightPositionWs(nLightIndex);
    vec3 lightVector = normalize(lightPosition - vPositionWs);

    return lightVector;
}

vec3 GetLightColor(uint nLightIndex)
{
    vec3 vColor = g_vLightColor_Brightness[nLightIndex].rgb;
    float flBrightness = g_vLightColor_Brightness[nLightIndex].a;

    return vColor * flBrightness;
}
