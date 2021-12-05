using MvkServer.Glm;
using System;

namespace MvkClient.Renderer.Font
{
    public class FontRenderer
    {
        /// <summary>
        /// Прорисовка символа
        /// </summary>
        protected static int RenderChar(char letter)
        {
            Symbol symbol = FontAdvance.Get(letter);
            if (symbol == null) return 0;
            GLRender.SymbolRender(symbol);
            return symbol.Width;
        }

        /// <summary>
        /// Узнать ширину символа
        /// </summary>
        protected static int WidthChar(char letter)
        {
            Symbol symbol = FontAdvance.Get(letter);
            if (symbol == null) return 0;
            return symbol.Width;
        }

        /// <summary>
        /// Прорисовка текста
        /// </summary>
        public static uint RenderText(float x, float y, vec4 color, string text)
        {
            string[] stringSeparators = new string[] { "\r\n" };
            string[] strs = text.Split(stringSeparators, StringSplitOptions.None);
            int h = 0;

            uint list = GLRender.ListBegin();
            foreach (string str in strs)
            {
                RenderString(x, y + h, color, str);
                //char[] vc = str.ToCharArray();
                //int w = 0;
                //for (int i = 0; i < vc.Length; i++)
                //{
                //    GLRender.PushMatrix(color, new vec3(x + w, y + h, 0));
                //    int w0 = RenderChar(vc[i]);
                //    if (w0 > 0) w += w0 + 2;
                //    GLRender.PopMatrix();
                //}
                h += FontAdvance.VertAdvance + 4;
            }

            GLRender.ListEnd();
            return list;
        }

        /// <summary>
        /// Прорисовка строки
        /// </summary>
        public static void RenderString(float x, float y, vec4 color, string text)
        {
            char[] vc = text.ToCharArray();
            int w = 0;
            for (int i = 0; i < vc.Length; i++)
            {
                GLRender.PushMatrix(color, new vec3(x + w, y, 0));
                int w0 = RenderChar(vc[i]);
                if (w0 > 0) w += w0 + 2;
                GLRender.PopMatrix();
            }
        }

        /// <summary>
        /// Узнать ширину текста
        /// </summary>
        public static int WidthString(string text)
        {
            char[] vc = text.ToCharArray();
            int w = 0;
            for (int i = 0; i < vc.Length; i++)
            {
                int w0 = WidthChar(vc[i]);
                if (w0 > 0) w += w0 + 2;
                GLRender.PopMatrix();
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
