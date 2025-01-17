#version 330 core

uniform mat3 projection;
uniform mat3 transforms[2]; // Right now we only use 2 transforms: one for outer border scaling and one for inner border scaling (to generate borders).
uniform vec2 offsetMultiplier;

layout (location = 0) in vec2 position;
layout (location = 1) in float transformMatrixIndex;
layout (location = 2) in vec2 offset;

out vec2 FragmentCoorindates;
out vec2 TextureCoordinates;

void main()
{
    vec2 offsetPosition = position + offsetMultiplier * offset; // Dynamically compute offsets (used for round corners). The offset moves points in a corner to generate a corner radius and it is multiplied by a factor to dynamically determine the corner radius.
    vec3 transformedPos = transforms[int(transformMatrixIndex)] * vec3(offsetPosition, 1.0); // Apply transform matrix.
    vec3 projectedPos = projection * transformedPos; // Apply ortogonal projection to transform pixel coordinates into normalized coordinates.

    gl_Position = vec4(projectedPos.xy, 0.0, 1.0);
    FragmentCoorindates = transformedPos.xy;
    TextureCoordinates = transformMatrixIndex == 0.0f ? vec2(position.x + 0.5, -position.y + 0.5) : vec2(0.5, 0.5);
}
