using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Util;
using MvkClient.World;
using MvkServer;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public SmoothFrame Fov { get; protected set; }
        /// <summary>
        /// Плавное перемещение глаз, сел/встал
        /// </summary>
        public SmoothFrame Eye { get; protected set; }
        /// <summary>
        /// массив матрицы перспективу камеры 3D
        /// </summary>
        public float[] Projection { get; protected set; }
        /// <summary>
        /// массив матрицы расположения камеры в пространстве
        /// </summary>
        public float[] LookAt { get; protected set; }
        /// <summary>
        /// Принудительно обработать FrustumCulling
        /// </summary>
        public bool IsFrustumCulling { get; protected set; } = false;
        /// <summary>
        /// Массив чанков которые попадают под FrustumCulling для рендера
        /// </summary>
        public ChunkRender[] ChunkFC { get; protected set; } = new ChunkRender[0];
        /// <summary>
        /// Вид камеры
        /// </summary>
        public EnumViewCamera ViewCamera { get; protected set; } = EnumViewCamera.Eye;

        protected vec3 positionFrame;
        protected float pitchFrame;
        protected float yawHeadFrame;
        protected float yawBodyFrame;
        protected float eyeFrame;

        public EntityPlayerSP(WorldClient world) : base(world)
        {
            Fov = new SmoothFrame(1.22f);
            Eye = new SmoothFrame(GetEyeHeight());
            interpolation.Start();
        }

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            base.Update();
            if (RotationEquals()) IsFrustumCulling = true;
            if (isMotionServer)
            {
                IsFrustumCulling = true;
                isMotionServer = false;
            }
            Eye.Update();
            Fov.Update();
            // Расчёт амплитуды конечностей, при движении
            UpLimbSwing();

            // Если имеется вращение камеры или было перемещение, то запускаем расчёт FrustumCulling
            //if (RotationEquals() || isMotionServer)
            //{
            //    isMotionServer = false;
            //    InitFrustumCulling();
            //}
        }

        /// <summary>
        /// Обновление в каждом тике, если были требования по изминению позицыи, вращения, бег, сидеть и тп.
        /// </summary>
        protected override void UpdateLiving()
        {
            base.UpdateLiving();
            Eye.Set(GetEyeHeight(), 4);
            Fov.Set(IsSprinting ? 1.22f : 1.43f, 4);
            ClientWorld.ClientMain.TrancivePacket(new PacketB20Player().Position(Position, IsSneaking));
        }

        /// <summary>
        /// Проверка изменения вращения
        /// </summary>
        /// <returns>True - разные значения, надо InitFrustumCulling</returns>
        public bool RotationEquals()
        {
            if (RotationPitchLast != RotationPitch || RotationYawLast != RotationYawHead
                || RotationYaw != RotationYawPrev)
            {
                //RotationYaw = RotationYawLast;
                //RotationPitch = RotationPitchLast;
                //SetRotation(RotationYawLast, RotationPitchLast);
                SetRotationHead(RotationYawLast, RotationYaw, RotationPitchLast);
                float yawHead = RotationYawHead;
                float yawBody = RotationYaw;
                float pitch = RotationPitch;
                Task.Factory.StartNew(() =>
                {
                    ClientWorld.ClientMain.TrancivePacket(new PacketB20Player().YawPitch(yawHead, yawBody, pitch));
                });
                return true;
            }
            return false;
        }

        public void MouseMove(float deltaX, float deltaY)
        {
            // Чувствительность мыши
            float speedMouse = 1.5f;

            if (deltaX == 0 && deltaY == 0) return;
            float pitch = RotationPitchLast - deltaY / (float)GLWindow.WindowHeight * speedMouse;
            float yaw = RotationYawLast + deltaX / (float)GLWindow.WindowWidth * speedMouse;

            if (pitch < -glm.radians(89.0f)) pitch = -glm.radians(89.0f);
            if (pitch > glm.radians(89.0f)) pitch = glm.radians(89.0f);
            if (yaw > glm.pi) yaw -= glm.pi360;
            if (yaw < -glm.pi) yaw += glm.pi360;

            RotationYawLast = yaw;
            RotationPitchLast = pitch;
        }

        

        /// <summary>
        /// Обновить матрицу камеры
        /// </summary>
        public bool UpLookAt(float timeIndex)
        {
            vec3 pos = GetPositionEyeFrame(timeIndex);
            vec3 front = GetLookFrame(timeIndex).normalize();

            if (ViewCamera == EnumViewCamera.Back)
            {
                // вид сзади, но надо check на кализию камеры
                pos = GetPositionCamera(pos, front * -1f);
            } else if (ViewCamera == EnumViewCamera.Front)
            {
                // вид сзади, но надо check на кализию камеры
                pos = GetPositionCamera(pos, front);
                front *= -1f;
            }

            vec3 up = new vec3(0, 1, 0);
            if (!IsFlying && MvkGlobal.WIGGLE_EFFECT)
            {
                // Эффект болтания когда игрок движется 
                vec3 right = glm.cross(up, front).normalize();
                // эффект лево-право
                float limb = glm.cos(LimbSwing * 0.3331f) * GetLimbSwingAmountFrame(timeIndex);
                pos += right * limb * .12f;
                up = glm.cross(front, right);
                // эффект вверх-вниз
                pos += up * Mth.Abs(limb * .2f);
            }
            float[] lookAt = glm.lookAt(pos, front, up).to_array();
            if (!Mth.EqualsArrayFloat(lookAt, LookAt, 0.00001f))
            {
                LookAt = lookAt;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Обновить перспективу камеры
        /// </summary>
        public void UpProjection()
            => Projection = glm.perspective(Fov.ValueFrame, (float)GLWindow.WindowWidth / (float)GLWindow.WindowHeight, 0.001f, OverviewChunk * 22.624f * 2f).to_array();


        /// <summary>
        /// Обновление в кадре
        /// </summary>
        public void UpdateFrame(float timeIndex)
        {
            // Меняем угол обзора (как правило при изменении скорости)
            if (Fov.UpdateFrame(timeIndex)) UpProjection();
            // Меняем положения глаз
            Eye.UpdateFrame(timeIndex);

            eyeFrame = Eye.ValueFrame;
            positionFrame = base.GetPositionFrame(timeIndex);
            yawBodyFrame = GetRotationYawBodyFrame(timeIndex);
            yawHeadFrame = GetRotationYawFrame(timeIndex);
            pitchFrame = GetRotationPitchFrame(timeIndex);


            // Изменяем матрицу глаз игрока
            if (UpLookAt(timeIndex))// || IsFrustumCulling)
            {
                // Если имеется вращение камеры или было перемещение, то запускаем расчёт FrustumCulling
                //if (IsFrustumCulling)
                InitFrustumCulling();
            }
        }

        /// <summary>
        /// Требуется перерасчёт FrustumCulling
        /// </summary>
        public void FrustumCulling() => IsFrustumCulling = true;

        /// <summary>
        /// Перерасчёт FrustumCulling
        /// </summary>
        public void InitFrustumCulling()
        {
            if (LookAt == null || Projection == null) return;

            Frustum frustum = new Frustum();
            frustum.Init(LookAt, Projection);

            int countAll = 0;
            int countFC = 0;
            vec2i chunkPos = GetChunkPos();
            List<ChunkRender> listC = new List<ChunkRender>();

            for (int i = 0; i < DistSqrt.Length; i++)
            {
                int xc = DistSqrt[i].x + chunkPos.x;
                int zc = DistSqrt[i].y + chunkPos.y;
                int xb = xc << 4;
                int zb = zc << 4;

                if (frustum.IsBoxInFrustum(xb, 0, zb, xb + 15, 255, zb + 15))
                {
                    countFC++;
                    listC.Add(ClientWorld.ChunkPrClient.GetChunkRender(new vec2i(xc, zc), true));
                }
                countAll++;
            }
            ChunkFC = listC.ToArray();
            IsFrustumCulling = false;
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
        /// Определить положение камеры, при виде сзади и спереди, проверка RayCast
        /// </summary>
        /// <param name="pos">позиция глаз</param>
        /// <param name="vec">направляющий вектор к расположению камеры</param>
        protected vec3 GetPositionCamera(vec3 pos, vec3 vec)
        {
            MovingObjectPosition moving = World.RayCast(pos, vec, MvkGlobal.CAMERA_DIST);
            return pos + vec * (moving.IsBlock() ? glm.distance(pos, new vec3(moving.Put) + new vec3(.5f)) : MvkGlobal.CAMERA_DIST);
        }

        #region Frame

        /// <summary>
        /// Высота глаз для кадра
        /// </summary>
        public override float GetEyeHeightFrame() => eyeFrame;

        /// <summary>
        /// Получить позицию сущности для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        public override vec3 GetPositionFrame(float timeIndex) => positionFrame;

        /// <summary>
        /// Получить вектор направления камеры тела
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public override vec3 GetLookBodyFrame(float timeIndex) => GetRay(yawBodyFrame, pitchFrame);

        /// <summary>
        /// Получить вектор направления камеры от головы
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public override vec3 GetLookFrame(float timeIndex) => GetRay(yawHeadFrame, pitchFrame);

        #endregion
    }
}
