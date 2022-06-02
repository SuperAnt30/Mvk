using MvkClient.Util;
using MvkServer.Util;
using SharpGL;
using System.Drawing;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Объект работы текстуры для освещения
    /// </summary>
    public class TextureLightMap
    {
        private uint locationLightMap = 0;
        private Bitmap bitmap = new Bitmap(16, 16);
        private float skyLightPrev = -1f;

        public void Update(float skyLight)
        {
            if (skyLightPrev != skyLight)
            {
                skyLightPrev = skyLight;
                for (int x = 0; x < 16; x++)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        float b = x / 15f;
                        float s = Mth.Min(y / 15f, skyLight);
                        s = (1f - b) * (1f - s);
                        s = s * .8f;
                        int l = 255 - (int)(s * 255f);
                        bitmap.SetPixel(x, y, Color.FromArgb(255, l, l, l));
                    }
                }

                //bitmap.Save("LightMap.png", System.Drawing.Imaging.ImageFormat.Png);
                UpdateLightmap();
            }
        }

        private void UpdateLightmap()
        {
            bool isCreate = locationLightMap == 0;
            if (isCreate)
            {
                uint[] texture = new uint[1];
                GLWindow.gl.GenTextures(1, texture);
                locationLightMap = texture[0];
            }
            GLWindow.gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            GLWindow.gl.BindTexture(OpenGL.GL_TEXTURE_2D, locationLightMap);
            GLWindow.gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA, 16, 16,
                0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, BufferedImage.BitmapToByteArray(bitmap));
            if (isCreate)
            {
                GLWindow.gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
                GLWindow.gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
                GLWindow.gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP);
                GLWindow.gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP);
            }
            GLWindow.gl.ActiveTexture(OpenGL.GL_TEXTURE0);
        }
    }
}
