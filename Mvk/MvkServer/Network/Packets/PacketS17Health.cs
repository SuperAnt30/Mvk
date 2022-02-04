namespace MvkServer.Network.Packets
{
    public struct PacketS17Health : IPacket
    {
        /// <summary>
        /// Здоровье игрока
        /// </summary>
        private float health;

        public PacketS17Health(float health) => this.health = health;

        public float GetHealth() => health;

        public void ReadPacket(StreamBase stream) => health = stream.ReadFloat();

        public void WritePacket(StreamBase stream) => stream.WriteFloat(health);
    }
}
