using MvkServer.Entity.Player;
using MvkServer.Glm;
using System.Collections.Generic;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Объект для чанка, фиксирует какие игроки могут видеть чанк
    /// </summary>
    public class ChunkCoordPlayers
    {
        /// <summary>
        /// Позиция чанка
        /// </summary>
        public vec2i Position { get; protected set; }
        /// <summary>
        /// Список игроков
        /// </summary>
        protected List<EntityPlayerServer> players = new List<EntityPlayerServer>();

        public ChunkCoordPlayers(vec2i pos) => Position = pos;

        public void AddPlayer(EntityPlayerServer player)
        {
            if (!players.Contains(player))
            {
                players.Add(player);
                player.LoadedChunks.Add(Position);
            }
        }

        public bool RemovePlayer(EntityPlayerServer player)
        {
            if (players.Contains(player))
            {
                players.Remove(player);
                player.LoadedChunks.Remove(Position);
                return true;
            }
            return false;
        }

        public int Count => players.Count;
    }
}
