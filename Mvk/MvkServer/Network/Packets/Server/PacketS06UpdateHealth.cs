namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет пораметров здоровья игрока
    /// </summary>
    public struct PacketS06UpdateHealth : IPacket
    {
        private float health;

        public float GetHealth() => health;

        public PacketS06UpdateHealth(float health)
        {
            this.health = health;
        }

        public void ReadPacket(StreamBase stream)
        {
            health = stream.ReadFloat();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteFloat(health);
        }
    }
}
