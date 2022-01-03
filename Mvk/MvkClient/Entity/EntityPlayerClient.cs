using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Util;
using MvkClient.World;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.Util;
using System.Collections.Generic;

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

        public EntityPlayerClient(WorldClient world) => World = world;

        /// <summary>
        /// Задать перемещение игрока
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        public void SetMove(vec3 pos, float yaw, float pitch)
        {
            SetPosition(pos);
            SetRotation(yaw, pitch);
            //UpLookAt();
        }
        /// <summary>
        /// Задать перемещение игрока
        /// </summary>
        /// <param name="pos"></param>
        public void SetMove(vec3 pos) => SetMove(pos, RotationYaw, RotationPitch);
        public void SetMovePerv(vec3 pos)
        {
            PervPosition = pos;
            UpLookAt();
        }
        public void SetLast(vec3 pos) => LastTickPosition = pos;
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
            LookAt = glm.lookAt(PervPosition, PervPosition + Front, Up).to_array();
            
            // TODO::InitFrustumCulling возможно надо как-то поставить затычку, чтоб был интервал, не чаще 15 мс между
            InitFrustumCulling();
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
            float pitch = RotationPitch - deltaY / Height * speedMouse;
            float yaw = RotationYaw - deltaX / Width * speedMouse;

            if (pitch < -glm.radians(89.0f)) pitch = -glm.radians(89.0f);
            if (pitch > glm.radians(89.0f)) pitch = glm.radians(89.0f);
            if (yaw > glm.pi) yaw -= glm.pi360;
            if (yaw < -glm.pi) yaw += glm.pi360;

            SetRotation(yaw, pitch);
            UpLookAt();
        }

        protected void InitFrustumCulling()
        {
            if (LookAt == null || Projection == null) return;

            Frustum frustum = new Frustum();
            frustum.Init(LookAt, Projection);

            int countAll = 0;
            int countFC = 0;
            List<ChunkRender> listC = new List<ChunkRender>();
            
            for (int i = 0; i < DistSqrt.Length; i++)
            {
                int xc = DistSqrt[i].x + ChunkPos.x;
                int zc = DistSqrt[i].y + ChunkPos.y;
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
        }


        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (!Motion.Equals(new vec3(0)))
            {
                vec3 pos = Position + Motion;
                //World.Player.PervPosition.SetMove(pos)
                World.Player.SetMove(pos);
                World.ClientMain.TrancivePacket(new PacketC20Player(pos));
            }
        }

    }
}
