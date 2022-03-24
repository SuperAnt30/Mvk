using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Рендер display list
    /// </summary>
    public class RenderDL
    {
        /// <summary>
        /// Виден ли элемент
        /// </summary>
        public bool IsHidden { get; set; } = false;

        public float RotationPointX { get; set; } = 0;
        public float RotationPointY { get; set; } = 0;
        public float RotationPointZ { get; set; } = 0;

        public float RotateAngleX { get; set; } = 0;
        public float RotateAngleY { get; set; } = 0;
        public float RotateAngleZ { get; set; } = 0;

        protected float scale = 1.0f;
        protected uint dList;
        protected bool compiled = false;

        public RenderDL() { }

        public RenderDL(float scale)
        {
            this.scale = scale;
        }

        public void Render()
        {
            if (DoCompiled()) ToListCall();
        }

        protected virtual void ToListCall()
        {
            bool rotation = RotationPointX == 0f && RotationPointY == 0f && RotationPointZ == 0f;

            if (RotateAngleX == 0f && RotateAngleY == 0f && RotateAngleZ == 0f)
            {
                if (!rotation)
                {
                    GLRender.PushMatrix();
                    GLRender.Translate(RotationPointX * scale, RotationPointY * scale, RotationPointZ * scale);
                }
                GLRender.ListCall(dList);
                if (!rotation) GLRender.PopMatrix();
            }
            else
            {
                GLRender.PushMatrix();
                if (!rotation) GLRender.Translate(RotationPointX * scale, RotationPointY * scale, RotationPointZ * scale);
                if (RotateAngleZ != 0f) GLRender.Rotate(glm.degrees(RotateAngleZ), 0, 0, 1);
                if (RotateAngleY != 0f) GLRender.Rotate(glm.degrees(RotateAngleY), 0, 1, 0);
                if (RotateAngleX != 0f) GLRender.Rotate(glm.degrees(RotateAngleX), 1, 0, 0);
                GLRender.ListCall(dList);
                GLRender.PopMatrix();
            }
        }

        protected bool DoCompiled()
        {
            if (!IsHidden)
            {
                if (!compiled)
                {
                    CompileDisplayList();
                }
                return true;
            }
            return false;
        }

        private void CompileDisplayList()
        {
            dList = GLRender.ListBegin();
            DoRender();
            GLRender.ListEnd();
            compiled = true;
        }

        protected virtual void DoRender() { }

        public void SetRotationPoint(float x, float y, float z)
        {
            RotationPointX = x;
            RotationPointY = y;
            RotationPointZ = z;
        }

        public void MatrixOrtho2d(int width, int height)
        {
            GLWindow.gl.MatrixMode(OpenGL.GL_PROJECTION);
            GLWindow.gl.LoadIdentity();
            //GLWindow.gl.Ortho2D(0, width, height, 0);
            GLWindow.gl.Ortho(0, width, height, 0, -100, 100);
            GLWindow.gl.MatrixMode(OpenGL.GL_MODELVIEW);
            GLWindow.gl.LoadIdentity();
            GLRender.CullEnable();
        }

    }
}
