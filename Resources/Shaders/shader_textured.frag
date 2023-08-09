#version 330 core

in  vec4 v_color;
in  vec2 v_tex_coord;

out vec4 frag_color;

uniform sampler2D texture0;

void main()
{
	frag_color = mix(v_color, texture(texture0, v_tex_coord), 0.5); 
	//frag_color = texture(texture0, v_tex_coord); 
	//frag_color = vec4(v_tex_coord, 0.0, 1.0);
}