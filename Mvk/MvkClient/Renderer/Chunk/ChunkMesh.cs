namespace MvkClient.Renderer.Chunk
{
    /// <summary>
    /// Объект сетки чанка
    /// </summary>
    public class ChunkMesh : MeshBuffer
    {
        /// <summary>
        /// Буфер точки, точка xyz, текстура точки uv, цвет точки rgba
        /// </summary>
        protected override int[] attrs { get; } = new int[] { 3, 2, 3, 2 };

        /// <summary>
        /// Пометка изменения
        /// </summary>
        public bool IsModifiedRender { get; private set; } = true;
        /// <summary>
        /// Пометка псевдо чанка для рендера
        /// </summary>
        public void SetModifiedRender() => IsModifiedRender = true;
        /// <summary>
        /// Массив буфера сетки
        /// </summary>
        public float[] Buffer { get; protected set; }
        /// <summary>
        /// Статус обработки сетки
        /// </summary>
        public StatusMesh Status { get; private set; } = StatusMesh.Null;

        /// <summary>
        /// Изменить статус на рендеринг
        /// </summary>
        public void StatusRendering()
        {
            IsModifiedRender = false;
            Status = StatusMesh.Rendering;
        }
        /// <summary>
        /// Изменить статус отменить рендеринг
        /// </summary>
        public void NotRendering() => IsModifiedRender = false;

        /// <summary>
        /// Удалить
        /// </summary>
        public override void Delete()
        {
            base.Delete();
            Status = StatusMesh.Null;
            Buffer = null;
        }

        /// <summary>
        /// Буфер внесён
        /// </summary>
        public void SetBuffer(float[] buffer)
        {
            Buffer = buffer;
            Status = StatusMesh.Binding;
        }

        /// <summary>
        /// Занести буфер в OpenGL 
        /// </summary>
        public bool BindBuffer()
        {
            if (Buffer != null)// && !IsRendering && IsBinding)
            {
                BindBuffer(Buffer);
                Buffer = null;
                Status = StatusMesh.Wait;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Статус обработки сетки
        /// </summary>
        public enum StatusMesh
        {
            /// <summary>
            /// Пустой
            /// </summary>
            Null,
            /// <summary>
            /// Ждём
            /// </summary>
            Wait,
            /// <summary>
            /// Процесс рендеринга
            /// </summary>
            Rendering,
            /// <summary>
            /// Процесс связывания сетки с OpenGL
            /// </summary>
            Binding
        }
    }
}
