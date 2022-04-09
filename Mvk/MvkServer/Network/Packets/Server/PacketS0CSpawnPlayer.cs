using MvkServer.Entity;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Item;
using System.Collections;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет спавна сетевого игрока
    /// </summary>
    public struct PacketS0CSpawnPlayer : IPacket
    {
        private ushort id;
        private string uuid;
        private string name;
        private vec3 pos;
        private float yaw;
        private float pitch;
        private ItemStack[] stacks;
        private ArrayList list;

        public ushort GetId() => id;
        public string GetUuid() => uuid;
        public string GetName() => name;
        public vec3 GetPos() => pos;
        public float GetYaw() => yaw;
        public float GetPitch() => pitch;
        public ItemStack[] GetStacks() => stacks;
        public ArrayList GetList() => list;

        public PacketS0CSpawnPlayer(EntityPlayer entity)
        {
            uuid = entity.UUID;
            id = entity.Id;
            name = entity.GetName();
            pos = entity.Position;
            yaw = entity.RotationYawHead;
            pitch = entity.RotationPitch;
            stacks = entity.Inventory.GetCurrentItemAndArmor();
            list = entity.MetaData.GetAllWatched();
        }

        public void ReadPacket(StreamBase stream)
        {
            uuid = stream.ReadString();
            id = stream.ReadUShort();
            name = stream.ReadString();
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            yaw = stream.ReadFloat();
            pitch = stream.ReadFloat();
            int count = stream.ReadByte();
            stacks = new ItemStack[count];
            for (int i = 0; i < count; i++)
            {
                stacks[i] = ItemStack.ReadStream(stream);
            }
            list = DataWatcher.ReadWatchedListFromPacketBuffer(stream);
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteString(uuid);
            stream.WriteUShort(id);
            stream.WriteString(name);
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteFloat(yaw);
            stream.WriteFloat(pitch);
            int count = stacks.Length;
            stream.WriteByte((byte)count);
            for (int i = 0; i < count; i++)
            {
                ItemStack.WriteStream(stacks[i], stream);
            }
            DataWatcher.WriteWatchedListToPacketBuffer(list, stream);
        }
    }
}
