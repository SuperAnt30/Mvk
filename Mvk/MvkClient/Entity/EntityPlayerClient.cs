using MvkClient.Renderer;
using MvkServer.Entity.Player;
using MvkServer.Glm;

namespace MvkClient.Entity
{
    /// <summary>
    /// Сущность игрока для клиента
    /// </summary>
    public class EntityPlayerClient : EntityPlayer
    {
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

        protected float Width => (float)GLWindow.WindowWidth;
        protected float Height => (float)GLWindow.WindowHeight;
        /// <summary>
        /// Задать id и имя игрока
        /// </summary>
        public void SetUUID(string name, string uuid)
        {
            Name = name;
            UUID = uuid;
        }

        /// <summary>
        /// Задать перемещение игрока
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        public void SetMove(vec3 pos, float yaw, float pitch)
        {
            HitBox.SetPos(pos);
            SetRotation(yaw, pitch);
            UpLookAt();
        }
        /// <summary>
        /// Задать перемещение игрока
        /// </summary>
        /// <param name="pos"></param>
        public void SetMove(vec3 pos) => SetMove(pos, RotationYaw, RotationPitch);
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
            LookAt = glm.lookAt(HitBox.Position, HitBox.Position + Front, Up).to_array();
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


    }
}
