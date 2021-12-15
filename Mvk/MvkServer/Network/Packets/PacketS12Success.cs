using MvkServer.Glm;

namespace MvkServer.Network.Packets
{
    public struct PacketS12Success : IPacket
    {
        private string uuid;
        public vec3i Pos;
        public float Yaw;
        public float Pitch;

        public PacketS12Success(string uuid)
        {
            this.uuid = uuid;
            Pos = new vec3i(0, 64, 0);
            Yaw = 0;
            Pitch = 0;
        }

        /// <summary>
        /// хэш игрока
        /// </summary>
        public string GetUuid() => uuid;

        public void ReadPacket(StreamBase stream)
        {
            uuid = stream.ReadString();
            Pos = new vec3i(stream.ReadInt(), stream.ReadByte(), stream.ReadInt());
            Yaw = stream.ReadFloat();
            Pitch = stream.ReadFloat();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteString(uuid);
            stream.WriteInt(Pos.x);
            stream.WriteByte((byte)Pos.y);
            stream.WriteInt(Pos.z);
            stream.WriteFloat(Yaw);
            stream.WriteFloat(Pitch);
        }
    }
}
