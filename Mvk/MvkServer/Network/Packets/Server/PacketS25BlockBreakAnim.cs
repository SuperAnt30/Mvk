using MvkServer.Util;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Анимация разрушения блока
    /// </summary>
    public struct PacketS25BlockBreakAnim : IPacket
    {
        private int breakerId;
        private BlockPos blockPos;
        private int progress;

        public int GetBreakerId() => breakerId;
        public BlockPos GetBlockPos() => blockPos;
        public int GetProgress() => progress;

        public PacketS25BlockBreakAnim(int breakerId, BlockPos blockPos, int progress)
        {
            this.breakerId = breakerId;
            this.blockPos = blockPos;
            this.progress = progress;
        }

        public void ReadPacket(StreamBase stream)
        {
            breakerId = stream.ReadUShort();
            blockPos = new BlockPos(stream.ReadInt(), stream.ReadInt(), stream.ReadInt());
            progress = stream.ReadSByte();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort((ushort)breakerId);
            stream.WriteInt(blockPos.X);
            stream.WriteInt(blockPos.Y);
            stream.WriteInt(blockPos.Z);
            stream.WriteSByte((sbyte)progress);
        }
    }
}
