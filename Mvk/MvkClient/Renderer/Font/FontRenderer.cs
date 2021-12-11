using MvkAssets;
using MvkServer.Glm;
using System;

namespace MvkClient.Renderer.Font
{
    public class FontRenderer
    {
        /// <summary>
        /// Прорисовка символа
        /// </summary>
        protected static int RenderChar(char letter, int size)
        {
            Symbol symbol = FontAdvance.Get(letter, size);
            if (symbol == null) return 0;
            symbol.Draw();
            return symbol.Width;
        }

        /// <summary>
        /// Узнать ширину символа
        /// </summary>
        protected static int WidthChar(char letter, int size)
        {
            Symbol symbol = FontAdvance.Get(letter, size);
            if (symbol == null) return 0;
            return symbol.Width;
        }

        /// <summary>
        /// Прорисовка текста
        /// </summary>
        public static void RenderText(float x, float y, vec4 color, string text, FontSize size)
        {
            string[] stringSeparators = new string[] { "\r\n" };
            string[] strs = text.Split(stringSeparators, StringSplitOptions.None);
            int h = 0;

            GLWindow.gl.Color(color.x, color.y, color.z, 1f);
            foreach (string str in strs)
            {
                RenderString(x, y + h, new vec4(), str, size, false);
                h += FontAdvance.VertAdvance[(int)size] + 4;
            }
        }

        /// <summary>
        /// Прорисовка строки
        /// </summary>
        public static void RenderString(float x, float y, vec4 color, string text, FontSize size) 
            => RenderString(x, y, color, text, size, true);

        protected static void RenderString(float x, float y, vec4 color, string text, FontSize size, bool isColor)
        {
            char[] vc = text.ToCharArray();
            int w = 0;

            if (isColor) GLWindow.gl.Color(color.x, color.y, color.z, 1f); 
            for (int i = 0; i < vc.Length; i++)
            {
                GLWindow.gl.PushMatrix();
                GLWindow.gl.Translate(x + w, y, 0);
                int w0 = RenderChar(vc[i], (int)size);
                if (w0 > 0) w += w0 + Assets.StepFont(size);
                GLWindow.gl.PopMatrix();
            }
        }

        /// <summary>
        /// Узнать ширину текста
        /// </summary>
        public static int WidthString(string text, FontSize size)
        {
            char[] vc = text.ToCharArray();
            int w = 0;
            for (int i = 0; i < vc.Length; i++)
            {
                int w0 = WidthChar(vc[i], (int)size);
                if (w0 > 0) w += w0 + Assets.StepFont(size);
            }
            return w;
        }

        /// <summary>
        /// Прорисовка
        /// </summary>
        /// <param name="list">ключ листа</param>
        public static void Draw(uint list) => GLRender.ListCall(list);

    }
}
