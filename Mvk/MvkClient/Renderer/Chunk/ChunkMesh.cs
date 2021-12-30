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

        /// <summary>
        /// Пометка изменения
        /// </summary>
        public bool IsModifiedRender { get; protected set; } = true;
        /// <summary>
        /// Пометка псевдо чанка для рендера
        /// </summary>
        public void SetModifiedRender() => IsModifiedRender = true;
        /// <summary>
        /// Массив буфера сетки
        /// </summary>
        public float[] Buffer { get; protected set; }

        /// <summary>
        /// Удалить
        /// </summary>
        public override void Delete()
        {
            base.Delete();
            Buffer = null;
        }

        /// <summary>
        /// Буфер внесён
        /// </summary>
        public void SetBuffer(float[] buffer)
        {
            Buffer = buffer;
            IsModifiedRender = false;
        }

        /// <summary>
        /// Занести буфер в OpenGL 
        /// </summary>
        public bool BindBuffer()
        {
            if (Buffer != null && Buffer.Length > 0)
            {
                BindBuffer(Buffer);
                Buffer = null;
                return true;
            }
            return false;
        }
    }
}
