using MvkAssets;
using MvkClient.Entity;
using MvkClient.Renderer.Chunk;
using MvkClient.Renderer.Shaders;
using MvkClient.Util;
using MvkClient.World;
using MvkServer;
using MvkServer.Glm;
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

        /// <summary>
        /// Дополнительный счётчик, для повторной проверки, если камера не двигается, а чанки догружаются
        /// Возможноcть обрабатывать только структуру чанка чтоб догрузить в FC чанк
        /// </summary>
        private int addInitFrustumCulling = 0;

        public WorldRenderer(WorldClient world)
        {
            World = world;
        }

        /// <summary>
        /// Прорисовка мира
        /// </summary>
        public void Draw()
        {
            // время от TPS клиента
            float timeIndex = World.TimeIndex();

            if (World.Player.Projection == null) World.Player.UpProjection();
            if (World.Player.LookAt == null) World.Player.UpLookAt(timeIndex);

            // Обновить кадр основного игрока, камера и прочее
            World.Player.UpdateFrame(timeIndex);

            // Воксели VBO
            DrawVoxel(timeIndex);

            // Сущности DisplayList
            DrawEntities(timeIndex);

            // Чистка сетки чанков при необходимости
            World.ChunkPrClient.RemoteMeshChunks();
        }

        
        /// <summary>
        /// Прорисовка вокселей VBO
        /// </summary>
        private void DrawVoxel(float timeIndex)
        {
            long time = Client.Time();

            GLWindow.Texture.BindTexture(AssetsTexture.Atlas);
            ShaderVoxel shader = GLWindow.Shaders.ShVoxel;
            shader.Bind(GLWindow.gl);
            shader.SetUniformMatrix4(GLWindow.gl, "projection", World.Player.Projection);
            shader.SetUniformMatrix4(GLWindow.gl, "lookat", World.Player.LookAt);
            //shader.SetUniform3(GLWindow.gl, "pos", 0, -24, 0);

            int countRender = MvkGlobal.COUNT_RENDER_CHUNK_FRAME;
            bool fastTime = Client.Time() - time <= MvkGlobal.COUNT_RENDER_CHUNK_FRAME;

            for (int i = 0; i < World.Player.ChunkFC.Length; i++)
            {
                bool fast = fastTime || countRender == MvkGlobal.COUNT_RENDER_CHUNK_FRAME;
                FrustumStruct fs = World.Player.ChunkFC[i];
                if (fs.IsChunk())
                {
                    ChunkRender chunk = fs.GetChunk();
                    if (chunk.IsModifiedToRender && fast && countRender > 0)
                    {
                        // в отдельном потоке рендер
                        vec2i pos = new vec2i(chunk.Position);
                        if (World.IsChunksSquareLoaded(pos))
                        {
                            countRender--;
                            Task.Factory.StartNew(chunk.Render);
                        }
                    }
                    else
                    {
                        shader.SetUniform3(GLWindow.gl, "pos", 
                            (chunk.Position.x << 4) - World.RenderEntityManager.CameraOffset.x, 
                            -World.RenderEntityManager.CameraOffset.y,
                            (chunk.Position.y << 4) - World.RenderEntityManager.CameraOffset.z);
                        chunk.Draw(fast);
                    }
                }
                else
                {
                    addInitFrustumCulling++;
                }
            }

            shader.Unbind(GLWindow.gl);

            // Если хоть один чанк не догружен, то продолжаем счётчик
            if (addInitFrustumCulling > 0) addInitFrustumCulling++;
            // Или много не догруженных чанков, либо прошло время для повторной проверки
            if (addInitFrustumCulling > 100)
            {
                addInitFrustumCulling = 0;
                World.Player.CheckChunkFrustumCulling();
            }
        }

        /// <summary>
        /// Прорисовка сущностей DisplayList
        /// </summary>
        private void DrawEntities(float timeIndex)
        {
            // Матрица камеры
            World.Player.CameraMatrixProjection();

            World.CountEntitiesShowBegin();
            // Основной игрок, вид сзади или спереди
            World.RenderEntityManager.RenderEntity(World.Player, timeIndex);

            // Остальные сущности
            foreach (EntityPlayerClient entity in World.PlayerEntities.Values)
            {
                if (entity.Name != World.Player.Name)
                {
                    World.RenderEntityManager.RenderEntity(entity, entity.TimeIndex());
                }
            }
        }

        //private LineMesh hitboxPlayer = new LineMesh();


        //private void RenderHitBoxEntitis(float timeIndex)
        //{
        //    List<float> buffer = new List<float>();
        //    if (World.Player.ViewCamera != EnumViewCamera.Eye) buffer.AddRange(RenderHitBox(World.Player, timeIndex));
        //    foreach(EntityPlayerClient entity in World.PlayerEntities.Values)
        //    {
        //        if (entity.Name != World.Player.Name) buffer.AddRange(RenderHitBox(entity, entity.TimeIndex()));
        //    }
        //    hitboxPlayer.BindBuffer(buffer);
        //}

        //private List<float> RenderHitBox(EntityPlayerClient entity, float timeIndex)
        //{
        //    vec3 pos = entity.GetPositionFrame(timeIndex);// entity.PositionFrame;
        //    float w = entity.Width; // .6
        //    float w2 = w * 2f;
        //    float h = entity.Height; // 3.6
        //    float e = entity.GetEyeHeight(); 
        //    float y = pos.y + h / 2f;
        //    List<float> buffer = new List<float>();
        //    buffer.AddRange(hitboxPlayer.Box(pos.x, y + 0.01f, pos.z, w2, h, w2, 0, 1, 1, 1));
        //    y = pos.y + e;
        //    vec4 col = new vec4(1, .5f, 1, 1);
        //    buffer.AddRange(hitboxPlayer.Line(pos.x - w, y, pos.z + w, pos.x + w, y, pos.z + w, col));
        //    buffer.AddRange(hitboxPlayer.Line(pos.x - w, y, pos.z - w, pos.x + w, y, pos.z - w, col));
        //    buffer.AddRange(hitboxPlayer.Line(pos.x + w, y, pos.z - w, pos.x + w, y, pos.z + w, col));
        //    buffer.AddRange(hitboxPlayer.Line(pos.x - w, y, pos.z - w, pos.x - w, y, pos.z + w, col));

        //    // RED тело
        //    col = new vec4(1f, 0, 0, 1);
        //    vec3 rayBody = entity.GetLookBodyFrame(timeIndex);
        //    rayBody.y = 0;
        //    rayBody = rayBody.normalize();
        //    buffer.AddRange(hitboxPlayer.Line(pos.x, pos.y + .01f, pos.z, pos.x + rayBody.x, pos.y + .01f, pos.z + rayBody.z, col));

        //    // YELOUW голова
        //    col = new vec4(1f, 1f, 0, 1);
        //    vec3 rayHead = entity.GetLookFrame(timeIndex);
        //    buffer.AddRange(hitboxPlayer.Line(pos.x, y, pos.z, pos.x + rayHead.x, y + rayHead.y, pos.z + rayHead.z, col));
        //    vec3 rayHead2 = new vec3(rayHead.x, 0, rayHead.z);
        //    rayHead2 = rayHead2.normalize();
        //    buffer.AddRange(hitboxPlayer.Line(
        //        pos.x + rayHead2.x, pos.y + .01f, pos.z + rayHead2.z, 
        //        pos.x + rayHead2.x * 2f, pos.y + .01f, pos.z + rayHead2.z * 2f, col));

        //    pos += new vec3(rayHead.x, 0, rayHead.z) * 2f;
        //    float xz = glm.cos(entity.LimbSwing * 0.6662f) * 1.4f * entity.GetLimbSwingAmountFrame(timeIndex);
            
        //    buffer.AddRange(hitboxPlayer.Line(pos.x, y, pos.z, pos.x, y + xz, pos.z, col));

        //    return buffer;
        //}

        ///// <summary>
        ///// Прорисовка линий 3д
        ///// </summary>
        //protected void DrawLine()
        //{
        //    ShaderLine shader = GLWindow.Shaders.ShLine;
        //    shader.Bind(GLWindow.gl);
        //    shader.SetUniformMatrix4(GLWindow.gl, "projection", World.Player.Projection);
        //    shader.SetUniformMatrix4(GLWindow.gl, "lookat", World.Player.LookAt);

        //    hitboxPlayer.DrawLine();
        //    //WorldLineM.Draw();

        //    shader.Unbind(GLWindow.gl);
        //}
    }
}
