#version 450
#extension GL_ARB_separate_shader_objects : enable

const vec4 colors[] = 
{
    vec4(  0 / 255.0,   0 / 255.0,   0 / 255.0, 1),             // Black
    vec4(  0 / 255.0,  55 / 255.0, 218 / 255.0, 1),             // DarkBlue
    vec4( 19 / 255.0, 161 / 255.0,  14 / 255.0, 1),             // DarkGreen
    vec4( 58 / 255.0, 150 / 255.0, 221 / 255.0, 1),             // DarkCyan
    vec4(197 / 255.0,  15 / 255.0,  31 / 255.0, 1),             // DarkRed
    vec4(136 / 255.0,  23 / 255.0, 152 / 255.0, 1),             // DarkMagenta
    vec4(193 / 255.0, 156 / 255.0,   0 / 255.0, 1),             // DarkYellow
    vec4(204 / 255.0, 204 / 255.0, 204 / 255.0, 1),             // DarkWhite
    vec4(118 / 255.0, 118 / 255.0, 118 / 255.0, 1),             // BrightBlack
    vec4( 59 / 255.0, 120 / 255.0, 255 / 255.0, 1),             // BrightBlue
    vec4( 22 / 255.0, 198 / 255.0,  12 / 255.0, 1),             // BrightGreen
    vec4( 97 / 255.0, 214 / 255.0, 214 / 255.0, 1),             // BrightCyan
    vec4(231 / 255.0,  72 / 255.0,  86 / 255.0, 1),             // BrightRed
    vec4(180 / 255.0,   0 / 255.0, 158 / 255.0, 1),             // BrightMagenta
    vec4(249 / 255.0, 241 / 255.0, 165 / 255.0, 1),             // BrightYellow
    vec4(242 / 255.0, 242 / 255.0, 242 / 255.0, 1),             // White
};

const int selectedColorIndex = 3;                                  

layout(binding = 0) uniform UniformBufferObject
{
    float time;
} ubo;

layout(location = 0) in vec4 inPosUv;
layout(location = 1) in int bgFgSelected;
layout(location = 2) in vec2 offset;

layout(location = 0) out vec4 fragBackgroundColor;
layout(location = 1) out vec4 fragColor;
layout(location = 2) out vec2 fragTexCoord;

void main() 
{
    gl_Position = vec4(inPosUv.xy + offset, 0.0, 1.0);

    fragBackgroundColor = colors[bgFgSelected >> 16];
    fragColor = colors[(bgFgSelected >> 8) & 0xFF];

    if((bgFgSelected & 0xFF) > 0)
        fragColor *= ((sin(ubo.time / 100.0) + 1.0) * 3.0 / 4.0 + 2.0 / 3.0);                       // normalize and move to the upper 1/3

    fragTexCoord = inPosUv.zw;                                                                      // for a smoother animation
}