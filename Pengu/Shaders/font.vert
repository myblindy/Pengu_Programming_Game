#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 0) uniform UniformBufferObject
{
    float time;
} ubo;

layout(location = 0) in vec4 inPosUv;
layout(location = 1) in float selected;

layout(location = 0) out vec4 fragColor;
layout(location = 1) out vec2 fragTexCoord;

void main() 
{
    gl_Position = vec4(inPosUv.xy, 0.0, 1.0);
    if(selected < 0.5)
        fragColor = vec4(1, 1, 1, 1);
    else
        fragColor = vec4(0, 1, 0, 1) * ((sin(ubo.time / 100.0) + 1.0) * 3.0 / 4.0 + 2.0 / 3.0);     // normalize and move to the upper 1/3
    fragTexCoord = inPosUv.zw;                                                                      // for a smoother animation
}