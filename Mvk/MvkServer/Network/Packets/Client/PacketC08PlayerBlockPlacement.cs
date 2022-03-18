using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер установку блока
    /// </summary>
    public struct PacketC08PlayerBlockPlacement : IPacket
    {
        private BlockPos blockPos;
        private vec3 facing;

        /// <summary>
        /// Позиция блока где устанавливаем блок
        /// </summary>
        public BlockPos GetBlockPos() => blockPos;
        /// <summary>
        /// Точка куда устанавливаем блок (параметр с RayCast)
        /// значение в пределах 0..1, образно фиксируем пиксел клика
        /// </summary>
        public vec3 GetFacing() => facing;

        public PacketC08PlayerBlockPlacement(BlockPos blockPos, vec3 facing)
        {
            this.blockPos = blockPos;
            this.facing = facing;
        }

        public void ReadPacket(StreamBase stream)
        {
            blockPos = new BlockPos(stream.ReadInt(), stream.ReadInt(), stream.ReadInt());
            facing = new vec3(stream.ReadByte() / 16f, stream.ReadByte() / 16f, stream.ReadByte() / 16f);
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteInt(blockPos.X);
            stream.WriteInt(blockPos.Y);
            stream.WriteInt(blockPos.Z);
            stream.WriteByte((byte)(facing.x * 16f));
            stream.WriteByte((byte)(facing.y * 16f));
            stream.WriteByte((byte)(facing.z * 16f));
        }
    }
}
