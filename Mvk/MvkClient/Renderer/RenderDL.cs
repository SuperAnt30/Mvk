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
            if (RotationPointX == 0f && RotationPointY == 0f && RotationPointZ == 0f)
            {
                GLRender.ListCall(dList);
            }
            else
            {
                GLWindow.gl.Translate(RotationPointX * scale, RotationPointY * scale, RotationPointZ * scale);
                GLRender.ListCall(dList);
                GLWindow.gl.Translate(-RotationPointX * scale, -RotationPointY * scale, -RotationPointZ * scale);
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
            GLWindow.gl.Ortho2D(0, width, height, 0);
            GLWindow.gl.MatrixMode(OpenGL.GL_MODELVIEW);
            GLWindow.gl.LoadIdentity();
        }

    }
}
