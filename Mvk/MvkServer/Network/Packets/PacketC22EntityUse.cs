using MvkServer.Glm;

namespace MvkServer.Network.Packets
{
    /// <summary>
    /// Отправляем на сервер взаимодействие с сущностью
    /// </summary>
    //public struct PacketC22EntityUse : IPacket
    //{
    //    private ushort id;
    //    private vec3 pos;
    //    private EnumAction action;

    //    public PacketC22EntityUse(ushort id, EnumAction action)
    //    {
    //        this.id = id;
    //        pos = new vec3();
    //        this.action = action;
    //    }
    //    public PacketC22EntityUse(ushort id, vec3 pos)
    //    {
    //        this.id = id;
    //        this.pos = pos;
    //        action = EnumAction.Push;
    //    }

    //    /// <summary>
    //    /// id игрока на которого произошло действие
    //    /// </summary>
    //    public ushort GetId() => id;
    //    public EnumAction GetAction() => action;
    //    public vec3 GetPos() => pos;

    //    public void ReadPacket(StreamBase stream)
    //    {
    //        id = stream.ReadUShort();
    //        action = (EnumAction)stream.ReadByte();
    //        if (action == EnumAction.Push)
    //        {
    //            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
    //        }
    //    }

    //    public void WritePacket(StreamBase stream)
    //    {
    //        stream.WriteUShort(id);
    //        stream.WriteByte((byte)action);
    //        if (action == EnumAction.Push)
    //        {
    //            stream.WriteFloat(pos.x);
    //            stream.WriteFloat(pos.y);
    //            stream.WriteFloat(pos.z);
    //        }
    //    }

    //    /// <summary>
    //    /// Варианты действия
    //    /// </summary>
    //    public enum EnumAction
    //    {
    //        Push = 1,
    //        Attack = 2
    //    }
    //}
}
