#version 330 core

in  vec2 v_tex_coord;

out vec4 frag_color;

uniform sampler2D texture0;

void main()
{
	frag_color = texture(texture0, v_tex_coord);
}