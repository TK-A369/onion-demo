#version 330 core

in  vec2 v_tex_coord;

out vec4 frag_color;

uniform sampler2D texture_lightmap;

void main()
{
	frag_color = texture(texture_lightmap, v_tex_coord);
}