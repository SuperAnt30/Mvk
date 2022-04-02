using MvkServer.Glm;

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

        /// <summary>
        /// Позволяет изменять углы после рендеринга блока
        /// </summary>
        public void PostRender(float scale)
        {
            if (!IsHidden)
            {
                if (!compiled) CompileDisplayList();

                bool rotation = RotationPointX == 0f && RotationPointY == 0f && RotationPointZ == 0f;

                if (RotateAngleX == 0f && RotateAngleY == 0f && RotateAngleZ == 0f)
                {
                    if (!rotation) GLRender.Translate(RotationPointX * scale, RotationPointY * scale, RotationPointZ * scale);
                }
                else
                {
                    if (!rotation) GLRender.Translate(RotationPointX * scale, RotationPointY * scale, RotationPointZ * scale);
                    if (RotateAngleZ != 0f) GLRender.Rotate(glm.degrees(RotateAngleZ), 0, 0, 1);
                    if (RotateAngleY != 0f) GLRender.Rotate(glm.degrees(RotateAngleY), 0, 1, 0);
                    if (RotateAngleX != 0f) GLRender.Rotate(glm.degrees(RotateAngleX), 1, 0, 0);
                }
            }
        }
    }
}
