using MvkServer.Glm;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет расположения игрока, при старте, телепорт, рестарте и тп
    /// </summary>
    public struct PacketS08PlayerPosLook : IPacket
    {
        private vec3 pos;
        private float yaw;
        private float pitch;

        public vec3 GetPos() => pos;
        public float GetYaw() => yaw;
        public float GetPitch() => pitch;

        public PacketS08PlayerPosLook(vec3 pos, float yaw, float pitch)
        {
            this.pos = pos;
            this.yaw = yaw;
            this.pitch = pitch;
        }

        public void ReadPacket(StreamBase stream)
        {
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            yaw = stream.ReadFloat();
            pitch = stream.ReadFloat();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteFloat(yaw);
            stream.WriteFloat(pitch);
        }
    }
}
