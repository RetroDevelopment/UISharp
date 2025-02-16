#version 330 core

uniform vec4 color;
uniform vec4 clipArea;
uniform sampler2D mainTexture;
uniform bool hasTexture;

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
    vec4 textureColor = texture(mainTexture, TextureCoordinates);
    //FragColor = textureColor * vec4(color.rgb, color.a * mask);
    //FragColor = mix(color, textureColor, textureColor.a) * mask;
   
    // TODO: remove the if then else and use step
    if (hasTexture)
    {
        FragColor = textureColor * mask;
    }
    else
    {
        FragColor = color * mask;
    }
}