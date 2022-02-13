namespace MvkServer.Network.Packets
{
    //public struct PacketS17Health : IPacket
    //{
    //    private ushort id;

    //    /// <summary>
    //    /// Здоровье игрока
    //    /// </summary>
    //    private float health;
    //    /// <summary>
    //    /// был нанесён урон
    //    /// </summary>
    //    private bool damage;

    //    public PacketS17Health(float health, bool damage, ushort id)
    //    {
    //        this.health = health;
    //        this.damage = damage;
    //        this.id = id;
    //    }

    //    public ushort GetId() => id;
    //    public bool GetDamage() => damage;
    //    public float GetHealth() => health;

    //    public void ReadPacket(StreamBase stream)
    //    {
    //        id = stream.ReadUShort();
    //        health = stream.ReadFloat();
    //        damage = stream.ReadBool();
    //    }

    //    public void WritePacket(StreamBase stream)
    //    {
    //        stream.WriteUShort(id);
    //        stream.WriteFloat(health);
    //        stream.WriteBool(damage);
    //    }
    //}
}
