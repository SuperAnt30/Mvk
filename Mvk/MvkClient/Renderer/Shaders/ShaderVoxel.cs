namespace MvkClient.Renderer.Shaders
{
    /// <summary>
    /// Шейдер кубиков
    /// </summary>
    public class ShaderVoxel : ShaderVE
    {
        protected override string _VertexShaderSource { get; } = @"#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec2 v_texCoord;
layout(location = 2) in vec4 v_color;

out vec4 a_color;
out vec2 a_texCoord;
out vec4 a_position;

uniform mat4 projview;

void main()
{
    a_color = v_color;
	a_texCoord = v_texCoord;
	gl_Position = projview * vec4(v_position, 1.0f);
}";
        protected override string _FragmentShaderSource { get; } = @"#version 330 core
 
in vec4 a_color;
in vec2 a_texCoord;
out vec4 f_color;

uniform sampler2D u_texture0;

void main(){
	f_color = a_color * texture(u_texture0, a_texCoord);
}";
    }
}
