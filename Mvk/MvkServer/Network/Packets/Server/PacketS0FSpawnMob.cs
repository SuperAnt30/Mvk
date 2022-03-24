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
        private EnumEntities type;
        private vec3 pos;
        private float yaw;
        private float yawHead;
        private float pitch;

        public ushort GetId() => id;
        public EnumEntities GetEnum() => type;
        public vec3 GetPos() => pos;
        public float GetYaw() => yaw;
        public float GetYawHead() => yawHead;
        public float GetPitch() => pitch;

        public PacketS0FSpawnMob(EntityBase entity)
        {
            id = entity.Id;
            type = entity.Type;
            pos = entity.Position;
            if (entity is EntityLivingHead entityLH)
            {
                yawHead = entityLH.RotationYawHead;
                yaw = entityLH.RotationYaw;
                pitch = entityLH.RotationPitch;
            }
            else
            {
                yawHead = 0;
                yaw = 0;
                pitch = 0;
            }
        }

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
            type = (EnumEntities)stream.ReadUShort();
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            yawHead = stream.ReadFloat();
            yaw = stream.ReadFloat();
            pitch = stream.ReadFloat();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            stream.WriteUShort((ushort)type);
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteFloat(yawHead);
            stream.WriteFloat(yaw);
            stream.WriteFloat(pitch);
        }
    }
}
