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
            GLRender.SymbolRender(symbol);
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

           // uint list = GLRender.ListBegin();
            foreach (string str in strs)
            {
                RenderString(x, y + h, color, str, size);
                h += FontAdvance.VertAdvance[(int)size] + 4;
            }

            //GLRender.ListEnd();
            //return list;
        }

        /// <summary>
        /// Прорисовка строки
        /// </summary>
        public static void RenderString(float x, float y, vec4 color, string text, FontSize size)
        {
            char[] vc = text.ToCharArray();
            int w = 0;
            for (int i = 0; i < vc.Length; i++)
            {
                GLRender.PushMatrix(color, new vec3(x + w, y, 0));
                int w0 = RenderChar(vc[i], (int)size);
                if (w0 > 0) w += w0 + Assets.StepFont(size);
                GLRender.PopMatrix();
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
