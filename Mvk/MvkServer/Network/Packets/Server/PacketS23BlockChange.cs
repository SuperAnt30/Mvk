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

        // TODO::2022-04-18 заменить на структуру данных блок 3 байта, где id, метдата, освещение
        private BlockState blockState;

        //private EnumBlock eBlock;

        public BlockPos GetBlockPos() => blockPos;
        public BlockState GetBlockState() => blockState;
        //public EnumBlock GetDigging() => eBlock;

        //public PacketS23BlockChange(BlockPos blockPos, EnumBlock eBlock)
        //{
        //    this.blockPos = blockPos;
        //    this.eBlock = eBlock;
        //}
        public PacketS23BlockChange(WorldBase world, BlockPos blockPos)
        {
            blockState = world.GetBlockState(blockPos);
            //eBlock = world.GetEBlock(blockPos);
            this.blockPos = blockPos;
        }

        public void ReadPacket(StreamBase stream)
        {
            blockPos = new BlockPos(stream.ReadInt(), stream.ReadInt(), stream.ReadInt());
            blockState.ReadStream(stream);
            //eBlock = (EnumBlock)stream.ReadUShort();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteInt(blockPos.X);
            stream.WriteInt(blockPos.Y);
            stream.WriteInt(blockPos.Z);
            blockState.WriteStream(stream);
            //stream.WriteUShort((ushort)eBlock);
        }
    }
}
