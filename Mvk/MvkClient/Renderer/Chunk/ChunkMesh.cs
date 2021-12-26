namespace MvkClient.Renderer.Chunk
{
    /// <summary>
    /// Объект сетки чанка
    /// </summary>
    public class ChunkMesh : RenderMesh
    {
        /// <summary>
        /// Буфер точки, точка xyz, текстура точки uv, цвет точки rgba
        /// </summary>
        protected override int[] attrs { get; } = new int[] { 3, 2, 3, 2 };
    }
}
