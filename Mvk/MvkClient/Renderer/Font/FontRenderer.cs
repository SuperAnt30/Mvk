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
        /// Прорисовка текста
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        /// <param name="text"></param>
        public static uint RenderString(float x, float y, vec4 color, string text)
        {
            string[] stringSeparators = new string[] { "\r\n" };
            string[] strs = text.Split(stringSeparators, StringSplitOptions.None);
            int h = 0;

            uint list = GLRender.ListBegin();
            foreach (string str in strs)
            {
                char[] vc = str.ToCharArray();
                int w = 0;
                for (int i = 0; i < vc.Length; i++)
                {
                    GLRender.PushMatrix(color, new vec3(x + w, y + h, 0));
                    int w0 = RenderChar(vc[i]);
                    if (w0 > 0) w += w0 + 2;
                    GLRender.PopMatrix();
                }
                h += FontAdvance.VertAdvance + 4;
            }

            GLRender.ListEnd();
            return list;
        }

        /// <summary>
        /// Прорисовка
        /// </summary>
        /// <param name="list">ключ листа</param>
        public static void Draw(uint list) => GLRender.ListCall(list);

    }
}
