namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Пакет обзора камеры игрока
    /// </summary>
    public struct PacketC05PlayerLook : IPacket
    {
        private float yaw;
        private float pitch;
        private bool sneaking;

        public float GetYaw() => yaw;
        public float GetPitch() => pitch;
        public bool IsSneaking() => sneaking;

        public PacketC05PlayerLook(float yaw, float pitch, bool sneaking)
        {
            this.yaw = yaw;
            this.pitch = pitch;
            this.sneaking = sneaking;
        }

        public void ReadPacket(StreamBase stream)
        {
            yaw = stream.ReadFloat();
            pitch = stream.ReadFloat();
            sneaking = stream.ReadBool();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteFloat(yaw);
            stream.WriteFloat(pitch);
            stream.WriteBool(sneaking);
        }
    }
}
