using MvkServer.Entity.Player;
using MvkServer.Glm;

namespace MvkServer.Network.Packets
{
    public struct PacketS12Success : IPacket
    {
        private ushort id;
        private string uuid;
        public string Name;
        public vec3 Pos;
        public float Yaw;
        public float Pitch;

        public PacketS12Success(string uuid, ushort id)
        {
            this.uuid = uuid;
            this.id = id;
            Name = "";
            Pos = new vec3(0f, 64f, 0f);
            Yaw = 0;
            Pitch = 0;
        }

        public PacketS12Success(EntityPlayerServer player)
        {
            uuid = player.UUID;
            id = player.Id;
            Name = player.Name;
            Pos = player.Position;
            Yaw = player.RotationYaw;
            Pitch = player.RotationPitch;
        }

        /// <summary>
        /// id игрока
        /// </summary>
        public ushort GetId() => id;
        /// <summary>
        /// хэш игрока
        /// </summary>
        public string GetUuid() => uuid;

        public void ReadPacket(StreamBase stream)
        {
            uuid = stream.ReadString();
            id = stream.ReadUShort();
            Name = stream.ReadString();
            Pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            Yaw = stream.ReadFloat();
            Pitch = stream.ReadFloat();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteString(uuid);
            stream.WriteUShort(id);
            stream.WriteString(Name);
            stream.WriteFloat(Pos.x);
            stream.WriteFloat(Pos.y);
            stream.WriteFloat(Pos.z);
            stream.WriteFloat(Yaw);
            stream.WriteFloat(Pitch);
        }
    }
}
