using MvkServer.Glm;

namespace MvkServer.Network.Packets
{
    public struct PacketS12Success : IPacket
    {
        private string uuid;
        public vec3 Pos;
        public float Yaw;
        public float Pitch;

        public PacketS12Success(string uuid)
        {
            this.uuid = uuid;
            Pos = new vec3(0f, 64f, 0f);
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
            Pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            Yaw = stream.ReadFloat();
            Pitch = stream.ReadFloat();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteString(uuid);
            stream.WriteFloat(Pos.x);
            stream.WriteFloat(Pos.y);
            stream.WriteFloat(Pos.z);
            stream.WriteFloat(Yaw);
            stream.WriteFloat(Pitch);
        }
    }
}
