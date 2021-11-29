using MvkClient.Audio;
using MvkServer;
using SharpGL;
using System;

namespace MvkClient
{
    public class Client
    {
        public static void TestOpenGL(OpenGL gl)
        {

            gl.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
        }
        public static void TestOpenGLDraw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);

            uint index = gl.GenLists(1);
            gl.NewList(index, OpenGL.GL_COMPILE);

            gl.Begin(OpenGL.GL_TRIANGLES);
            gl.Color(1.0f, 1.0f, 1.0f, 0.5f);
            gl.Vertex(1f, 1f, 0);
            gl.Color(1.0f, 0f, 1.0f, 0.5f);
            gl.Vertex(0, 0, 0);
            gl.Color(0f, 1.0f, 1.0f, 0.5f);
            gl.Vertex(1f, 0, 0);
            gl.End();

            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(1.0f, 1.0f, 1.0f, 0.5f);
            gl.Vertex(1f, 1f, 0);
            gl.Vertex(0, 1f, 0);
            gl.Color(1.0f, 0f, 1.0f, 0.5f);
            gl.Vertex(0, 0, 0);
            gl.Color(0f, 1.0f, 1.0f, 0.5f);
            gl.Vertex(1f, 0, 0);

            gl.End();

            gl.LineWidth(3.0f);
            gl.Begin(OpenGL.GL_LINES);
            gl.Color(1.0f, 0, 0);
            gl.Vertex(-1f, -1f, 0);
            gl.Vertex(0, 0, 0);
            gl.End();

            gl.EndList();

            gl.CallList(index);

        }
        public static void TestSound()
        {
            // Инициализация звука
            IntPtr pDevice = Al.alcOpenDevice(null);
            IntPtr pContext = Al.alcCreateContext(pDevice, null);
            Al.alcMakeContextCurrent(pContext);

            AudioSample sample = new AudioSample();
            sample.LoadOgg(Server.TEST);
            Al.alGenSources(1, out uint sid);
            int errorCode = Al.alGetError();
            // По ошибке определяем источник и буфер обмена
            if (errorCode == 0)
            {
                Al.alGenBuffers(1, out uint bid);
                errorCode = Al.alGetError();
                if (errorCode == 0)
                {
                    // Всё норм
                    Al.alBufferData(bid, sample.AlFormat, sample.Buffer, sample.Size, sample.SamplesPerSecond);
                    Al.alSourcei(sid, Al.AL_BUFFER, (int)bid);

                    Al.alSourcef(sid, Al.AL_PITCH, 1.0f);
                    Al.alSourcef(sid, Al.AL_GAIN, 1.0f);
                    Al.alSource3f(sid, Al.AL_POSITION, 0, 0, 0);
                    Al.alSourcePlay(sid);
                }
            }
        }
    }
}
