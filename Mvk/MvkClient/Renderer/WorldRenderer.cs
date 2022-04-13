using MvkAssets;
using MvkClient.Entity;
using MvkClient.Renderer.Chunk;
using MvkClient.Renderer.Shaders;
using MvkClient.Util;
using MvkClient.World;
using MvkServer;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Util;
using MvkServer.World.Block;
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
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; protected set; }

        /// <summary>
        /// Дополнительный счётчик, для повторной проверки, если камера не двигается, а чанки догружаются
        /// Возможноcть обрабатывать только структуру чанка чтоб догрузить в FC чанк
        /// </summary>
        private int addInitFrustumCulling = 0;
        /// <summary>
        /// DL курсора прицела
        /// </summary>
        private RenderAim renderAim;
        /// <summary>
        /// DL курсора блока
        /// </summary>
        private RenderBlockCursor renderBlockCursor;
        /// <summary>
        /// DL курсора чанков
        /// </summary>
        private RenderChunkCursor renderChunkCursor;

        /// <summary>
        /// Массив всех блоков для GUI
        /// </summary>
        private RenderBlockGui[] listBlocksGui = new RenderBlockGui[BlocksCount.COUNT + 1];

        public WorldRenderer(WorldClient world)
        {
            World = world;
            ClientMain = world.ClientMain;
            renderAim = new RenderAim();
            renderBlockCursor = new RenderBlockCursor(ClientMain);
            renderChunkCursor = new RenderChunkCursor { IsHidden = true };

            for (int i = 0; i <= BlocksCount.COUNT; i++)
            {
                listBlocksGui[i] = new RenderBlockGui((EnumBlock)i, 64f);
            }
        }

        /// <summary>
        /// Прорисовка мира
        /// </summary>
        public void Draw(float timeIndex)
        {
            // Обновить кадр основного игрока, камера и прочее
            ClientMain.Player.UpdateFrame(timeIndex);

            if (ClientMain.Player.Projection == null) ClientMain.Player.UpProjection();
            if (ClientMain.Player.LookAt == null) ClientMain.Player.UpLookAt(timeIndex);

            // Обновить кадр основного игрока, камера и прочее
            //ClientMain.Player.UpdateFrame(timeIndex);

            // Воксели VBO
            List<ChunkRender> chunks = DrawVoxel(timeIndex);

            // Матрица камеры
            ClientMain.Player.CameraMatrixProjection();

            // Сущности DisplayList
            DrawEntities(chunks, timeIndex);

            // Рендер и прорисовка курсора выбранного блока по AABB
            renderBlockCursor.Render(ClientMain.Player.SelectBlock);
            
            // Курсор чанка
            renderChunkCursor.Render(ClientMain.World.RenderEntityManager.CameraOffset);

            // Эффекты
            ClientMain.EffectRender.Render(timeIndex);

            // Прорисовка вид не с руки, а видим себя
            if (ClientMain.Player.ViewCamera != EnumViewCamera.Eye)
                World.RenderEntityManager.RenderEntity(ClientMain.Player, timeIndex);

            // Воксели альфа VBO
            DrawVoxelAlpha(timeIndex);

            // Эффект 2д
            DrawEff2d(timeIndex);

            // Матрица камеры
            ClientMain.Player.CameraMatrixProjection();
            // Прорисовка руки
            if (ClientMain.Player.ViewCamera == EnumViewCamera.Eye)
                World.RenderEntityManager.RenderEntity(ClientMain.Player, timeIndex);

            // Чистка сетки чанков при необходимости
            World.ChunkPrClient.RemoteMeshChunks();
        }

        private void DrawEff2d(float timeIndex)
        {
            // вода
            vec3 pos = ClientMain.Player.Position + ClientMain.Player.PositionCamera;
            BlockBase block = World.GetBlock(new BlockPos(pos));

            // TODO:: water material
            if (block.EBlock == EnumBlock.Water)
            {
                DrawEffWater(timeIndex);
            }

            // урон
            if (ClientMain.Player.DamageTime > 0 && ClientMain.Player.ViewCamera == EnumViewCamera.Eye)
            {
                DrawEffDamage(ClientMain.Player.DamageTime, timeIndex);
            }

            ClientMain.World.WorldRender.Draw2D();
        }


        /// <summary>
        /// Смена видимости курсора чанка
        /// </summary>
        public void ChunkCursorHiddenShow() => renderChunkCursor.IsHidden = !renderChunkCursor.IsHidden;

        /// <summary>
        /// Прорисовка вокселей VBO
        /// </summary>
        private List<ChunkRender> DrawVoxel(float timeIndex)
        {
            
            if (Debug.IsDrawVoxelLine)
            {
                GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                GLRender.CullDisable();
            }
            else
            {
                GLRender.CullEnable();
                GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
            }

            long time = Client.Time();
            ShaderVoxel shader = VoxelsBegin();
            int countRender = MvkGlobal.COUNT_RENDER_CHUNK_FRAME;
            bool fastTime = Client.Time() - time <= MvkGlobal.COUNT_RENDER_CHUNK_FRAME;
            List<ChunkRender> chunks = new List<ChunkRender>();

            int count = ClientMain.Player.ChunkFC.Length - 1;

            // Пробегаем по всем чанкам которые видим FrustumCulling
            for (int i = 0; i <= count; i++)
            {
                bool fast = fastTime || countRender == MvkGlobal.COUNT_RENDER_CHUNK_FRAME;
                
                FrustumStruct fs = ClientMain.Player.ChunkFC[i];
                if (fs.IsChunk())
                {
                    ChunkRender chunk = fs.GetChunk();
                    byte[] vs = fs.GetSortList();
                    // Пробегаем по псевдо чанкам одно чанка
                    foreach(int y in vs)
                    {
                        bool isDense = chunk.IsModifiedRender(y);
                        // Проверяем надо ли рендер для псевдо чанка, и возможно ли по времени
                        if ((isDense || chunk.IsModifiedRenderAlpha(y)) && fast && countRender > 0)
                        {
                            // Проверяем занят ли чанк уже рендером
                            if (chunk.IsMeshDenseWait(y) && chunk.IsMeshAlphaWait(y))
                            {
                                if (!isDense && chunk.CheckAlphaZero(y))
                                {
                                    // заход только для альфа но его нет
                                    chunk.NotRenderingAlpha(y);
                                }
                                else
                                {
                                    // Обновление рендера псевдочанка
                                    Debug.CountUpdateChunck++;
                                    chunk.StartRendering(y);
                                    countRender--;
                                    int chY = y;
                                    // в отдельном потоке рендер
                                    Task.Factory.StartNew(() => { chunk.Render(chY); });
                                }
                            }
                        }

                        // Занести буфер сплошных блоков псевдо чанка если это требуется
                        if (fast && chunk.IsMeshDenseBinding(y)) chunk.BindBufferDense(y);

                        // Прорисовка сплошных блоков псевдо чанка
                        if (chunk.NotNullMeshDense(y))
                        {
                            shader.SetUniform3(GLWindow.gl, "pos",
                                (chunk.Position.x << 4) - World.RenderEntityManager.CameraOffset.x,
                                (y << 4) - World.RenderEntityManager.CameraOffset.y,
                                (chunk.Position.y << 4) - World.RenderEntityManager.CameraOffset.z);
                            chunk.DrawDense(y);
                        }

                    }
                    chunks.Add(chunk);

                    // Тут бы сущность
                    // DrawEntities2(chunk.ListEntities, timeIndex);

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
                ClientMain.Player.CheckChunkFrustumCulling();
            }

            if (Debug.IsDrawVoxelLine)
            {
                // Дебаг должен прорисовать текстуру по этому сетка тут не уместна
                GLRender.CullEnable();
                GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
            }

            return chunks;
        }

        /// <summary>
        /// Прорисовка вокселей альфа цвета
        /// </summary>
        private void DrawVoxelAlpha(float timeIndex)
        {
            GLRender.CullEnable();
            ShaderVoxel shader = VoxelsBegin();
            GLRender.DepthEnable();
            GLRender.DepthMask(false);
            GLRender.BlendEnable();

            int count = ClientMain.Player.ChunkFC.Length - 1;

            // Пробегаем по всем чанкам которые видим FrustumCulling в обратном порядке, с далека и ближе
            for (int i = count; i >= 0; i--)
            {
                FrustumStruct fs = ClientMain.Player.ChunkFC[i];
                if (fs.IsChunk())
                {
                    ChunkRender chunk = fs.GetChunk();
                    byte[] vs = fs.GetSortList();
                    // Пробегаем по псевдо чанкам одно чанка в обратном порядке
                    for (int j = vs.Length - 1; j >= 0; j--)
                    {
                        int y = vs[j];

                        // Занести буфер альфа блоков псевдо чанка если это требуется
                        if (chunk.IsMeshAlphaBinding(y)) chunk.BindBufferAlpha(y);

                        // Прорисовка альфа блоков псевдо чанка
                        if (chunk.NotNullMeshAlpha(y))
                        {
                            shader.SetUniform3(GLWindow.gl, "pos",
                                (chunk.Position.x << 4) - World.RenderEntityManager.CameraOffset.x,
                                (y << 4) - World.RenderEntityManager.CameraOffset.y,
                                (chunk.Position.y << 4) - World.RenderEntityManager.CameraOffset.z);
                            chunk.DrawAlpha(y);
                        }
                    }
                }
            }
            shader.Unbind(GLWindow.gl);
            GLRender.DepthMask(true);
        }

        /// <summary>
        /// Запуск шейдеров и текстуры для прорисовки вокселей
        /// </summary>
        /// <returns></returns>
        private ShaderVoxel VoxelsBegin()
        {
            GLWindow.Texture.BindTexture(AssetsTexture.Atlas);
            ShaderVoxel shader = GLWindow.Shaders.ShVoxel;
            shader.Bind(GLWindow.gl);
            shader.SetUniformMatrix4(GLWindow.gl, "projection", ClientMain.Player.Projection);
            shader.SetUniformMatrix4(GLWindow.gl, "lookat", ClientMain.Player.LookAt);
            shader.SetUniform1(GLWindow.gl, "takt", ClientMain.TickCounter); // & 31
            return shader;
        }

        private void DrawEntities2(MapListEntity[] entities, float timeIndex)
        {
            for (int i = 0; i < entities.Length; i++)
            {
                if (!entities[i].IsEmpty())
                {
                    for (int j = 0; j < entities[i].Count; j++)
                    {
                        EntityBase entity = entities[i].GetAt(j);
                        int playerId = ClientMain.Player.Id;
                        int entityId = entity.Id;
                        if (entityId != playerId)
                        {
                            World.RenderEntityManager.RenderEntity(entity, timeIndex);// entity.TimeIndex());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Прорисовка сущностей DisplayList
        /// </summary>
        private void DrawEntities(List<ChunkRender> chunks, float timeIndex)
        {
            World.CountEntitiesShowBegin();

            // Остальные сущности
            foreach (ChunkRender chunk in chunks)
            {
                DrawEntities2(chunk.ListEntities, timeIndex);
            }

            // Основной игрок, вид сзади или спереди
            //if (ClientMain.Player.Type == EnumEntities.PlayerHand)
            //{
            //    World.RenderEntityManager.RenderHand(timeIndex);
            //}
            //else
            //if (ClientMain.Player.ViewCamera != EnumViewCamera.Eye)
            //{
            //    World.RenderEntityManager.RenderEntity(ClientMain.Player, timeIndex);
            //}
        }

        private void DrawEffWater(float timeIndex)
        {
            int w = GLWindow.WindowWidth;
            int h = GLWindow.WindowHeight;
            // Эффект
            GLWindow.gl.MatrixMode(OpenGL.GL_PROJECTION);
            GLWindow.gl.LoadIdentity();
            GLWindow.gl.Ortho2D(0, w, h, 0);
            GLWindow.gl.MatrixMode(OpenGL.GL_MODELVIEW);
            GLWindow.gl.LoadIdentity();

            GLRender.PushMatrix();
            GLRender.Texture2DDisable();
            GLRender.Rectangle(0, 0, w, h, new vec4(0.0f, 0.1f, 0.4f, 0.7f));
            GLRender.PopMatrix();
        }

        private void DrawEffDamage(float damageTime, float timeIndex)
        {
            float dt = Mth.Sqrt((damageTime + timeIndex - 1f) / 5f * 1.6f);
            if (dt > 1f) dt = 1f;

            int w = GLWindow.WindowWidth;
            int h = GLWindow.WindowHeight;
            // Эффект
            GLWindow.gl.MatrixMode(OpenGL.GL_PROJECTION);
            GLWindow.gl.LoadIdentity();
            GLWindow.gl.Ortho2D(0, w, h, 0);
            GLWindow.gl.MatrixMode(OpenGL.GL_MODELVIEW);
            GLWindow.gl.LoadIdentity();

            GLRender.PushMatrix();
            GLRender.Texture2DDisable();
            GLRender.Rectangle(0, 0, w, h, new vec4(0.7f, 0.4f, 0.3f, 0.7f * dt));
            GLRender.PopMatrix();
        }

        //RenderBlockGui blockGui = new RenderBlockGui(EnumBlock.Cobblestone, 64f);
        /// <summary>
        /// Прорисовать 2д
        /// </summary>
        private void Draw2D()
        {
            int w = GLWindow.WindowWidth;
            int h = GLWindow.WindowHeight;

            renderAim.MatrixOrtho2d(w, h);

            // Прицел
            renderAim.IsHidden = ClientMain.Player.ViewCamera != EnumViewCamera.Eye;
            renderAim.Render(w, h);

            ItemStack itemStack = ClientMain.Player.Inventory.GetCurrentItem();
            if (itemStack != null && itemStack.Item is ItemBlock itemBlock)
            {
                listBlocksGui[(int)itemBlock.Block.EBlock].Render(64, 64);
            }
                
            //listBlocksGui[2].Render(w / 4 + 50, 16);
            //listBlocksGui[3].Render(w / 4 + 100, 16);
            //listBlocksGui[4].Render(w / 4 + 150, 16);

            //blockGui.Render2(100, 100);

            // ХП
            GLRender.PushMatrix();
            GLRender.Texture2DDisable();
            GLRender.LineWidth(1f);
            GLRender.Color(new vec3(0));
            for (int i = 0; i < 20; i++)
            {
                GLRender.Begin(OpenGL.GL_LINE_STRIP);
                GLRender.Vertex(30, h - 46 - i * 20, 0);
                GLRender.Vertex(46, h - 46 - i * 20, 0);
                GLRender.Vertex(46, h - 30 - i * 20, 0);
                GLRender.Vertex(30, h - 30 - i * 20, 0);
                GLRender.Vertex(30, h - 46 - i * 20, 0);
                GLRender.End();
            }

            int count = Mth.Floor(ClientMain.Player.Health);

            for (int i = 0; i < count; i++)
            {
                GLRender.Rectangle(31, h - 45 - i * 20, 45, h - 31 - i * 20, new vec4(.9f, .4f, .4f, .7f));
            }


            GLRender.PopMatrix();
        }

        /// <summary>
        /// Рендер и прорисовка курсора выбранного блока по AABB
        /// DisplayList
        /// </summary>
        //private void DrawCursorVoxel()
        //{
        //    if (ClientMain.Player.SelectBlock != null)
        //    {
        //        renderBlockCursor.Render(ClientMain.Player.SelectBlock);
        //    }
        //}


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
