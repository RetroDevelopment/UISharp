#version 330 core

uniform mat3 projection;
// Right now we only use 2 transforms: one for outer border scaling and one for inner border scaling (to generate borders).
uniform mat3 transforms[2];
uniform vec2 offsetMultiplier;
uniform bool visible;

layout (location = 0) in vec2 position;
layout (location = 1) in float transformMatrixIndex;
layout (location = 2) in vec2 offset;

out vec2 FragmentCoorindates;
out vec2 TextureCoordinates;

void main()
{
    // Dynamically compute offsets (used for rounded corners). The offset moves points in a corner to generate a corner radius and it is multiplied by a factor to dynamically determine the corner radius.
    vec2 offsetPosition = position + offsetMultiplier * offset;
    // Apply transform matrix.
    vec3 transformedPos = transforms[int(transformMatrixIndex)] * vec3(offsetPosition, 1.0);
    // Apply ortogonal projection to transform pixel coordinates into normalized coordinates.
    vec3 projectedPos = projection * transformedPos;
    
    FragmentCoorindates = transformedPos.xy;
    TextureCoordinates = transformMatrixIndex == 0.0f ? vec2(position.x + 0.5, -position.y + 0.5) : vec2(0.5, 0.5);
    // Small imprecision to avoid rounding errors to make coordinate fall between textels causing text artifacts
    TextureCoordinates += 0.001;
    FragmentCoorindates += 0.01;

    // If visible == true gl_position is projectedPos, otherwise gl_position is always 0, meaning the vertices
    // won't be visible.
    gl_Position = mix(vec4(0.0, 0.0, 0.0, 0.0),
                      vec4(projectedPos.xy, 0.0, 1.0),
                      step(0.5, float(visible)));
}
