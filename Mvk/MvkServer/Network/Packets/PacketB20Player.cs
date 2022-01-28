using MvkServer.Glm;

namespace MvkServer.Network.Packets
{
    public struct PacketB20Player : IPacket
    {
        private ushort id;

        private vec3 pos;
        private bool sneaking;
        private bool onGround;

        private float yawBody;
        private float yawHead;
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
        public PacketB20Player Position(vec3 pos, bool sneaking, bool onGround)
        {
            this.pos = pos;
            this.sneaking = sneaking;
            this.onGround = onGround;
            type = 0;
            return this;
        }
        /// <summary>
        /// Отправляем вращение клиента серверу
        /// </summary>
        public PacketB20Player YawPitch(float yawHead, float yawBody, float pitch)
        {
            this.yawHead = yawHead;
            this.yawBody = yawBody;
            this.pitch = pitch;
            type = 1;
            return this;
        }
        /// <summary>
        /// Отправляем координату позиции с сервера клиенту
        /// </summary>
        public PacketB20Player Position(vec3 pos, bool sneaking, bool onGround, ushort id)
        {
            this.pos = pos;
            this.sneaking = sneaking;
            this.onGround = onGround;
            this.id = id;
            type = 2;
            return this;
        }
        /// <summary>
        /// Отправляем вращение клиента серверу
        /// </summary>
        public PacketB20Player YawPitch(float yawHead, float yawBody, float pitch, ushort id)
        {
            this.yawHead = yawHead;
            this.yawBody = yawBody;
            this.pitch = pitch;
            this.id = id;
            type = 3;
            return this;
        }
        /// <summary>
        /// id игрока
        /// </summary>
        public ushort GetId() => id;
        public vec3 GetPos() => pos;
        public bool IsSneaking() => sneaking;
        public bool OnGround() => onGround;
        public float GetYawHead() => yawHead;
        public float GetYawBody() => yawBody;
        public float GetPitch() => pitch;

        public void ReadPacket(StreamBase stream)
        {
            type = stream.ReadByte();
            if (type > 1) id = stream.ReadUShort();
            if (type == 0 || type == 2)
            {
                pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
                sneaking = stream.ReadBool();
                onGround = stream.ReadBool();
            } else
            {
                yawHead = stream.ReadFloat();
                yawBody = stream.ReadFloat();
                pitch = stream.ReadFloat();
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte(type);
            if (type > 1) stream.WriteUShort(id);
            if (type == 0 || type == 2)
            {
                stream.WriteFloat(pos.x);
                stream.WriteFloat(pos.y);
                stream.WriteFloat(pos.z);
                stream.WriteBool(sneaking);
                stream.WriteBool(onGround);
            }
            else
            {
                stream.WriteFloat(yawHead);
                stream.WriteFloat(yawBody);
                stream.WriteFloat(pitch);
            }
        }
    }
}
