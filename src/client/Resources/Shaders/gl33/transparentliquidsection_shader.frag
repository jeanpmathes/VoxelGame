#version 330

out vec4 outputColor;

flat in int texIndex;
in vec2 texCoord;

in vec4 tint;

// binding 5
uniform sampler2DArray arrayTexture;
// binding 20
uniform sampler2D depthTex;

uniform float time;

float linearize_depth(float z_b, float zNear, float zFar)
{
	float z_n = 2.0 * z_b- 1.0;
    return 2.0 * zNear * zFar / (zFar + zNear - z_n * (zFar - zNear));
}

void main()
{
	vec4 color = texture(arrayTexture, vec3(texCoord, texIndex + int(mod(time * 16, 16))));
	float depth = texelFetch(depthTex, ivec2(int(gl_FragCoord.x), int(gl_FragCoord.y)), 0).x;

	float depth_linear = linearize_depth(depth, 0.1, 1000);
	float dist_linear = linearize_depth(gl_FragCoord.z, 0.1, 1000);

	float eff = clamp((depth_linear - dist_linear) * 0.2, 0.0, 0.7);

	color *= tint;

	vec4 dark_opaque = mix(vec4(color.rgb, 1.0), vec4(0.0, 0.0, 0.0, 1.0), 0.5);

	outputColor = mix(color, dark_opaque, eff);
}