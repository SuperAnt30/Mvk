using MvkServer.Glm;

namespace MvkServer.Network.Packets
{
    public struct PacketB20Player : IPacket
    {
        private vec3 pos;

        private float yaw;
        private float pitch;

        private float height;
        private float eyes;

        private byte type;
        /// <summary>
        /// тип запроса
        /// 0 - pos
        /// 1 - yaw, pitch
        /// 2 - pos, height, eyes
        /// </summary>
        public byte Type() => type;

        /// <summary>
        /// Отправляем координату позиции с сервера клиенту
        /// </summary>
        public PacketB20Player Position(vec3 pos)
        {
            this.pos = pos;
            type = 0;
            return this;
        }
        /// <summary>
        /// Отправляем координату позиции с сервера клиенту
        /// </summary>
        public PacketB20Player Position(vec3 pos, float height, float eyes)
        {
            this.pos = pos;
            this.height = height;
            this.eyes = eyes;
            type = 2;
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
        
        public vec3 GetPos() => pos;
        public float GetYaw() => yaw;
        public float GetPitch() => pitch;
        public float GetHeight() => height;
        public float GetEyes() => eyes;

        public void ReadPacket(StreamBase stream)
        {
            type = stream.ReadByte();
            switch (type)
            {
                case 0:
                    pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
                    break;
                case 1:
                    yaw = stream.ReadFloat();
                    pitch = stream.ReadFloat();
                    break;
                case 2:
                    pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
                    height = stream.ReadFloat();
                    eyes = stream.ReadFloat();
                    break;
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte(type);
            switch (type)
            {
                case 0:
                    stream.WriteFloat(pos.x);
                    stream.WriteFloat(pos.y);
                    stream.WriteFloat(pos.z);
                    break;
                case 1:
                    stream.WriteFloat(yaw);
                    stream.WriteFloat(pitch);
                    break;
                case 2:
                    stream.WriteFloat(pos.x);
                    stream.WriteFloat(pos.y);
                    stream.WriteFloat(pos.z);
                    stream.WriteFloat(height);
                    stream.WriteFloat(eyes);
                    break;
            }
        }
    }
}
