using MvkServer.Glm;

namespace MvkServer.Network.Packets
{
    public struct PacketB20Player : IPacket
    {
        private string name; // заменить на id игрока, сущьности

        private vec3 pos;
        private bool sneaking;

        private float yaw;
        private float pitch;

        private byte type;
        /// <summary>
        /// тип запроса
        /// 0 - pos
        /// 1 - yaw, pitch
        /// 2 - pos, name
        /// 3 - yaw, pitch, name
        /// </summary>
        public byte Type() => type;

        /// <summary>
        /// Отправляем координату позиции с сервера клиенту
        /// </summary>
        public PacketB20Player Position(vec3 pos, bool sneaking)
        {
            this.pos = pos;
            this.sneaking = sneaking;
            type = 0;
            return this;
        }
        /// <summary>
        /// Отправляем вращение клиента серверу
        /// </summary>
        public PacketB20Player YawPitch(float yaw, float pitch)
        {
            this.yaw = yaw;
            this.pitch = pitch;
            type = 1;
            return this;
        }
        /// <summary>
        /// Отправляем координату позиции с сервера клиенту
        /// </summary>
        public PacketB20Player Position(vec3 pos, bool sneaking, string name)
        {
            this.pos = pos;
            this.sneaking = sneaking;
            this.name = name;
            type = 2;
            return this;
        }
        /// <summary>
        /// Отправляем вращение клиента серверу
        /// </summary>
        public PacketB20Player YawPitch(float yaw, float pitch, string name)
        {
            this.yaw = yaw;
            this.pitch = pitch;
            this.name = name;
            type = 3;
            return this;
        }
        public string GetName() => name;
        public vec3 GetPos() => pos;
        public bool IsSneaking() => sneaking;
        public float GetYaw() => yaw;
        public float GetPitch() => pitch;

        public void ReadPacket(StreamBase stream)
        {
            type = stream.ReadByte();
            if (type > 1) name = stream.ReadString();
            if (type == 0 || type == 2)
            {
                pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
                sneaking = stream.ReadBool();
            } else
            {
                yaw = stream.ReadFloat();
                pitch = stream.ReadFloat();
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte(type);
            if (type > 1) stream.WriteString(name);
            if (type == 0 || type == 2)
            {
                stream.WriteFloat(pos.x);
                stream.WriteFloat(pos.y);
                stream.WriteFloat(pos.z);
                stream.WriteBool(sneaking);
            }
            else
            {
                stream.WriteFloat(yaw);
                stream.WriteFloat(pitch);
            }
        }
    }
}
