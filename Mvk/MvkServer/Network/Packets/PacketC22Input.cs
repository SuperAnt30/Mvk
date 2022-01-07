using MvkServer.Util;

namespace MvkServer.Network.Packets
{
    /// <summary>
    /// Отправляем на сервер действия клавиатуры
    /// </summary>
    public struct PacketC22Input : IPacket
    {
        private byte key;

        public PacketC22Input(EnumKeyAction key) => this.key = (byte)key;

        public EnumKeyAction GetKey() => (EnumKeyAction)key;

        public void ReadPacket(StreamBase stream) => key = stream.ReadByte();

        public void WritePacket(StreamBase stream) => stream.WriteByte(key);
    }
}
