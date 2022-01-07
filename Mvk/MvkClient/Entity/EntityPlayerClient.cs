using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Util;
using MvkClient.World;
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
        public WorldClient World { get; protected set; }
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

        public vec3 Front { get; protected set; }
        public vec3 Up { get; protected set; }
        public vec3 Right { get; protected set; }
        /// <summary>
        /// Угол обзора
        /// </summary>
        public float Fov { get; protected set; } = 1.221111f;
        /// <summary>
        /// Массив чанков которые попадают под FrustumCulling для рендера
        /// </summary>
        public ChunkRender[] ChunkFC { get; protected set; } = new ChunkRender[0];

        protected float Width => (float)GLWindow.WindowWidth;
        protected float Height => (float)GLWindow.WindowHeight;

        /// <summary>
        /// Объект времени c последнего тпс
        /// </summary>
        protected InterpolationTime interpolation = new InterpolationTime();

        public EntityPlayerClient(WorldClient world)
        {
            World = world;
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
            PositionLast = pos;
            SetMoveDraw(pos);
            RotationEquals();
            IsFrustumCulling = true;
        }

        /// <summary>
        /// Задать позицию для прорисовки на экране и обновить матрицу UpLookAt
        /// </summary>
        public void SetMoveDraw(vec3 pos)
        {
            PositionDraw = pos;
            UpLookAt();
        }
        /// <summary>
        /// Обновить матрицу камеры
        /// </summary>
        public void UpLookAt()
        {
            mat4 rotation = new mat4(1.0f);
            rotation = glm.rotate(rotation, RotationYaw, new vec3(0, 1, 0));
            rotation = glm.rotate(rotation, RotationPitch, new vec3(1, 0, 0));
            Front = new vec3(rotation * new vec4(0, 0, -1, 1));
            Right = new vec3(rotation * new vec4(1, 0, 0, 1));
            Up = new vec3(rotation * new vec4(0, 1, 0, 1));
            LookAt = glm.lookAt(PositionDraw, PositionDraw + Front, Up).to_array();

            // InitFrustumCulling перенесён в WorldRenderer.Draw перед прориовкой
        }
        /// <summary>
        /// Задать угол обзора
        /// </summary>
        /// <param name="fov">угол обзора в радианах</param>
        public void SetFov(float fov)
        {
            Fov = fov;
            UpProjection();
        }
        /// <summary>
        /// Обновить перспективу камеры
        /// </summary>
        public void UpProjection() 
            => Projection = glm.perspective(Fov, Width / Height, 0.001f, OverviewChunk * 22.624f * 2f).to_array();

        public void MouseMove(float deltaX, float deltaY)
        {
            // Чувствительность мыши
            float speedMouse = 1.5f;

            if (deltaX == 0 && deltaY == 0) return;
            float pitch = RotationPitchLast - deltaY / Height * speedMouse;
            float yaw = RotationYawLast - deltaX / Width * speedMouse;

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
                UpLookAt();
                float yaw = RotationYaw;
                float pitch = RotationPitch;
                Task.Factory.StartNew(() =>
                {
                    World.ClientMain.TrancivePacket(new PacketB20Player(yaw, pitch));
                });
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
            vec2i chunkPos = new vec2i(ChunkPos);
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
                    listC.Add(World.ChunkPrClient.GetChunkRender(new vec2i(xc, zc), true));
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
                World.ClientMain.TrancivePacket(new PacketC22Input(keyAction));
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
        public void SetPositionServer(vec3 pos)
        {
            interpolation.Restart();
            PositionLast = Position;
            SetPosition(pos);
        }
    }
}
