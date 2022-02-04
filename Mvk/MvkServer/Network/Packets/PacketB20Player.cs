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
        /// 2 - анимашка
        /// 3 - pos, yaw, pitch (респавн)
        /// 10 - pos, id
        /// 11 - yaw, pitch, id
        /// 12 - анимашка id
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
        /// Отправляем анимацию
        /// </summary>
        public PacketB20Player Animation()
        {
            type = 2;
            return this;
        }
        /// <summary>
        /// Отправляем респавн с сервера клиенту
        /// </summary>
        public PacketB20Player Respawn(vec3 pos, float yaw, float pitch)
        {
            this.pos = pos;
            yawHead = yaw;
            this.pitch = pitch;
            type = 3;
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
            type = 10;
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
            type = 11;
            return this;
        }
        /// <summary>
        /// Отправляем анимацию серверу
        /// </summary>
        public PacketB20Player Animation(ushort id)
        {
            this.id = id;
            type = 12;
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
            if (type > 9) id = stream.ReadUShort();
            if (type == 0 || type == 10)
            {
                pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
                sneaking = stream.ReadBool();
                onGround = stream.ReadBool();
            } else if (type == 1 || type == 11)
            {
                yawHead = stream.ReadFloat();
                yawBody = stream.ReadFloat();
                pitch = stream.ReadFloat();
            }
            else if (type == 3)
            {
                pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
                yawHead = stream.ReadFloat();
                pitch = stream.ReadFloat();
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte(type);
            if (type > 9) stream.WriteUShort(id);
            if (type == 0 || type == 10)
            {
                stream.WriteFloat(pos.x);
                stream.WriteFloat(pos.y);
                stream.WriteFloat(pos.z);
                stream.WriteBool(sneaking);
                stream.WriteBool(onGround);
            }
            else if(type == 1 || type == 11)
            {
                stream.WriteFloat(yawHead);
                stream.WriteFloat(yawBody);
                stream.WriteFloat(pitch);
            }
            else if (type == 3)
            {
                stream.WriteFloat(pos.x);
                stream.WriteFloat(pos.y);
                stream.WriteFloat(pos.z);
                stream.WriteFloat(yawHead);
                stream.WriteFloat(pitch);
            }
        }
    }
}
