using MvkAssets;
using MvkClient.Entity;
using MvkClient.Renderer.Chunk;
using MvkClient.Renderer.Shaders;
using MvkClient.World;
using MvkServer;
using MvkServer.Entity;
using MvkServer.Glm;
using SharpGL;
using System.Collections.Generic;
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
            long time = Client.Time();
            if (World.Player.Projection == null) World.Player.UpProjection();
            if (World.Player.LookAt == null) World.Player.UpLookAt();

            // время от TPS клиента
            float indexW = World.TimeIndex();
            foreach (EntityPlayerClient entity in World.Entities.Values)
            {
                entity.UpdatePositionFrame(entity.TimeIndex());
            }
            World.Player.UpdateFrame();

            RenderHitBoxEntitis();
            DrawLine();

            //GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
            //GLWindow.gl.Disable(OpenGL.GL_CULL_FACE);

            ShaderVoxel shader = GLWindow.Shaders.ShVoxel;
            shader.Bind(GLWindow.gl);
            //shader.SetUniformMatrix4(GLWindow.gl, "projview", glm.ortho(0, GLWindow.WindowWidth, GLWindow.WindowHeight, 0).to_array());
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


        private LineMesh hitboxPlayer = new LineMesh();


        private void RenderHitBoxEntitis()
        {
            List<float> buffer = new List<float>();
            buffer.AddRange(RenderHitBox(World.Player));
            foreach(EntityPlayerClient entity in World.Entities.Values)
            {
                if (entity.Name != World.Player.Name) buffer.AddRange(RenderHitBox(entity));
            }
            hitboxPlayer.BindBuffer(buffer);
        }

        private List<float> RenderHitBox(EntityPlayerClient entity)
        {
            vec3 pos = entity.PositionFrame;
            float w = entity.Width; // .6
            float w2 = w * 2f;
            float h = entity.Height; // 3.6
            float e = entity.GetEyeHeight(); 
            float y = pos.y + h / 2f;
            List<float> buffer = new List<float>();
            buffer.AddRange(hitboxPlayer.Box(pos.x, y, pos.z, w2, h, w2, 0, 1, 1, 1));
            y = pos.y + e;
            vec4 col = new vec4(1, .5f, 1, 1);
            buffer.AddRange(hitboxPlayer.Line(pos.x - w, y, pos.z + w, pos.x + w, y, pos.z + w, col));
            buffer.AddRange(hitboxPlayer.Line(pos.x - w, y, pos.z - w, pos.x + w, y, pos.z - w, col));
            buffer.AddRange(hitboxPlayer.Line(pos.x + w, y, pos.z - w, pos.x + w, y, pos.z + w, col));
            buffer.AddRange(hitboxPlayer.Line(pos.x - w, y, pos.z - w, pos.x - w, y, pos.z + w, col));
            
            float ycos = glm.cos(entity.RotationPitch);
            vec3 ray = new vec3(
                glm.sin(entity.RotationYaw) * ycos,
                glm.sin(entity.RotationPitch),
                -glm.cos(entity.RotationYaw) * ycos
                ).normalize() * 2f;

            buffer.AddRange(hitboxPlayer.Line(pos.x, y, pos.z, pos.x + ray.x, y + ray.y, pos.z + ray.z, col));

            return buffer;
            //hitboxPlayer.BindBuffer(buffer);
        }

        /// <summary>
        /// Прорисовка линий 3д
        /// </summary>
        protected void DrawLine()
        {
            ShaderLine shader = GLWindow.Shaders.ShLine;
            shader.Bind(GLWindow.gl);
            shader.SetUniformMatrix4(GLWindow.gl, "projection", World.Player.Projection);
            shader.SetUniformMatrix4(GLWindow.gl, "lookat", World.Player.LookAt);

            hitboxPlayer.DrawLine();
            //WorldLineM.Draw();

            shader.Unbind(GLWindow.gl);
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
