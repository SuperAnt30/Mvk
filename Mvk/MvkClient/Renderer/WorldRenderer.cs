using MvkAssets;
using MvkClient.Renderer.Chunk;
using MvkClient.Renderer.Shaders;
using MvkClient.World;
using MvkServer.Glm;
using SharpGL;
using System.Threading.Tasks;

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

        public WorldRenderer(WorldClient world)
        {
            World = world;
        }

        /// <summary>
        /// Прорисовка мира
        /// </summary>
        public void Draw()
        {
            //GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
            //GLWindow.gl.Disable(OpenGL.GL_CULL_FACE);

            ShaderVoxel shader = GLWindow.Shaders.ShVoxel;
            shader.Bind(GLWindow.gl);
            //shader.SetUniformMatrix4(GLWindow.gl, "projview", glm.ortho(0, GLWindow.WindowWidth, GLWindow.WindowHeight, 0).to_array());
            if (World.Player.Projection == null) World.Player.UpProjection();
            if (World.Player.LookAt == null) World.Player.UpLookAt();
            shader.SetUniformMatrix4(GLWindow.gl, "projection", World.Player.Projection);
            shader.SetUniformMatrix4(GLWindow.gl, "lookat", World.Player.LookAt);

            GLWindow.Texture.BindTexture(AssetsTexture.Atlas);

            // Временно! рендер прямо в кадрах
            int radius = World.Player.OverviewChunk;
            int chx = World.Player.HitBox.ChunkPos.x;
            int chz = World.Player.HitBox.ChunkPos.y;
            for (int x = chx - radius; x <= chx + radius; x++)
            {
                for (int z = chz - radius; z <= chz + radius; z++)
                {
                    ChunkRender chunk = World.ChunkPrClient.GetChunkRender(new vec2i(x, z), true);
                    if (chunk.IsModifiedToRender)
                    {
                        // в отдельном потоке рендер
                        vec2i pos = new vec2i(x, z);
                        Task.Factory.StartNew(() =>
                        {
                            if (World.IsChunksSquareLoaded(pos))
                            {
                                //Debug.DInt++;
                                chunk.Render();
                                //Debug.CountPoligon += chunk.CountPoligon;
                            }
                        });
                    }
                    else
                    {
                        // после рендера если был заливаем буфер
                        chunk.BufferToRender();
                        // прорисовка
                        chunk.MeshDense.Draw();
                    }
                }
            }

            shader.Unbind(GLWindow.gl);

            //GLWindow.gl.Enable(OpenGL.GL_CULL_FACE);
            //GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
        }
    }
}
