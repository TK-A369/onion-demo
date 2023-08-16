#version 330 core

layout (location = 0) in vec2 a_pos;

uniform mat3 camera = mat3(	1.0, 0.0, 0.0,
							0.0, 1.0, 0.0,
							0.0, 0.0, 1.0);

void main()
{
	vec3 pos_tmp1 = vec3(a_pos.x, a_pos.y, 1.0);
	vec3 pos_tmp2 = camera * pos_tmp1;
	gl_Position = vec4(pos_tmp2.x, pos_tmp2.y, 0.0, 1.0);
}