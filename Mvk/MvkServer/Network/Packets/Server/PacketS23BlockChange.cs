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
        private BlockState blockState;

        public BlockPos GetBlockPos() => blockPos;
        public BlockState GetBlockState() => blockState;

        public PacketS23BlockChange(WorldBase world, BlockPos blockPos)
        {
            blockState = world.GetBlockState(blockPos);
            this.blockPos = blockPos;
        }

        public void ReadPacket(StreamBase stream)
        {
            blockPos = new BlockPos(stream.ReadInt(), stream.ReadInt(), stream.ReadInt());
            blockState.ReadStream(stream);
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteInt(blockPos.X);
            stream.WriteInt(blockPos.Y);
            stream.WriteInt(blockPos.Z);
            blockState.WriteStream(stream);
        }
    }
}
