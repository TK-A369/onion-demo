#version 330 core

in vec2 v_world_pos;
in float v_light_intensity;
in vec3 v_light_color;

out vec4 frag_color;

uniform sampler2D texture_world = 0;
uniform sampler2D texture_light = 1;
// uniform sampler2D textureShadows;

void main()
{
	vec4 c1 = texture(texture_world, v_world_pos);
	vec4 c2 = texture(texture_light, v_world_pos) * v_light_intensity;
	// frag_color = vec4(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z, 1.0);
	frag_color = vec4(c2.x, c2.y, c2.z, 1.0);
	// frag_color = vec4(1.0, 0.0, 0.0, 1.0);
}