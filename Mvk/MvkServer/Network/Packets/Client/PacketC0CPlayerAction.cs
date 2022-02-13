namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Пакет действия игрока, падения
    /// </summary>
    public struct PacketC0CPlayerAction : IPacket
    {
        private EnumAction action;
        private float param;

        public EnumAction GetAction() => action;
        public float GetParam() => param;

        public PacketC0CPlayerAction(EnumAction action, float param)
        {
            this.action = action;
            this.param = param;
        }

        public void ReadPacket(StreamBase stream)
        {
            action = (EnumAction)stream.ReadByte();
            if (action == EnumAction.Fall)
            {
                param = stream.ReadFloat();
            }
        }
        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte((byte)action);
            if (action == EnumAction.Fall)
            {
                stream.WriteFloat(param);
            }
        }

        /// <summary>
        /// Варианты действия
        /// </summary>
        public enum EnumAction
        {
            /// <summary>
            /// Падение
            /// </summary>
            Fall = 1
        }
    }
}
