namespace MvkClient.Renderer.Chunk
{
    /// <summary>
    /// Структура очереди чанка для рендера
    /// </summary>
    public struct ChunkRenderQueue
    {
        /// <summary>
        /// Чанк рендера
        /// </summary>
        public ChunkRender chunk;
        /// <summary>
        /// Координата псевдочанка, который надо рендерить
        /// </summary>
        public int y;
    }
}
