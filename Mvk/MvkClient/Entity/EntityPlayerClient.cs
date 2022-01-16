using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Util;
using MvkClient.World;
using MvkServer.Entity;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MvkClient.Entity
{
    /// <summary>
    /// Сущность игрока для клиента
    /// </summary>
    public class EntityPlayerClient : EntityPlayer
    {
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient ClientWorld { get; protected set; }
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
        /// Плавное перемещение угла обзора
        /// </summary>
        public SmoothFrame Fov { get; protected set; }
        /// <summary>
        /// Плавное перемещение глаз, сел/встал
        /// </summary>
        public SmoothFrame Eye { get; protected set; }
        /// <summary>
        /// Позиция которая сейчас на экране 
        /// </summary>
        public vec3 PositionFrame { get; protected set; }
        /// <summary>
        /// Массив чанков которые попадают под FrustumCulling для рендера
        /// </summary>
        public ChunkRender[] ChunkFC { get; protected set; } = new ChunkRender[0];

        /// <summary>
        /// Объект времени c последнего тпс
        /// </summary>
        protected InterpolationTime interpolation = new InterpolationTime();

        public EntityPlayerClient(WorldClient world) : base()
        {
            ClientWorld = world;
            World = world;
            Fov = new SmoothFrame(1.22f);
            Eye = new SmoothFrame(GetEyeHeight());
            interpolation.Start();
        }

        /// <summary>
        /// Задать начальное положение игрока
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        public void SetPosBegin(vec3 pos, float yaw, float pitch)
        {
            SetPosition(pos);
            RotationYawLast = yaw;
            RotationPitchLast = pitch;
            PositionFrame = PositionPrev = pos;
            RotationEquals();
            IsFrustumCulling = true;
        }

        /// <summary>
        /// Обновить матрицу камеры
        /// </summary>
        public void UpLookAt()
        {
            mat4 rotation = new mat4(1.0f);
            rotation = glm.rotate(rotation, -RotationYaw, new vec3(0, 1, 0));
            rotation = glm.rotate(rotation, RotationPitch, new vec3(1, 0, 0));
            vec3 front = new vec3(rotation * new vec4(0, 0, -1, 1));
            //vec3 right = new vec3(rotation * new vec4(1, 0, 0, 1));
            vec3 up = new vec3(rotation * new vec4(0, 1, 0, 1));
            vec3 pos = new vec3(PositionFrame);
            pos.y += Eye.ValueFrame;
            LookAt = glm.lookAt(pos, pos + front, up).to_array();
        }
        
        /// <summary>
        /// Обновить перспективу камеры
        /// </summary>
        public void UpProjection() 
            => Projection = glm.perspective(Fov.ValueFrame, (float)GLWindow.WindowWidth / (float)GLWindow.WindowHeight, 0.001f, OverviewChunk * 22.624f * 2f).to_array();

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
        /// Проверка изменения вращения
        /// </summary>
        /// <returns>True - разные значения, надо InitFrustumCulling</returns>
        public bool RotationEquals()
        {
            if (RotationPitchLast != RotationPitch || RotationYawLast != RotationYaw)
            {
                RotationYaw = RotationYawLast;
                RotationPitch = RotationPitchLast;
                float yaw = RotationYaw;
                float pitch = RotationPitch;
                Task.Factory.StartNew(() =>
                {
                    ClientWorld.ClientMain.TrancivePacket(new PacketB20Player().YawPitch(yaw, pitch));
                });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Перерасчёт графического расположение игрока если было смещение, согласно индексу времени
        /// </summary>
        public bool UpdatePositionFrame(float index)
        {
            if (!Position.Equals(PositionPrev))
            {
                vec3 vp = (Position - PositionPrev) * index;
                PositionFrame = PositionPrev + vp;
                return true;
            }
            if (!PositionFrame.Equals(Position))
            {
                PositionFrame = Position;
                return true;
            }
            return false;
        }

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
        /// Получить коэффициент времени от прошлого пакета перемещения сервера в диапазоне 0 .. 1
        /// где 0 это финиш, 1 начало
        /// </summary>
        public float TimeIndex() => interpolation.TimeIndex();

        /// <summary>
        /// Задать позицию от сервера
        /// </summary>
        public void SetPositionServer(vec3 pos, bool sneaking)
        {
            interpolation.Restart();
            if (IsSneaking != sneaking)
            {
                IsSneaking = sneaking;
                if (IsSneaking) Sitting(); else Standing();
            }
            PositionPrev = Position;
            SetPosition(pos);
        }

        /// <summary>
        /// Обновление в кадре
        /// </summary>
        public void UpdateFrame()
        {
            // время от TPS клиента
            float indexW = ClientWorld.TimeIndex();
            // время от TPS пакетов сервера
            //float indexS = TimeIndex();
            bool isUpLookAt = false;
            bool isFrustumCulling = false;

            // Меняем угол обзора (как правило при изменении скорости)
            if (Fov.UpdateFrame(indexW)) UpProjection();
            // Меняем положения глаз
            if (Eye.UpdateFrame(indexW)) isUpLookAt = true;

            // Перерасчёт расположение игрока если было смещение, согласно индексу времени
            if (UpdatePositionFrame(indexW))
            {
                isFrustumCulling = true;
                isUpLookAt = true;
            }
            
            bool re = RotationEquals();

            if (isUpLookAt || re) UpLookAt();

            // Если имеется вражение камеры или было перемещение, то запускаем расчёт FrustumCulling
            if (re || isFrustumCulling || IsFrustumCulling) InitFrustumCulling();
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
                vec3 pos = Position + Motion;
                SetPosition(pos);
                Eye.Set(GetEyeHeight(), 4);
                Fov.Set(IsSprinting ? 1.22f : 1.43f, 4);

                ClientWorld.ClientMain.TrancivePacket(new PacketB20Player().Position(Position, IsSneaking));
            }

            Eye.Update();
            Fov.Update();
        }

        public override string ToString()
        {
            return Name + "\r\n" + base.ToString()
                + "\r\nposDraw:" + PositionFrame + "\r\nPitch: " + glm.degrees(RotationPitch);

                ;
            //return string.Format("{4}\r\nposDraw: {0}\r\nLast: {1} {2} {3}", 
            //    PositionDraw, PositionLast, IsSprinting ? "[Sp]" : "", FovDraw, Name);
        }
    }
}
