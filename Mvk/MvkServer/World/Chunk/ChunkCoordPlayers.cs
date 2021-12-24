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

        /// <summary>
        /// Добавить игрока в конкретный чанк
        /// </summary>
        public void AddPlayer(EntityPlayerServer player)
        {
            if (!players.Contains(player))
            {
                players.Add(player);
                player.LoadedChunks.Add(Position);
            }
        }

        /// <summary>
        /// Убрать игрока с конкретного чанка
        /// </summary>
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

        /// <summary>
        /// Перепроверить чанки игроков в попадание в обзоре, если нет, убрать
        /// </summary>
        public void FixOverviewChunk()
        {
            List<EntityPlayerServer> list = players.GetRange(0, players.Count);
            foreach (EntityPlayerServer entityPlayer in list)
            {
                vec2i min = entityPlayer.HitBox.ChunkPosManaged - entityPlayer.OverviewChunk;
                vec2i max = entityPlayer.HitBox.ChunkPosManaged + entityPlayer.OverviewChunk;
                if (Position.x < min.x || Position.x > max.x || Position.y < min.y || Position.y > max.y)
                {
                    RemovePlayer(entityPlayer);
                }
            }
        }
    }
}
