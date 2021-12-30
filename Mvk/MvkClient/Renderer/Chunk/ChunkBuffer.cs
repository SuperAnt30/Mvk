namespace MvkClient.Renderer.Chunk
{
    /// <summary>
    /// Объект буфера сеток псевдочанка
    /// </summary>
    public class ChunkBuffer
    {
        /// <summary>
        /// Уровень псевдочанка
        /// </summary>
        public int YBase { get; protected set; }
        /// <summary>
        /// Массив буфера сетки
        /// </summary>
        public float[] Buffer { get; protected set; } = new float[0];
        /// <summary>
        /// Массив альфа блоков Voxels
        /// </summary>
        //public List<VoxelData> Alphas { get; protected set; } = new List<VoxelData>();
        /// <summary>
        /// Пометка изменения
        /// </summary>
        public bool IsModifiedRender { get; protected set; } = true;

        public ChunkBuffer(int y) => YBase = y;

        /// <summary>
        /// Пометка псевдо чанка для рендера
        /// </summary>
        public void SetModifiedRender() => IsModifiedRender = true;

        /// <summary>
        /// Рендер сделан
        /// </summary>
        public void RenderDone(float[] buffer)
        {
            Buffer = buffer;
            IsModifiedRender = false;
        }

        

        /// <summary>
        /// Строка
        /// </summary>
        public override string ToString() => YBase.ToString() + " " + (IsModifiedRender ? "* " : "") + Buffer.Length.ToString();
    }
}
