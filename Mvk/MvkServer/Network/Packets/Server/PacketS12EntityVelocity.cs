using MvkServer.Glm;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Скорость  сущности
    /// </summary>
    public struct PacketS12EntityVelocity : IPacket
    {
        private ushort id;
        private vec3 motion;

        public ushort GetId() => id;
        public vec3 GetMotion() => motion;

        public PacketS12EntityVelocity(ushort id, vec3 motion)
        {
            this.id = id;
            this.motion = motion;
        }

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
            motion = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            stream.WriteFloat(motion.x);
            stream.WriteFloat(motion.y);
            stream.WriteFloat(motion.z);
        }
    }
}
