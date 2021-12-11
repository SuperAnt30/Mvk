namespace MvkServer.Network
{
    public interface IPacket
    {
        byte Id { get; }
        void ReadPacket(StreamBase stream);
        void WritePacket(StreamBase stream);
    }
}
