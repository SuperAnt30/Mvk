using MvkAssets;
using MvkClient.Renderer.Chunk;
using MvkClient.Renderer.Shaders;
using MvkClient.World;
using MvkServer;
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
        /// Плавное перемещение камиры игрока
        /// </summary>
        protected void PlayerMove()
        {
            bool isFrustumCulling = false;
            // Перерасчёт расположение игрока если было смещение, согласно индексу времени
            if (!World.Player.Position.Equals(World.Player.PositionLast))
            {
                // идёт плавное перемещение
                float index = World.Player.TimeIndex();
                vec3 vp = (World.Player.Position - World.Player.PositionLast) * index;
                World.Player.SetMoveDraw(World.Player.Position + vp);
                isFrustumCulling = true;
            }

            // Если имеется вражение камеры или было перемещение, то запускаем расчёт FrustumCulling
            if (World.Player.RotationEquals() || isFrustumCulling || World.Player.IsFrustumCulling)
            {
                World.Player.InitFrustumCulling();
            }
        }

        /// <summary>
        /// Прорисовка мира
        /// </summary>
        public void Draw()
        {
            long time = Client.Time();

            PlayerMove();

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


            RemoteMeshChunks();

            int countRender = MvkGlobal.COUNT_RENDER_CHUNK_FRAME;
            bool fastTime = Client.Time() - time <= MvkGlobal.COUNT_RENDER_CHUNK_FRAME;

            for (int i = 0; i < World.Player.ChunkFC.Length; i++)
            {
                bool fast = fastTime || countRender == MvkGlobal.COUNT_RENDER_CHUNK_FRAME;
                ChunkRender chunk = World.Player.ChunkFC[i];
                if (chunk.IsModifiedToRender && fast && countRender > 0)
                {
                    // в отдельном потоке рендер
                    vec2i pos = new vec2i(World.Player.ChunkFC[i].Position);
                    if (World.IsChunksSquareLoaded(pos))
                    {
                        countRender--;
                        Task.Factory.StartNew(chunk.Render);
                    }
                }
                else
                {
                    chunk.Draw(fast);
                }
            }

            shader.Unbind(GLWindow.gl);

            //GLWindow.gl.Enable(OpenGL.GL_CULL_FACE);
            //GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
        }

        /// <summary>
        /// Чистка сетки опенгл
        /// </summary>
        protected void RemoteMeshChunks()
        {
            long time = Client.Time();
            long timeNew = time;
            // 4 мc на чистку чанков
            while(World.ChunkPrClient.RemoteMeshChunks.Count > 0 && timeNew - time < 5)
            {
                if (World.ChunkPrClient.RemoteMeshChunks[0] != null 
                    && !World.ChunkPrClient.RemoteMeshChunks[0].IsChunkLoaded)
                {
                    World.ChunkPrClient.RemoteMeshChunks[0].MeshDelete();
                }
                World.ChunkPrClient.RemoteMeshChunks.RemoveAt(0);
                timeNew = Client.Time();
            }
        }
    }
}
