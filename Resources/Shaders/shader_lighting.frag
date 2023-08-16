#version 330 core

in vec2 v_world_pos;
in float v_light_intensity;
in vec3 v_light_color;

out vec4 frag_color;

uniform sampler2D texture_world;
uniform sampler2D texture_light;
uniform sampler2D texture_shadows;

void main()
{
	vec4 c1 = texture(texture_world, v_world_pos);
	vec4 c2 = texture(texture_light, v_world_pos);
	vec4 c3 = vec4(v_light_color, 1.0) * v_light_intensity;
	vec4 c4 = texture(texture_shadows, v_world_pos);
	frag_color = vec4(c1.r * c2.r * c3.r * c4.r, c1.g * c2.g * c3.g * c4.g, c1.b * c2.b * c3.b * c4.b, 1.0);
	// frag_color = vec4(c2.x, c2.y, c2.z, 1.0);
	// frag_color = vec4(0.0, 1.0, 1.0, 1.0);
}