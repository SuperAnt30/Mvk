using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Util;
using MvkClient.World;
using MvkServer;
using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.Util;
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

            if (isMotion)
            {
                isMotion = false;
                
                Eye.Set(GetEyeHeight(), 4);
                Fov.Set(IsSprinting ? 1.22f : 1.43f, 4);

                ClientWorld.ClientMain.TrancivePacket(new PacketB20Player().Position(Position, IsSneaking));
            }
            if (RotationEquals()) IsFrustumCulling = true;
            Eye.Update();
            Fov.Update();
        }

        /// <summary>
        /// Проверка изменения вращения
        /// </summary>
        /// <returns>True - разные значения, надо InitFrustumCulling</returns>
        public bool RotationEquals()
        {
            if (RotationPitchLast != RotationPitch || RotationYawLast != RotationYawHead)
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
        /// Отправить пакет действия клавиш управления
        /// </summary>
        public void KeyActionTrancivePacket(EnumKeyAction keyAction)
        {
            if (keyAction != EnumKeyAction.None)
            {
                Mov.Key(keyAction);
                //ClientWorld.ClientMain.TrancivePacket(new PacketC22Input(keyAction));
                //if (keyAction == EnumKeyAction.SprintingDown) Mov.Sprinting.Begin();
                //else if(keyAction == EnumKeyAction.SprintingUp) Mov.Sprinting.End();
            }
        }

        /// <summary>
        /// Высота глаз для кадра
        /// </summary>
        public override float GetEyeHeightFrame() => Eye.ValueFrame;

        /// <summary>
        /// Обновить матрицу камеры
        /// </summary>
        public void UpLookAt(float timeIndex)
        {
            vec3 pos = GetPositionEyeFrame(timeIndex);
            vec3 front = GetLookFrame(timeIndex).normalize();
            // pos = pos - front * 8f; // вид сзади, но надо check на кализию камеры
            vec3 up = new vec3(0, 1, 0);
            if (!IsFlying && MvkGlobal.WIGGLE_EFFECT)
            {
                // Эффект болтания когда игрок движется
                float limb = glm.cos(LimbSwing * 0.3331f) * 0.12f * GetLimbSwingAmountFrame(timeIndex);
                vec3 right = glm.cross(up, front).normalize();
                right.y = limb * -.05f;
                pos += right * limb;
                up = glm.cross(front, right);
            }
            LookAt = glm.lookAt(pos, pos + front, up).to_array();
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
            // Изменяем матрицу глаз игрока
            UpLookAt(timeIndex);
            // Если имеется вражение камеры или было перемещение, то запускаем расчёт FrustumCulling
            if (IsFrustumCulling) InitFrustumCulling();
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
    }
}
