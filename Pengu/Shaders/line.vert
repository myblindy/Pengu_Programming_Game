#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) in vec2 inPos;

layout(location = 0) out vec4 fragColor;

void main() 
{
    gl_Position = vec4(inPos, 0.0, 1.0);
    fragColor = gl_VertexIndex < 20 ? vec4(1, 1, 1, 1) : vec4(0.5, 0.5, 1, 0.5);
}