using MvkServer.Glm;

namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Пакет расположения игрока
    /// </summary>
    public struct PacketC04PlayerPosition : IPacket
    {
        private vec3 pos;
        private bool sneaking;
        private bool sprinting;

        public vec3 GetPos() => pos;
        public bool IsSneaking() => sneaking;
        public bool IsSprinting() => sprinting;

        public PacketC04PlayerPosition(vec3 pos, bool sneaking, bool sprinting)
        {
            this.pos = pos;
            this.sneaking = sneaking;
            this.sprinting = sprinting;
        }

        public void ReadPacket(StreamBase stream)
        {
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            sneaking = stream.ReadBool();
            sprinting = stream.ReadBool();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteBool(sneaking);
            stream.WriteBool(sprinting);
        }
    }
}
