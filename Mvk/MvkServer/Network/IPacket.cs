namespace MvkServer.Network
{
    public interface IPacket
    {
        void ReadPacket(StreamBase stream);
        void WritePacket(StreamBase stream);
    }
}
