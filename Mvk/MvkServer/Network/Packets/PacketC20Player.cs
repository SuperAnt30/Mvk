using MvkServer.Glm;

namespace MvkServer.Network.Packets
{
    public struct PacketC20Player : IPacket
    {
        private vec3 pos;

        public PacketC20Player(vec3 pos)
        {
            this.pos = pos;
        }

        public vec3 GetPos() => pos;

        public void ReadPacket(StreamBase stream)
        {
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
        }
    }
}
