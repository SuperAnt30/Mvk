using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Звуковой эффект
    /// </summary>
    public struct PacketS29SoundEffect : IPacket
    {
        private AssetsSample assetsSample;
        private vec3 position;
        private float volume;
        public float pitch;

        public AssetsSample GetAssetsSample() => assetsSample;
        public vec3 GetPosition() => position;
        public float GetVolume() => volume;
        public float GetPitch() => pitch;

        public PacketS29SoundEffect(AssetsSample assetsSample, vec3 position, float volume, float pitch)
        {
            this.assetsSample = assetsSample;
            this.position = position;
            this.volume = volume;
            this.pitch = pitch;
        }

        public void ReadPacket(StreamBase stream)
        {
            assetsSample = (AssetsSample)stream.ReadUShort();
            position = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            volume = stream.ReadByte() / 255f;
            pitch = stream.ReadFloat();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort((ushort)assetsSample);
            stream.WriteFloat(position.x);
            stream.WriteFloat(position.y);
            stream.WriteFloat(position.z);
            stream.WriteByte((byte)Mth.Clamp((int)(volume * 255), 0, 255));
            stream.WriteFloat(pitch);
        }
    }
}
