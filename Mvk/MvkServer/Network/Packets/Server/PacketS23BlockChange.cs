using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Отправляем клиенту один изменённый блок
    /// </summary>
    public struct PacketS23BlockChange : IPacket
    {
        private BlockPos blockPos;
        private EnumBlock eBlock;

        public BlockPos GetBlockPos() => blockPos;
        public EnumBlock GetDigging() => eBlock;

        //public PacketS23BlockChange(BlockPos blockPos, EnumBlock eBlock)
        //{
        //    this.blockPos = blockPos;
        //    this.eBlock = eBlock;
        //}
        public PacketS23BlockChange(WorldBase world, BlockPos blockPos)
        {
            eBlock = world.GetEBlock(blockPos);
            this.blockPos = blockPos;
        }

        public void ReadPacket(StreamBase stream)
        {
            blockPos = new BlockPos(stream.ReadInt(), stream.ReadInt(), stream.ReadInt());
            eBlock = (EnumBlock)stream.ReadByte();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteInt(blockPos.X);
            stream.WriteInt(blockPos.Y);
            stream.WriteInt(blockPos.Z);
            stream.WriteUShort((ushort)eBlock);
        }
    }
}
