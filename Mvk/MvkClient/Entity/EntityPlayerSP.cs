using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Util;
using MvkClient.World;
using MvkServer;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Management;
using MvkServer.Network.Packets.Client;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using SharpGL;
using System;
using System.Collections.Generic;

namespace MvkClient.Entity
{
    /// <summary>
    /// Сущность основного игрока тикущего клиента
    /// </summary>
    public class EntityPlayerSP : EntityPlayerClient
    {
        /// <summary>
        /// Плавное перемещение угла обзора
        /// </summary>
        public SmoothFrame Fov { get; private set; }
        /// <summary>
        /// Плавное перемещение глаз, сел/встал
        /// </summary>
        public SmoothFrame Eye { get; private set; }
        /// <summary>
        /// массив матрицы перспективу камеры 3D
        /// </summary>
        public float[] Projection { get; private set; }
        /// <summary>
        /// массив матрицы расположения камеры в пространстве
        /// </summary>
        public float[] LookAt { get; private set; }
        /// <summary>
        /// Вектор луча
        /// </summary>
        public vec3 RayLook { get; private set; }

        /// <summary>
        /// Принудительно обработать FrustumCulling
        /// </summary>
        public bool IsFrustumCulling { get; private set; } = false;
        /// <summary>
        /// Объект расчёта FrustumCulling
        /// </summary>
        public Frustum FrustumCulling { get; private set; } = new Frustum();
        /// <summary>
        /// Массив чанков которые попадают под FrustumCulling для рендера
        /// </summary>
        public FrustumStruct[] ChunkFC { get; private set; } = new FrustumStruct[0];
        /// <summary>
        /// Список сущностей которые попали в луч
        /// </summary>
        public EntityPlayerMP[] EntitiesLook { get; private set; } = new EntityPlayerMP[0];
        /// <summary>
        /// Вид камеры
        /// </summary>
        public EnumViewCamera ViewCamera { get; private set; } = EnumViewCamera.Eye;
        /// <summary>
        /// Последнее значение поворота вокруг своей оси
        /// </summary>
        public float RotationYawLast { get; private set; }
        /// <summary>
        /// Последнее значение поворота вверх вниз
        /// </summary>
        public float RotationPitchLast { get; private set; }
        /// <summary>
        /// Выбранный блок
        /// </summary>
        public BlockBase SelectBlock { get; private set; } = null;
        /// <summary>
        /// Позиция камеры в блоке для альфа, в зависимости от вида (с глаз, с зади, спереди)
        /// </summary>
        public vec3i PositionAlphaBlock { get; private set; }
        /// <summary>
        /// Позиция камеры
        /// </summary>
        public vec3 PositionCamera { get; private set; }

        /// <summary>
        /// Массив по длинам используя квадратный корень для всей видимости в объёме для обновление чанков в объёме
        /// </summary>
        private vec3i[] distSqrtAlpha;
        /// <summary>
        /// Позиция камеры в чанке для альфа, в зависимости от вида (с глаз, с зади, спереди)
        /// </summary>
        private vec3i positionAlphaChunk;
        /// <summary>
        /// Позиция когда был запрос рендера для альфа блоков для малого смещения, в чанке
        /// </summary>
        private vec3i positionAlphaBlockPrev;
        /// <summary>
        /// Позиция когда был запрос рендера для альфа блоков для большого смещения, за пределами чанка
        /// </summary>
        private vec3i positionAlphaChunkPrev;
        /// <summary>
        /// Позиция с учётом итерполяции кадра
        /// </summary>
        private vec3 positionFrame;
        /// <summary>
        /// Поворот Pitch с учётом итерполяции кадра
        /// </summary>
        private float pitchFrame;
        /// <summary>
        /// Поворот Yaw головы с учётом итерполяции кадра
        /// </summary>
        private float yawHeadFrame;
        /// <summary>
        /// Поворот Yaq тела с учётом итерполяции кадра
        /// </summary>
        private float yawBodyFrame;
        /// <summary>
        /// Высота глаз с учётом итерполяции кадра
        /// </summary>
        private float eyeFrame;

        /// <summary>
        /// Лист DisplayList матрицы 
        /// </summary>
        private uint dListLookAt;
        /// <summary>
        /// массив векторов расположения камеры в пространстве для DisplayList
        /// </summary>
        private vec3[] lookAtDL;

        /// <summary>
        /// Счётчик паузы анимации левого удара, такты
        /// </summary>
        private int leftClickPauseCounter = 0;
        /// <summary>
        /// Холостой удар
        /// </summary>
        private bool blankShot = false;
        /// <summary>
        /// Активна ли рука в действии
        /// 0 - не активна, 1 - активна левая, 2 - активна правая
        /// </summary>
        private ActionHand handAction = ActionHand.None;
        /// <summary>
        /// Объект работы над игроком разрушения блока, итомы и прочее
        /// </summary>
        private ItemInWorldManager itemInWorldManager;
        /// <summary>
        /// Сущность по которой ударил игрок, если null то нет атаки
        /// </summary>
        private EntityBase entityAtack;
        /// <summary>
        /// Выбранная ячейка
        /// </summary>
        private int currentPlayerItem = 0;

        public EntityPlayerSP(WorldClient world) : base(world)
        {
            Fov = new SmoothFrame(1.43f);
            Eye = new SmoothFrame(GetEyeHeight());
            itemInWorldManager = new ItemInWorldManager(world, this);
        }

        /// <summary>
        /// Возвращает true, если эта вещь названа
        /// </summary>
        public override bool HasCustomName() => false;

        #region DisplayList

        /// <summary>
        /// Обновить матрицу
        /// </summary>
        private void UpMatrixProjection()
        {
            if (lookAtDL != null && lookAtDL.Length == 3)
            {
                GLRender.ListDelete(dListLookAt);
                dListLookAt = GLRender.ListBegin();
                GLWindow.gl.Viewport(0, 0, GLWindow.WindowWidth, GLWindow.WindowHeight);

                GLWindow.gl.MatrixMode(OpenGL.GL_PROJECTION);
                GLWindow.gl.LoadIdentity();
                GLWindow.gl.Perspective(glm.degrees(Fov.ValueFrame), (float)GLWindow.WindowWidth / (float)GLWindow.WindowHeight, 0.001f, OverviewChunk * 22.624f * 2f);
                //GLWindow.gl.LookAt(lookAtDL[0].x, lookAtDL[0].y, lookAtDL[0].z,
                //    lookAtDL[1].x, lookAtDL[1].y, lookAtDL[1].z,
                //    lookAtDL[2].x, lookAtDL[2].y, lookAtDL[2].z);
                GLWindow.gl.MatrixMode(OpenGL.GL_MODELVIEW);
                GLWindow.gl.LoadIdentity();

                GLWindow.gl.LookAt(lookAtDL[0].x, lookAtDL[0].y, lookAtDL[0].z,
                    lookAtDL[1].x, lookAtDL[1].y, lookAtDL[1].z,
                    lookAtDL[2].x, lookAtDL[2].y, lookAtDL[2].z);

                // Код с фиксированной функцией может использовать альфа-тестирование
                // Чтоб корректно прорисовывался кактус
                GLWindow.gl.AlphaFunc(OpenGL.GL_GREATER, 0.1f);
                GLWindow.gl.Enable(OpenGL.GL_ALPHA_TEST);
                //GLWindow.gl.Enable(OpenGL.GL_TEXTURE_2D);
                //GLWindow.gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                GLRender.ListEnd();
            }
        }

        /// <summary>
        /// Прорисовать матрицу для DisplayList
        /// </summary>
        public void CameraMatrixProjection() => GLRender.ListCall(dListLookAt);

        #endregion

        /// <summary>
        /// Надо ли обрабатывать LivingUpdate, для мобов на сервере, и игроки у себя
        /// </summary>
        protected override bool IsLivingUpdate() => true;

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            // Такты изминения глаз при присидании, и угол обзора при ускорении. Должны быть до base.Update()
            Eye.Update();
            Fov.Update();
            LastTickPos = PositionPrev = Position;
            RotationPitchPrev = RotationPitch;
            RotationYawPrev = RotationYaw;
            RotationYawHeadPrev = RotationYawHead;

            base.Update();

            // Обновление курсоро, не зависимо от действия игрока, так как рядом может быть изминение
            UpCursor();

            // Проверка на обновление чанков альфа блоков, в такте после перемещения
            UpdateChunkRenderAlphe();

            if (RotationEquals())
            {
                IsFrustumCulling = true;
            }
            if (ActionChanged != EnumActionChanged.None)
            {
                IsFrustumCulling = true;
                UpdateActionPacket();
            }

            // счётчик паузы для анимации удара
            leftClickPauseCounter++;

            // обновляем счётчик удара
            ItemInWorldManager.StatusAnimation statusUpdate = itemInWorldManager.UpdateBlock();
            if (statusUpdate == ItemInWorldManager.StatusAnimation.Animation)
            {
                leftClickPauseCounter = 100;
            }
            else if (statusUpdate == ItemInWorldManager.StatusAnimation.NotAnimation)
            {
                leftClickPauseCounter = 0;
            }

            if (!AtackUpdate())
            {
                // Если нет атаки то проверяем установку или разрушение блока
                if (handAction != ActionHand.None)
                {
                    HandActionUpdate();
                    if (leftClickPauseCounter > 5 || blankShot)
                    {
                        SwingItem();
                        blankShot = false;
                        leftClickPauseCounter = 0;
                    }
                }
            }

            // синхронизация выброного слота
            SyncCurrentPlayItem();

            // Скрыть прорисовку себя если вид с глаз
            Type = Health > 0 && ViewCamera == EnumViewCamera.Eye ? EnumEntities.PlayerHand : EnumEntities.Player;
            //Type = EnumEntities.PlayerHand;
        }

        /// <summary>
        /// Скорость перемещения
        /// </summary>
        protected override float GetAIMoveSpeed(float strafe, float forward)
        {
            bool isSneaking = IsSneaking();
            float speed = Mth.Max(Speed.Strafe * Mth.Abs(strafe), Speed.Forward * Mth.Abs(forward));
            if (IsSprinting() && forward < 0 && !isSneaking)
            {
                // Бег 
                speed *= Speed.Sprinting;
            }
            else if (!IsFlying && (forward != 0 || strafe != 0) && isSneaking)
            {
                // Крадёмся
                speed *= Speed.Sneaking;
            }
            return speed;
        }

        /// <summary>
        /// Удар рукой по сущности в такте игры
        /// </summary>
        private bool AtackUpdate()
        {
            if (entityAtack != null)
            {
                if (!entityAtack.IsDead)
                {
                    SwingItem();

                    vec3 pos = entityAtack.Position + new vec3(.5f);
                    for (int i = 0; i < 5; i++)
                    {
                        ClientWorld.SpawnParticle(EnumParticle.Test, pos + new vec3((rand.Next(16) - 8) / 16f, (rand.Next(12) - 6) / 16f, (rand.Next(16) - 8) / 16f), new vec3(0));
                    }
                    ClientWorld.ClientMain.TrancivePacket(new PacketC03UseEntity(entityAtack.Id, entityAtack.Position - Position));
                    entityAtack = null;
                    return true;
                }
                entityAtack = null;
            }
            return false;
        }

        /// <summary>
        /// Размахивает предметом, который держит игрок
        /// </summary>
        public override void SwingItem()
        {
            base.SwingItem();
            ClientWorld.ClientMain.TrancivePacket(new PacketC0AAnimation());
        }

        /// <summary>
        /// Обновление в каждом тике, если были требования по изминению позицыи, вращения, бег, сидеть и тп.
        /// </summary>
        private void UpdateActionPacket()
        {
            bool isSS = false;
            if (ActionChanged.HasFlag(EnumActionChanged.IsSneaking))
            {
                Eye.Set(GetEyeHeight(), 4);
                isSS = true;
            }
            if (ActionChanged.HasFlag(EnumActionChanged.IsSprinting))
            {
                Fov.Set(IsSprinting() ? 1.22f : 1.43f, 4);
                isSS = true;
            }

            bool isPos = ActionChanged.HasFlag(EnumActionChanged.Position);
            bool isLook = ActionChanged.HasFlag(EnumActionChanged.Look);

            if (isPos && isLook)
            {
                ClientWorld.ClientMain.TrancivePacket(new PacketC06PlayerPosLook(
                    Position, RotationYawHead, RotationPitch, IsSneaking(), IsSprinting()
                ));
            }
            else if (isLook)
            {
                ClientWorld.ClientMain.TrancivePacket(new PacketC05PlayerLook(RotationYawHead, RotationPitch, IsSneaking()));
            }
            else if (isPos || isSS)
            {
                ClientWorld.ClientMain.TrancivePacket(new PacketC04PlayerPosition(Position, IsSneaking(), IsSprinting()));
            }
            ActionNone();
        }

        /// <summary>
        /// Проверка изменения вращения
        /// </summary>
        /// <returns>True - разные значения, надо InitFrustumCulling</returns>
        public bool RotationEquals()
        {
            if (RotationPitchLast != RotationPitch || RotationYawLast != RotationYawHead || RotationYaw != RotationYawPrev)
            {
                SetRotationHead(RotationYawLast, RotationPitchLast);
                return true;
            }
            return false;
        }

        public void MouseMove(float deltaX, float deltaY)
        {
            // Чувствительность мыши
            float speedMouse = 2f;// 1.5f;

            if (deltaX == 0 && deltaY == 0) return;
            float pitch = RotationPitchLast - deltaY / (float)GLWindow.WindowHeight * speedMouse;
            float yaw = RotationYawLast + deltaX / (float)GLWindow.WindowWidth * speedMouse;

            if (pitch < -glm.radians(89.0f)) pitch = -glm.radians(89.0f);
            if (pitch > glm.radians(89.0f)) pitch = glm.radians(89.0f);
            if (yaw > glm.pi) yaw -= glm.pi360;
            if (yaw < -glm.pi) yaw += glm.pi360;

            RotationYawLast = yaw;
            RotationPitchLast = pitch;

            UpCursor();
        }

        /// <summary>
        /// Обновить курсор
        /// </summary>
        private void UpCursor()
        {
            MovingObjectPosition moving = RayCast();
            SelectBlock = moving.Block;
            if (moving.IsBlock())
            {
                ChunkBase chunk = World.GetChunk(moving.Block.Position.GetPositionChunk());
                vec3i pos = moving.Block.Position.GetPosition0();
                string s1 = ToBlockDebug(chunk, new vec3i(pos.x, pos.y + 1, pos.z));
                string s2 = ToBlockDebug(chunk, new vec3i(pos.x, pos.y + 2, pos.z));
                Debug.BlockFocus = string.Format(
                    "Block: {0} Light: {1} {2} H:{3}|{4} #{6}\r\n",
                    moving.Block,
                    s1, s2,
                    "*",//chunk.Light.GetHeight(moving.Block.Position.X & 15, moving.Block.Position.Z & 15),
                        //chunk.Light.HeightMapMax
                    chunk.GetTopFilledSegment(),
                    "",
                    chunk.GetDebugAllSegment()
                );
                //Debug.DStr = "";
            } else
            {
               // Debug.DStr = moving.ToString();
                Debug.BlockFocus = "";
            }
        }


        private string ToBlockDebug(ChunkBase chunk, vec3i pos)
        {
            ChunkStorage chunkStorage = chunk.StorageArrays[pos.y >> 4];
            if (!chunkStorage.IsEmpty())
            {
                int ls = chunkStorage.GetLightFor(pos.x, pos.y & 15, pos.z, EnumSkyBlock.Sky);
                int lb = chunkStorage.GetLightFor(pos.x, pos.y & 15, pos.z, EnumSkyBlock.Block);
                int cb = chunkStorage.countVoxel;
                return string.Format("s{0} b{1} c{2}", ls, lb, cb);
            }
            return "@-@";
        }
        /// <summary>
        /// Обновить матрицу камеры
        /// </summary>
        public bool UpLookAt(float timeIndex)
        {
            vec3 pos = new vec3(0, GetEyeHeightFrame(), 0);
            vec3 front = GetLookFrame(timeIndex).normalize();
            vec3 up = new vec3(0, 1, 0);

            if (ViewCamera == EnumViewCamera.Back)
            {
                // вид сзади
                pos = GetPositionCamera(pos, front * -1f, MvkGlobal.CAMERA_DIST);
            } else if (ViewCamera == EnumViewCamera.Front)
            {
                // вид спереди
                pos = GetPositionCamera(pos, front, MvkGlobal.CAMERA_DIST);
                front *= -1f;
            } else
            {
                //vec3 right = glm.cross(up, front).normalize();
                //vec3 f2 = (front * -1f - right * .7f).normalize();
                //pos = GetPositionCamera(pos, f2, 2f);
            }

            if (!IsFlying && MvkGlobal.WIGGLE_EFFECT)
            {
                // Эффект болтания когда игрок движется 
                vec3 right = glm.cross(up, front).normalize();
                float limbS = GetLimbSwingAmountFrame(timeIndex) * .12f;
                // эффект лево-право
                float limb = glm.cos(LimbSwing * 0.3331f) * limbS;
                pos += right * limb;
                up = glm.cross(front, right);
                // эффект вверх-вниз
                limb = glm.cos(LimbSwing * 0.6662f) * limbS;
                pos += up * limb;
            }
            float[] lookAt = glm.lookAt(pos, pos + front, up).to_array();
            if (!Mth.EqualsArrayFloat(lookAt, LookAt, 0.00001f))
            {
                PositionCamera = pos;
                LookAt = lookAt;
                RayLook = front;
                lookAtDL = new vec3[] { pos, pos + front, up };
                UpMatrixProjection();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Обновить перспективу камеры
        /// </summary>
        public override void UpProjection()
        {
            Projection = glm.perspective(Fov.ValueFrame, (float)GLWindow.WindowWidth / (float)GLWindow.WindowHeight, 0.001f, OverviewChunk * 22.624f * 2f).to_array();
            UpMatrixProjection();
        }

        /// <summary>
        /// Обновление в кадре
        /// </summary>
        public void UpdateFrame(float timeIndex)
        {
            // Меняем положения глаз
            Eye.UpdateFrame(timeIndex);
            eyeFrame = Eye.ValueFrame;

            positionFrame = base.GetPositionFrame(timeIndex);
            yawBodyFrame = GetRotationYawBodyFrame(timeIndex);
            yawHeadFrame = GetRotationYawFrame(timeIndex);
            pitchFrame = GetRotationPitchFrame(timeIndex);

            // Меняем угол обзора (как правило при изменении скорости)
            if (Fov.UpdateFrame(timeIndex)) { }
            UpProjection();

            vec3 offset = new vec3(positionFrame.x, positionFrame.y + eyeFrame, positionFrame.z);
            ClientWorld.RenderEntityManager.SetCamera(positionFrame, yawHeadFrame, pitchFrame);

            // Изменяем матрицу глаз игрока
            if (UpLookAt(timeIndex) || IsFrustumCulling)
            {
                // Если имеется вращение камеры или было перемещение, то запускаем расчёт FrustumCulling
                InitFrustumCulling();
            }
        }

        /// <summary>
        /// Требуется перерасчёт FrustumCulling
        /// </summary>
        public void UpFrustumCulling() => IsFrustumCulling = true;

        /// <summary>
        /// Перерасчёт FrustumCulling
        /// </summary>
        public void InitFrustumCulling()
        {
            if (LookAt == null || Projection == null) return;
            FrustumCulling.Init(LookAt, Projection);
            
            Debug.DrawFrustumCulling.Clear();
            int countFC = 0;
            vec3i chunkPos = new vec3i(
                Mth.Floor(positionFrame.x) >> 4,
                Mth.Floor(positionFrame.y) >> 4,
                Mth.Floor(positionFrame.z) >> 4);
            
            List<FrustumStruct> listC = new List<FrustumStruct>();

            if (DistSqrt != null)
            {
                for (int i = 0; i < DistSqrt.Length; i++)
                {
                    int xc = DistSqrt[i].x;
                    int zc = DistSqrt[i].y;
                    int xb = xc << 4;
                    int zb = zc << 4;

                    int x1 = xb - 15;
                    int y1 = -255;
                    int z1 = zb - 15;
                    int x2 = xb + 15;
                    int y2 = 255;
                    int z2 = zb + 15;
                    if (FrustumCulling.IsBoxInFrustum(x1, y1, z1, x2, y2, z2))
                    {
                        vec2i coord = new vec2i(xc + chunkPos.x, zc + chunkPos.z);
                        ChunkRender chunk = ClientWorld.ChunkPrClient.GetChunkRender(coord);
                        FrustumStruct frustum;
                        if (chunk == null) frustum = new FrustumStruct(coord);
                        else frustum = new FrustumStruct(chunk);

                        int count = frustum.FrustumShow(FrustumCulling, x1, z1, x2, z2, Mth.Floor(positionFrame.y + eyeFrame));// positionFrame.y);
                        if (count > 0)
                        {
                            if (Debug.IsDrawFrustumCulling)
                            {
                                coord = new vec2i(xc, zc);
                                if (!Debug.DrawFrustumCulling.Contains(coord)) Debug.DrawFrustumCulling.Add(coord);
                            }
                            listC.Add(frustum);
                            countFC += count;
                        }
                    }
                }
            }
            Debug.CountMeshAll = countFC;
            Debug.RenderFrustumCulling = true;
            ChunkFC = listC.ToArray();
            IsFrustumCulling = false;
        }

        /// <summary>
        /// Проверить не догруженые чанки и догрузить если надо
        /// </summary>
        public void CheckChunkFrustumCulling()
        {
            for (int i = 0; i < ChunkFC.Length; i++)
            {
                FrustumStruct fs = ChunkFC[i];
                
                if (!fs.IsChunk())
                {
                    ChunkRender chunk = ClientWorld.ChunkPrClient.GetChunkRender(fs.GetCoord());
                    if (chunk != null)
                    {
                        ChunkFC[i] = new FrustumStruct(chunk, fs.GetSortList());
                    }
                }
            }
        }

        /// <summary>
        /// Следующий вид камеры
        /// </summary>
        public void ViewCameraNext()
        {
            int count = Enum.GetValues(typeof(EnumViewCamera)).Length - 1;
            int value = (int)ViewCamera;
            value++;
            if (value > count) value = 0;
            ViewCamera = (EnumViewCamera)value;
        }

        /// <summary>
        /// Занести массив сущностей попадающих в луч
        /// </summary>
        public void SetEntitiesLook(List<EntityPlayerMP> entities) => EntitiesLook = entities.ToArray();

        /// <summary>
        /// Определить положение камеры, при виде сзади и спереди, проверка RayCast
        /// </summary>
        /// <param name="pos">позиция глаз</param>
        /// <param name="vec">направляющий вектор к расположению камеры</param>
        private vec3 GetPositionCamera(vec3 pos, vec3 vec, float dis)
        {
            vec3 offset = ClientWorld.RenderEntityManager.CameraOffset;
            MovingObjectPosition moving = World.RayCastBlock(pos + offset, vec, dis);
            return pos + vec * (moving.IsBlock() ? glm.distance(pos, moving.RayHit + new vec3(moving.Norm) * .5f - offset) : dis);
        }

        /// <summary>
        /// Получить объект по тикущему лучу
        /// </summary>
        private MovingObjectPosition RayCast()
        {
            // максимальная дистанция луча
            float maxDis = MvkGlobal.RAY_CAST_DISTANCE;
            vec3 pos = GetPositionFrame();
            pos.y += GetEyeHeightFrame();
            vec3 dir = RayLook;
            vec3 offset = ClientWorld.RenderEntityManager.CameraOffset;
            MovingObjectPosition movingObjectBlock = World.RayCastBlock(pos, dir, maxDis);

            MapListEntity listEntity = World.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityLiving, BoundingBox.AddCoordBias(dir * maxDis), Id);
            float entityDis = movingObjectBlock.IsBlock() ? glm.distance(pos, movingObjectBlock.RayHit) : maxDis;
            EntityBase pointedEntity = null;
            float step = .5f;

            while(listEntity.Count > 0)
            {
                EntityBase entity = listEntity.FirstRemove();

                if (entity.CanBeCollidedWith())
                {
                    float size = entity.GetCollisionBorderSize();
                    AxisAlignedBB aabb = entity.BoundingBox.Expand(new vec3(size));
                    for (float addStep = 0; addStep <= entityDis; addStep += step)
                    {
                        if (aabb.IsVecInside(pos + dir * addStep))
                        {
                            pointedEntity = entity;
                            entityDis = addStep;
                            break;
                        }
                    }
                }
            }
            return pointedEntity != null ? new MovingObjectPosition(pointedEntity) : movingObjectBlock;
        }

        /// <summary>
        /// Действие правой рукой
        /// </summary>
        private void HandActionUpdate()
        {
            MovingObjectPosition moving = RayCast();

            if (handAction == ActionHand.Left)
            {
                // Разрушаем блок
                if (itemInWorldManager.IsDestroyingBlock && ((moving.IsBlock() && !itemInWorldManager.BlockPosDestroy.ToVec3i().Equals(moving.Block.Position.ToVec3i())) || !moving.IsBlock()))
                {
                    // Отмена разбитие блока, сменился блок
                    ClientMain.TrancivePacket(new PacketC07PlayerDigging(itemInWorldManager.BlockPosDestroy, PacketC07PlayerDigging.EnumDigging.About));
                    itemInWorldManager.DestroyAbout();
                }
                else if (itemInWorldManager.IsDestroyingBlock)
                {
                    // Этап разбития блока
                    if (itemInWorldManager.IsDestroy())
                    {
                        // Стоп, окончено разбитие
                        ClientMain.TrancivePacket(new PacketC07PlayerDigging(itemInWorldManager.BlockPosDestroy, PacketC07PlayerDigging.EnumDigging.Stop));
                        itemInWorldManager.DestroyStop();
                    }
                }
                else if (itemInWorldManager.NotPauseUpdate)
                {
                    DestroyingBlockStart(moving, false);
                }
            }
            else if (handAction == ActionHand.Right && itemInWorldManager.NotPauseUpdate)
            {
                // устанавливаем блок
                PutBlockStart(moving, false);
            }
        }

        /// <summary>
        /// Действие правой рукой
        /// </summary>
        public void HandAction()
        {
            MovingObjectPosition moving = RayCast();
            if (moving.IsEntity())
            {
                // Курсор попал на сущность
                entityAtack = moving.Entity;
                handAction = ActionHand.None;
            }
            else
            {
                DestroyingBlockStart(moving, true);
                handAction = ActionHand.Left;
            }
        }

        /// <summary>
        /// Действие правй рукой, ставим блок
        /// </summary>
        public void HandActionTwo()
        {
            MovingObjectPosition moving = RayCast();
            PutBlockStart(moving, true);
        }

        private void PutBlockStart(MovingObjectPosition moving, bool start)
        {
            ItemStack itemStack = Inventory.GetCurrentItem();
            if (itemStack != null)
            {
                if (itemStack.Item is ItemBlock itemBlock && moving.IsBlock())
                {
                    // В стаке блок, и по лучу можем устанавливать блок
                    BlockPos blockPos = new BlockPos(moving.Put);
                    vec3 facing = moving.RayHit - new vec3(moving.Put);
                    if (itemBlock.ItemUse(itemStack, this, World, blockPos, new EnumFacing(), facing))
                    {
                        ClientMain.TrancivePacket(new PacketC08PlayerBlockPlacement(blockPos, facing));
                        itemInWorldManager.Put(blockPos, facing, Inventory.CurrentItem);
                        itemInWorldManager.PutPause(start);
                    }
                    else
                    {
                        itemInWorldManager.PutAbout();
                    }
                    handAction = ActionHand.Right;
                }
                // Тут будут другие действия на предмет, к примеру покушать
            }
        }

        /// <summary>
        /// Начало разрушения блока
        /// </summary>
        private void DestroyingBlockStart(MovingObjectPosition moving, bool start)
        {
            if (!itemInWorldManager.IsDestroyingBlock && moving.IsBlock())
            {
                // Начало разбитие блока
                ClientWorld.ClientMain.TrancivePacket(new PacketC07PlayerDigging(moving.Block.Position, PacketC07PlayerDigging.EnumDigging.Start));
                itemInWorldManager.DestroyStart(moving.Block.Position);
            }
            else if (start)
            {
                blankShot = true;
            }
        }

        /// <summary>
        /// Отмена действия правой рукой
        /// </summary>
        public void UndoHandAction()
        {
            if (itemInWorldManager.IsDestroyingBlock)
            {
                ClientMain.TrancivePacket(new PacketC07PlayerDigging(itemInWorldManager.BlockPosDestroy, PacketC07PlayerDigging.EnumDigging.About));
                itemInWorldManager.DestroyAbout();
            }
            handAction = ActionHand.None;
        }

        /// <summary>
        /// Нет нажатий
        /// </summary>
        public override void InputNone()
        {
            base.InputNone();
            UndoHandAction();
        }

        public override void Respawn()
        {
            base.Respawn();
            ClientWorld.ClientMain.GameMode();
        }

        /// <summary>
        /// Падение
        /// </summary>
        protected override void Fall()
        {
            if (fallDistanceResult > .0001f)
            {
                ClientWorld.ClientMain.TrancivePacket(new PacketC0CPlayerAction(PacketC0CPlayerAction.EnumAction.Fall, fallDistanceResult));
                ParticleFall(fallDistanceResult);
                fallDistanceResult = 0;
            }
        }

        /// <summary>
        /// Задать место положение игрока, при спавне, телепорте и тп
        /// </summary>
        public override void SetPosLook(vec3 pos, float yaw, float pitch)
        {
            base.SetPosLook(pos, yaw, pitch);
            RotationYawLast = RotationYaw;
            RotationPitchLast = RotationPitch;
        }

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public override void SetOverviewChunk(int overviewChunk)
        {
            OverviewChunkPrev = OverviewChunk = overviewChunk;
            DistSqrt = MvkStatic.GetSqrt(OverviewChunk);
            distSqrtAlpha = MvkStatic.GetSqrt3d(OverviewChunk < MvkGlobal.UPDATE_ALPHE_CHUNK ? OverviewChunk : MvkGlobal.UPDATE_ALPHE_CHUNK);
        }

        /// <summary>
        /// Проверить изменение слота если изменён, отправить на сервер
        /// </summary>
        private void SyncCurrentPlayItem()
        {
            int currentItem = Inventory.CurrentItem;
            if (currentItem != currentPlayerItem)
            {
                currentPlayerItem = currentItem;
                ClientMain.TrancivePacket(new PacketC09HeldItemChange(currentPlayerItem));
            }
        }

        /// <summary>
        /// Проверка на обновление чанков альфа блоков, в такте после перемещения
        /// </summary>
        private void UpdateChunkRenderAlphe()
        {
            PositionAlphaBlock = new vec3i(Position + PositionCamera);
            positionAlphaChunk = new vec3i(PositionAlphaBlock.x >> 4, PositionAlphaBlock.y >> 4, PositionAlphaBlock.z >> 4);

            if (!positionAlphaChunk.Equals(positionAlphaChunkPrev))
            {
                // Если смещение чанком
                positionAlphaChunkPrev = positionAlphaChunk;
                positionAlphaBlockPrev = PositionAlphaBlock;
                vec2i posCh = GetChunkPos();
                int posY = GetChunkY();
                for (int d = 0; d < distSqrtAlpha.Length; d++)
                {
                    vec2i pos = new vec2i(posCh.x + distSqrtAlpha[d].x, posCh.y + distSqrtAlpha[d].z);
                    ChunkRender chunk = ClientWorld.ChunkPrClient.GetChunkRender(pos);
                    if (chunk != null) chunk.ModifiedToRenderAlpha(posY + distSqrtAlpha[d].y);
                }
            }
            else if (!PositionAlphaBlock.Equals(positionAlphaBlockPrev))
            {
                // Если смещение блока
                positionAlphaBlockPrev = PositionAlphaBlock;
                vec2i posCh = GetChunkPos();
                ClientWorld.ChunkPrClient.ModifiedToRenderAlpha(posCh.x, GetChunkY(), posCh.y);
            }
        }

        #region Frame

        /// <summary>
        /// Высота глаз для кадра
        /// </summary>
        public override float GetEyeHeightFrame() => eyeFrame;

        /// <summary>
        /// Получить позицию сущности для кадра
        /// </summary>
        public vec3 GetPositionFrame() => positionFrame;

        /// <summary>
        /// Получить вектор направления камеры тела для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public override vec3 GetLookBodyFrame(float timeIndex) => GetRay(yawBodyFrame, pitchFrame);

        /// <summary>
        /// Получить вектор направления камеры от головы для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public override vec3 GetLookFrame(float timeIndex) => GetRay(yawHeadFrame, pitchFrame);

        #endregion


        /// <summary>
        /// Действие рук
        /// </summary>
        private enum ActionHand
        {
            /// <summary>
            /// Нет действий
            /// </summary>
            None,
            /// <summary>
            /// Левой рукой
            /// </summary>
            Left,
            /// <summary>
            /// Правой рукой
            /// </summary>
            Right
        }
    }
}
