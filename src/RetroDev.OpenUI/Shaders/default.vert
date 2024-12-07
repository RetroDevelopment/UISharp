#version 330 core

uniform mat3 transform;

layout (location = 0) in vec2 position;

out vec2 FragmentCoorindates;
out vec2 TextureCoordinates;

void main()
{
    vec3 transformedPos = transform * vec3(position, 1.0);
    gl_Position = vec4(transformedPos.xy, 0.0, 1.0);
    FragmentCoorindates = transformedPos.xy;
    TextureCoordinates = vec2((position.x + 1.0) / 2.0, (-position.y + 1.0) / 2.0);
}