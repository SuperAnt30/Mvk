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
layout(location = 2) in int v_rgbl;
layout(location = 3) in int v_anim;
//layout(location = 2) in int v_r;
//layout(location = 3) in int v_g;
//layout(location = 4) in int v_b;
//layout(location = 5) in int v_light1;
//layout(location = 6) in int v_light2;
//layout(location = 2) in vec3 v_color;
//layout(location = 3) in vec2 v_light;

out vec4 a_color;
out vec2 a_texCoord;
out float fog_factor;
out vec3 fog_color;

uniform mat4 projection;
uniform mat4 lookat;
uniform float takt;
uniform float sky;
uniform float overview;
uniform vec3 colorfog;
uniform vec3 pos;
uniform vec3 camera;


void main()
{
    fog_color = colorfog;
    float camera_distance = distance(camera, vec3(v_position));
    fog_factor = pow(clamp(camera_distance / overview, 0.0, 1.0), 4.0);

    float r = (v_rgbl & 0xFF) / 255.0;
    float g = ((v_rgbl >> 8) & 0xFF) / 255.0;
    float b = ((v_rgbl >> 16) & 0xFF) / 255.0;
    float lightSky = ((v_rgbl >> 24) & 0xF) / 15.0;
    lightSky *= sky;
    float lightBlock = ((v_rgbl >> 28) & 0xF) / 15.0;
    float light = max(lightSky, lightBlock);

    // чтоб не сильно темно было, минимиальная яркость 0,2
    light = light * 0.8 + 0.2;

    a_texCoord = v_texCoord;

    int frame = (v_anim & 0xFF);
    if (frame > 0)
    {
        // анимация блоков
        int pause = ((v_anim >> 8) & 0xFF);
        int t;
        if (pause > 1) {
            int maxframe = frame * pause;
            int tt = maxframe - 1;
            t = ((int(takt) & tt) / pause);
        } else {
            int tt = frame - 1;
            t = (int(takt) & tt);
        }
        a_texCoord.y += t * 0.015625;
    }
    if (light < 0.1) light = 0.1;
    //light = 1.0;

    vec3 v_color = vec3(r * light, g * light, b * light);
    vec3 pos2 = pos + v_position;
    gl_Position = projection * lookat * vec4(pos2, 1.0);
    a_color = vec4(v_color, 1.0);
	//a_texCoord = v_texCoord;
}";
        protected override string _FragmentShaderSource { get; } = @"#version 330 core
 
in vec4 a_color;
in vec2 a_texCoord;
in float fog_factor;
in vec3 fog_color;

out vec4 f_color;

uniform sampler2D u_texture0;

void main()
{
    vec4 color = a_color * texture(u_texture0, a_texCoord);
    vec3 col3 = vec3(color);
    //vec3 cols = vec3(0.73, 0.83, 1.0);
    col3 = mix(col3, fog_color, fog_factor);
    f_color = vec4(col3, color.a);
	//f_color = a_color * texture(u_texture0, a_texCoord);
}";
    }
}
