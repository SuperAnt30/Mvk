using MvkClient.Renderer.Chunk;
using MvkServer.Util;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Объект рендера альфа блоков
    /// </summary>
    public class BlockAlphaRender : BlockRender
    {
        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockAlphaRender(ChunkRender chunkRender, int cbX, int cbY, int cbZ) 
            : base(chunkRender, cbX, cbY, cbZ) { }
    }


}
