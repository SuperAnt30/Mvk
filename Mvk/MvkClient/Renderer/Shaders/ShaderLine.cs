namespace MvkClient.Renderer.Shaders
{
    /// <summary>
    /// Шейдер линий
    /// </summary>
    public class ShaderLine : ShaderVE
    {
        protected override string _VertexShaderSource { get; } = @"#version 330 core

layout(location = 0) in vec3 v_position;
layout(location = 1) in vec4 v_color;

out vec4 a_color;

uniform mat4 projection;
uniform mat4 lookat;

void main()
{
	a_color = v_color;
    gl_Position = projection * lookat * vec4(v_position, 1.0);
}";
        protected override string _FragmentShaderSource { get; } = @"#version 330 core
 
in vec4 a_color;
out vec4 f_color;

void main(){
	f_color = a_color;
}";
    }
}
