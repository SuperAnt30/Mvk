using MvkServer.Entity.Player;
using MvkServer.Glm;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Движение сущности
    /// </summary>
    public struct PacketS14EntityMotion : IPacket
    {
        private ushort id;
        private vec3 pos;
        private float yaw;
        private float pitch;
        private bool sneaking;

        public ushort GetId() => id;
        public vec3 GetPos() => pos;
        public float GetYaw() => yaw;
        public float GetPitch() => pitch;
        public bool IsSneaking() => sneaking;

        public PacketS14EntityMotion(EntityPlayer entity)
        {
            id = entity.Id;
            pos = entity.Position;
            yaw = entity.RotationYawHead;
            pitch = entity.RotationPitch;
            sneaking = entity.IsSneaking;
        }

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            yaw = stream.ReadFloat();
            pitch = stream.ReadFloat();
            sneaking = stream.ReadBool();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteFloat(yaw);
            stream.WriteFloat(pitch);
            stream.WriteBool(sneaking);
        }
    }
}
