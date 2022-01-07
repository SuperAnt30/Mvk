using MvkServer.Glm;

namespace MvkServer.Network.Packets
{
    public struct PacketB20Player : IPacket
    {
        private vec3 pos;
        private float yaw;
        private float pitch;
        private bool rotating;

        /// <summary>
        /// Отправляем координату позиции с сервера клиенту
        /// </summary>
        public PacketB20Player(vec3 pos)
        {
            yaw = 0;
            pitch = 0;
            this.pos = pos;
            rotating = false;
        }
        /// <summary>
        /// Отправляем вращение клиента серверу
        /// </summary>
        public PacketB20Player(float yaw, float pitch)
        {
            pos = new vec3();
            this.yaw = yaw;
            this.pitch = pitch;
            rotating = true;
        }

        public vec3 GetPos() => pos;
        public bool GetRotating() => rotating;
        public float GetYaw() => yaw;
        public float GetPitch() => pitch;

        public void ReadPacket(StreamBase stream)
        {
            rotating = stream.ReadBool();
            if (rotating)
            {
                yaw = stream.ReadFloat();
                pitch = stream.ReadFloat();
            }
            else
            {
                pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteBool(rotating);
            if (rotating)
            {
                stream.WriteFloat(yaw);
                stream.WriteFloat(pitch);
            }
            else
            {
                stream.WriteFloat(pos.x);
                stream.WriteFloat(pos.y);
                stream.WriteFloat(pos.z);
            }
        }
    }
}
