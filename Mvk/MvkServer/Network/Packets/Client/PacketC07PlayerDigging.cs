using MvkServer.Util;

namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер действие с блоком добычи
    /// </summary>
    public struct PacketC07PlayerDigging : IPacket
    {
        private BlockPos blockPos;
        private EnumDigging digging;

        public BlockPos GetBlockPos() => blockPos;
        public EnumDigging GetDigging() => digging;

        public PacketC07PlayerDigging(BlockPos blockPos, EnumDigging digging)
        {
            this.blockPos = blockPos;
            this.digging = digging;
        }

        public void ReadPacket(StreamBase stream)
        {
            blockPos = new BlockPos(stream.ReadInt(), stream.ReadInt(), stream.ReadInt());
            digging = (EnumDigging)stream.ReadByte();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteInt(blockPos.X);
            stream.WriteInt(blockPos.Y);
            stream.WriteInt(blockPos.Z);
            stream.WriteByte((byte)digging);
        }

        /// <summary>
        /// Варианты действия разрушения
        /// </summary>
        public enum EnumDigging
        {
            /// <summary>
            /// Начали разрушать блок
            /// </summary>
            Start = 0,
            /// <summary>
            /// Отмена разрушения блока
            /// </summary>
            About = 1,
            /// <summary>
            /// Блок разрушен
            /// </summary>
            Stop = 2
            //Drop = 3
        }
    }
}
