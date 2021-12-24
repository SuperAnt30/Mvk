using MvkAssets;
using MvkClient.Renderer.Shaders;
using MvkClient.World;
using MvkServer.Glm;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Объект рендера мира
    /// </summary>
    public class WorldRenderer
    {
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; protected set; }

        private RenderMesh render = new RenderMesh();

        public WorldRenderer(WorldClient world)
        {
            World = world;
            float[] buffer = RenderMesh.Rectangle2d(new vec2(200), new vec2(1224), new vec2(0), new vec2(0.5f), new vec4(1, 1, 1, 1));
            render.Render(buffer);
        }

        /// <summary>
        /// Прорисовка мира
        /// </summary>
        public void Draw()
        {
            //ShaderVoxel shader = GLWindow.Shaders.ShVoxel;
            //shader.Bind(GLWindow.gl);
            //shader.SetUniformMatrix4(GLWindow.gl, "projview", glm.ortho(0, GLWindow.WindowWidth, GLWindow.WindowHeight, 0).to_array());
                        
            //GLWindow.Texture.BindTexture(AssetsTexture.Atlas);
            //render.Draw();

            //shader.Unbind(GLWindow.gl);
        }
    }
}
