#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) in vec4 inPosUv;

layout(location = 0) out vec4 fragColor;
layout(location = 1) out vec2 fragTexCoord;

void main() 
{
    gl_Position = vec4(inPosUv.xy, 0.0, 1.0);
    fragColor = vec4(1, 1, 1, 1);
    fragTexCoord = inPosUv.zw;
}