#version 330 core

layout (location = 0) in vec2 a_pos;
layout (location = 1) in float a_light_intensity;
layout (location = 2) in vec3 a_light_color;

out vec2 v_world_pos;
out float v_light_intensity;
out vec3 v_light_color;

void main()
{
	gl_Position = vec4(a_pos.x, a_pos.y, 0.0, 1.0);
	v_world_pos = (a_pos + vec2(1.0, 1.0)) / 2.0;
	v_light_intensity = a_light_intensity;
	v_light_color = a_light_color;
}