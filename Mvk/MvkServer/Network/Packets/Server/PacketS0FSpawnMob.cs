using MvkServer.Entity;
using MvkServer.Glm;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет спавна моба
    /// </summary>
    public struct PacketS0FSpawnMob : IPacket
    {
        private ushort id;
        //private string name;
        private vec3 pos;
        private float yaw;
        private float yawHead;
        private float pitch;

        public ushort GetId() => id;
        //public string GetName() => name;
        public vec3 GetPos() => pos;
        public float GetYaw() => yaw;
        public float GetYawHead() => yawHead;
        public float GetPitch() => pitch;

        public PacketS0FSpawnMob(EntityLivingHead entity)
        {
            id = entity.Id;
            //name = entity.Name;
            pos = entity.Position;
            yawHead = entity.RotationYawHead;
            yaw = entity.RotationYaw;
            pitch = entity.RotationPitch;
        }

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
            //name = stream.ReadString();
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            yawHead = stream.ReadFloat();
            yaw = stream.ReadFloat();
            pitch = stream.ReadFloat();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            //stream.WriteString(name);
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteFloat(yawHead);
            stream.WriteFloat(yaw);
            stream.WriteFloat(pitch);
        }
    }
}
