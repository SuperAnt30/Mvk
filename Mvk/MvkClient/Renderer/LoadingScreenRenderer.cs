using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Renderer
{
    public class LoadingScreenRenderer
    {

        /// <summary>
        /// Максимальное значение элементов загрузки
        /// </summary>
        protected int max = 1;
        /// <summary>
        /// Сколько элементов загруженно
        /// </summary>
        protected int value = 0;
        /// <summary>
        /// Графический лист фона
        /// </summary>
        protected uint listBg;
        /// <summary>
        /// Графический лист процесса
        /// </summary>
        protected uint listPr;
        /// <summary>
        /// Загрузка идёт ли
        /// </summary>
        protected bool isLoading = true;

        public void Start(int max)
        {
            this.max = max;
            Resized();
        }

        /// <summary>
        /// Следующий шаг загрузки
        /// </summary>
        /// <returns>true - идёт загрузка, false - загрузка закончена</returns>
        public bool Next()
        {
            value++;
            RenderPr();
            if (value >= max)
            {
                isLoading = false;
                GLRender.ListDelete(listBg);
            }
            return isLoading;
        }

        public void Resized()
        {
            RenderBg();
            RenderPr();
        }

        /// <summary>
        /// Прорисовка
        /// </summary>
        public void Draw()
        {
            GLRender.ListCall(listBg);
            GLRender.ListCall(listPr);
        }

        /// <summary>
        /// Генерация фона
        /// </summary>
        protected void RenderBg()
        {
            GLRender.ListDelete(listBg);
            listBg = GLRender.ListBegin();

            int w = GLWindow.WindowWidth;
            int wc = w / 2;
            int h = GLWindow.WindowHeight;
            OpenGL gl = GLWindow.gl;

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(0, w, h, 0);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
            gl.Disable(OpenGL.GL_TEXTURE_2D);

            gl.PushMatrix();

            vec4 colBg = new vec4(0.8f, 0.8f, 0.78f, 1f);
            vec4 colPr = new vec4(0.7f, 0f, 0f, 1f);
            // фон
            GLRender.Rectangle(0, 0, w, h, colBg);
            // процесс
            GLRender.Rectangle(wc - 208, h - 140, wc + 208, h - 100, colPr);

            gl.PopMatrix();

            gl.EndList();
        }

        /// <summary>
        /// Генерация процесса
        /// </summary>
        protected void RenderPr()
        {
            GLRender.ListDelete(listPr);
            listPr = GLRender.ListBegin();

            int w = GLWindow.WindowWidth;
            int wc = w / 2;
            int h = GLWindow.WindowHeight;
            OpenGL gl = GLWindow.gl;

            gl.PushMatrix();

            vec4 colBg = new vec4(0.8f, 0.8f, 0.78f, 1f);
            vec4 colPr = new vec4(0.7f, 0f, 0f, 1f);
            int wcl = value * 400 / max;
            // процесс
            GLRender.Rectangle(wc - 204, h - 136, wc + 204, h - 104, colBg);
            GLRender.Rectangle(wc - 200, h - 132, wc - 200 + wcl, h - 108, colPr);

            gl.PopMatrix();

            gl.EndList();
        }
    }
}
