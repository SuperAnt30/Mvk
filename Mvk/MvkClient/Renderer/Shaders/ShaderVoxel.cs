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
layout(location = 2) in vec3 v_color;
layout(location = 3) in vec2 v_light;

out vec4 a_color;
out vec2 a_texCoord;

uniform mat4 projection;
uniform mat4 lookat;
uniform vec3 pos;

void main()
{
    vec3 pos2 = pos + v_position;
    gl_Position = projection * lookat * vec4(pos2, 1.0);
//    a_color = vec4(1.0, 1.0, 1.0, 1.0);
    a_color = vec4(v_color, 1.0);
	a_texCoord = v_texCoord;
}";
        protected override string _FragmentShaderSource { get; } = @"#version 330 core
 
in vec4 a_color;
in vec2 a_texCoord;
out vec4 f_color;

uniform sampler2D u_texture0;

void main()
{
	f_color = a_color * texture(u_texture0, a_texCoord);
}";
    }
}
