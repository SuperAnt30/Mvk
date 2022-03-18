using MvkServer.Glm;

namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Рендер модели
    /// </summary>
    public class ModelRender : RenderDL
    {
        public float RotateAngleX { get; set; } = 0;
        public float RotateAngleY { get; set; } = 0;
        public float RotateAngleZ { get; set; } = 0;

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

        protected override void ToListCall()
        {
            if (RotateAngleX == 0f && RotateAngleY == 0f && RotateAngleZ == 0f)
            {
                base.ToListCall();
            }
            else
            {
                GLWindow.gl.PushMatrix();
                GLWindow.gl.Translate(RotationPointX * scale, RotationPointY * scale, RotationPointZ * scale);
                if (RotateAngleZ != 0f) GLWindow.gl.Rotate(glm.degrees(RotateAngleZ), 0, 0, 1);
                if (RotateAngleY != 0f) GLWindow.gl.Rotate(glm.degrees(RotateAngleY), 0, 1, 0);
                if (RotateAngleX != 0f) GLWindow.gl.Rotate(glm.degrees(RotateAngleX), 1, 0, 0);
                GLRender.ListCall(dList);
                GLWindow.gl.PopMatrix();
            }
        }

        protected override void DoRender() => box.Render(scale);

        public void Mirror() => IsMirror = true;
    }
}
