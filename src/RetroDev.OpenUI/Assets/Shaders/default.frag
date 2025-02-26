#version 330 core

uniform vec4 color;
uniform vec4 clipArea;
uniform sampler2D mainTexture;
uniform int textureMode;
uniform float zIndex;

in vec2 FragmentCoorindates;
in vec2 TextureCoordinates;

out vec4 FragColor;

// Function to calculate the clipping mask
float includeInClippingArea(vec2 coord)
{
    //step(edge, x) returns 0.0 if x is less than edge, and 1.0 otherwise.
    float xMask = step(clipArea.x, coord.x) * step(coord.x, clipArea.z);
    float yMask = step(clipArea.w, coord.y) * step(coord.y, clipArea.y);
    
    return xMask * yMask;
}

void main()
{
    float mask = includeInClippingArea(FragmentCoorindates);

    // Using branching here because condition only use uniform, so fragment shader will be recompiled
    // removing the condition. This avoids for example texture sampling or other unnecessary calculations.
    // With instanced rendering these recompilation will not be frequent.

    // No tetxure
    if (textureMode == 0)
    {
        FragColor = color * mask;
    }
    // RGBA texture
    else if (textureMode == 1)
    {
        vec4 textureColor = texture(mainTexture, TextureCoordinates);
        FragColor = textureColor * mask;
    }
    // Gray scale texture    
    else 
    {
        vec4 textureColor = texture(mainTexture, TextureCoordinates);
        vec4 colorizedGrayImageColor = vec4(color.r, color.g, color.b, textureColor.r * color.a);
        FragColor = colorizedGrayImageColor * mask;
    }

    // If mask is 0 gl_FragDept = 1.0, that is, the fragment will be clipped and it will not overwrite the buffer to avoid transparency artifacts
    // If the fragment is fully transparetn (FragColor.a == 0) same thing, because otherwise the depth buffer value would prevent from rendering solid colors below this fragment
    // Note the complexity in this formula is introduced because fully transparent vbos are batched together with fully opaque vbos, therefore not rendered in back to front order like semi-transparent vbos.
    // This is because there are many fully transparent objects (e.g. label background) and it would kill performance to run them in back to front order, since this would require a draw call per object.
    gl_FragDepth = mix(zIndex * mask + (1.0 - mask), 1.0, step(FragColor.a, 0.0));
}