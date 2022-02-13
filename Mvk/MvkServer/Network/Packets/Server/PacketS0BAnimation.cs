namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет анимации
    /// </summary>
    public struct PacketS0BAnimation : IPacket
    {
        private ushort id;
        private byte animation;

        public ushort GetId() => id;
        public EnumAnimation GetAnimation() => (EnumAnimation)animation;

        public PacketS0BAnimation(ushort id, EnumAnimation animation)
        {
            this.id = id;
            this.animation = (byte)animation;
        }

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
            animation = stream.ReadByte();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            stream.WriteByte(animation);
        }

        public enum EnumAnimation
        {
            /// <summary>
            /// Качающий предмет
            /// </summary>
            SwingItem = 1,
            /// <summary>
            /// Анимация боли
            /// </summary>
            Hurt = 2
        }
    }
}
