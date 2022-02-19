using MvkServer.Glm;
using System.Collections.Generic;

namespace MvkServer.Util
{
    /// <summary>
    /// Объект для отладки чанков
    /// </summary>
    public class DebugChunk
    {
        public List<vec3i> listChunkServer = new List<vec3i>();
        public List<vec2i> listChunkPlayers = new List<vec2i>();
        public List<vec3i> listChunkPlayer = new List<vec3i>();
        public List<vec3i> listChunkPlayerEntity = new List<vec3i>();
        public bool isRender;
    }
}
