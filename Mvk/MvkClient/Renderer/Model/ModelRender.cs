namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Рендер модели
    /// </summary>
    public class ModelRender : RenderDL
    {
        /// <summary>
        /// Зеркальный
        /// </summary>
        public bool IsMirror { get; protected set; } = false;

        private ModelBase model;
        private ModelBox box;

        /// <summary>
        /// Смещение X в текстуре, используемой для отображения этой модели. 
        /// </summary>
        private readonly int textureOffsetX;
        /// <summary>
        /// Смещение Y в текстуре, используемой для отображения этой модели. 
        /// </summary>
        private readonly int textureOffsetY;

        public ModelRender(ModelBase model, int u, int v)
        {
            this.model = model;
            textureOffsetX = u;
            textureOffsetY = v;
        }

        public ModelRender SetBox(float x, float y, float z, int w, int h, int d, float scaleFactor)
        {
            box = new ModelBox(model.TextureSize, textureOffsetX, textureOffsetY, x, y, z, w, h, d, scaleFactor, IsMirror);
            return this;
        }

        public void Render(float scale)
        {
            this.scale = scale;
            Render();
        }

        protected override void DoRender() => box.Render(scale);

        public void Mirror() => IsMirror = true;
    }
}
