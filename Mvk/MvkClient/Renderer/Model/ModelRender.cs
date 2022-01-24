using MvkServer.Glm;

namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Рендер модели
    /// </summary>
    public class ModelRender
    {
        /// <summary>
        /// Виден ли элемент
        /// </summary>
        public bool IsHidden { get; set; } = false;

        public float RotateAngleX { get; set; } = 0;
        public float RotateAngleY { get; set; } = 0;
        public float RotateAngleZ { get; set; } = 0;

        public float RotationPointX { get; set; } = 0;
        public float RotationPointY { get; set; } = 0;
        public float RotationPointZ { get; set; } = 0;
        /// <summary>
        /// Зеркальный
        /// </summary>
        public bool IsMirror { get; protected set; } = false;

        private ModelBase model;
        private ModelBox box;
        private uint dList;
        private bool compiled = false;
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

        public ModelRender SetBox(float x, float y, float z, int w, int h, int d)
        {
            box = new ModelBox(model.TextureSize, textureOffsetX, textureOffsetY, x, y, z, w, h, d);
            return this;
        }

        public void Render(float scale)
        {
            if (!IsHidden)
            {
                if (!compiled) CompileDisplayList(scale);

                if (RotateAngleX == 0f && RotateAngleY == 0f && RotateAngleZ == 0f)
                {
                    if (RotationPointX == 0f && RotationPointY == 0f && RotationPointZ == 0f)
                    {
                        GLRender.ListCall(dList);
                    } else
                    {
                        GLWindow.gl.Translate(RotationPointX * scale, RotationPointY * scale, RotationPointZ * scale);
                        GLRender.ListCall(dList);
                        GLWindow.gl.Translate(-RotationPointX * scale, -RotationPointY * scale, -RotationPointZ * scale);
                    }
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
                    //GLRender.ListCall(dList);
                }
                GLRender.ListDelete(dList);
                compiled = false;
            }
        }

        public void SetRotationPoint(float x, float y, float z)
        {
            RotationPointX = x;
            RotationPointY = y;
            RotationPointZ = z;
        }

        private void CompileDisplayList(float scale)
        {
            dList = GLRender.ListBegin();
            box.Render(scale);
            GLRender.ListEnd();
            compiled = true;
        }

        public void Mirror() => IsMirror = true;
    }
}
