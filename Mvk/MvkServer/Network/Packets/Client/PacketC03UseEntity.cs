using MvkServer.Glm;

namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер взаимодействие с сущностью
    /// </summary>
    public struct PacketC03UseEntity : IPacket
    {
        private ushort id;
        private vec3 vec;
        private EnumAction action;

        public PacketC03UseEntity(ushort id, EnumAction action)
        {
            this.id = id;
            vec = new vec3();
            this.action = action;
        }
        public PacketC03UseEntity(ushort id, vec3 vec)
        {
            this.id = id;
            this.vec = vec;
            action = EnumAction.Attack;
        }

        /// <summary>
        /// id игрока на которого произошло действие
        /// </summary>
        public ushort GetId() => id;
        public EnumAction GetAction() => action;
        public vec3 GetVec() => vec;

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
            action = (EnumAction)stream.ReadByte();
            if (action == EnumAction.Attack)
            {
                vec = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            stream.WriteByte((byte)action);
            if (action == EnumAction.Attack)
            {
                stream.WriteFloat(vec.x);
                stream.WriteFloat(vec.y);
                stream.WriteFloat(vec.z);
            }
        }

        /// <summary>
        /// Варианты действия
        /// </summary>
        public enum EnumAction
        {
            /// <summary>
            /// Взаимодействие
            /// </summary>
            Interact = 1,
            /// <summary>
            /// Атака
            /// </summary>
            Attack = 2
        }
    }
}
