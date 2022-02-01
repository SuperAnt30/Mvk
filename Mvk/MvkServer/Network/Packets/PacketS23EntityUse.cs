using MvkServer.Glm;

namespace MvkServer.Network.Packets
{
    /// <summary>
    /// Отправляем от сервер взаимодействие с сущностью
    /// </summary>
    //public struct PacketS23EntityUse : IPacket
    //{
    //    private vec3 pos;
    //    private PacketC22EntityUse.EnumAction action;

    //    public PacketS23EntityUse(PacketC22EntityUse.EnumAction action)
    //    {
    //        pos = new vec3();
    //        this.action = action;
    //    }
    //    public PacketS23EntityUse(vec3 pos)
    //    {
    //        this.pos = pos;
    //        action = PacketC22EntityUse.EnumAction.Push;
    //    }

    //    public PacketC22EntityUse.EnumAction GetAction() => action;
    //    public vec3 GetPos() => pos;

    //    public void ReadPacket(StreamBase stream)
    //    {
    //        action = (PacketC22EntityUse.EnumAction)stream.ReadByte();
    //        if (action == PacketC22EntityUse.EnumAction.Push)
    //        {
    //            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
    //        }
    //    }

    //    public void WritePacket(StreamBase stream)
    //    {
    //        stream.WriteByte((byte)action);
    //        if (action == PacketC22EntityUse.EnumAction.Push)
    //        {
    //            stream.WriteFloat(pos.x);
    //            stream.WriteFloat(pos.y);
    //            stream.WriteFloat(pos.z);
    //        }
    //    }
    //}
}
