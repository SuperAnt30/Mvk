//using MvkServer.Glm;
//using SharpGL;

//namespace MvkClient.Renderer
//{
//    /// <summary>
//    /// Рендер прицела курсор
//    /// </summary>
//    public class RenderAim : RenderDL
//    {
//        private int width;
//        private int height;

//        public void Render(int width, int height)
//        {
//            this.width = width;
//            this.height = height;
//            Render();
//        }

//        protected override void DoRender()
//        {
//            GLRender.Texture2DDisable();
//            GLRender.LineWidth(2f);
//            GLRender.Color(new vec4(1, 1, 1, .8F));
//            GLRender.Begin(OpenGL.GL_LINES);
//            GLRender.Vertex(0, -8f, 0);
//            GLRender.Vertex(0, 8f, 0);
//            GLRender.Vertex(-8f, 0, 0);
//            GLRender.Vertex(8f, 0, 0);
//            GLRender.End();
//        }

//        protected override void ToListCall()
//        {
//            GLRender.PushMatrix();
//            GLWindow.gl.Translate(width / 2, height / 2, 0);
//            GLRender.ListCall(dList);
//            GLRender.PopMatrix();
//        }

        
//    }
//}
