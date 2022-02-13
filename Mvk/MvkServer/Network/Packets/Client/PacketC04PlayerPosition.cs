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

        public vec3 GetPos() => pos;
        public bool IsSneaking() => sneaking;

        public PacketC04PlayerPosition(vec3 pos, bool sneaking)
        {
            this.pos = pos;
            this.sneaking = sneaking;
        }

        public void ReadPacket(StreamBase stream)
        {
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            sneaking = stream.ReadBool();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteBool(sneaking);
        }
    }
}
