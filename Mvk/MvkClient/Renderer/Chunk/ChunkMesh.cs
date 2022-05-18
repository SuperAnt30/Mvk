using MvkClient.Util;
using SharpGL;
using System;

namespace MvkClient.Renderer.Chunk
{
    /// <summary>
    /// Объект сетки чанка
    /// </summary>
    public class ChunkMesh : IDisposable
    {
        /// <summary>
        /// Пометка изменения
        /// </summary>
        public bool IsModifiedRender { get; private set; } = false;
        /// <summary>
        /// Статус обработки сетки
        /// </summary>
        public StatusMesh Status { get; private set; } = StatusMesh.Null;

        private OpenGL gl;
        private readonly uint[] vao = new uint[1];
        private readonly uint[] vbo = new uint[1];
        /// <summary>
        /// Количество float на одну вершину
        /// </summary>
        private readonly int vertexSize = 7;
        /// <summary>
        /// Количество float в буфере на один полигон
        /// </summary>
        private readonly int poligonFloat = 21;
        /// <summary>
        /// Количество вершин
        /// </summary>
        private int countVertices = 0;
        /// <summary>
        /// Количество полигонов
        /// </summary>
        private int countPoligon = 0;
        /// <summary>
        /// Создавался ли объект BindBufferNew
        /// </summary>
        private bool empty = true;
        /// <summary>
        /// Объект буфера
        /// </summary>
        private BufferData bufferData;

        public ChunkMesh()
        {
            gl = GLWindow.gl;
            bufferData = new BufferData();
        }

        /// <summary>
        /// Изменить статус на рендеринг
        /// </summary>
        public void StatusRendering()
        {
            IsModifiedRender = false;
            Status = StatusMesh.Rendering;
        }
        /// <summary>
        /// Изменить статус отменить рендеринг
        /// </summary>
        public void NotRendering() => IsModifiedRender = false;
        /// <summary>
        /// Пометка псевдо чанка для рендера
        /// </summary>
        public void SetModifiedRender() => IsModifiedRender = true;
        

        /// <summary>
        /// Буфер внесён
        /// </summary>
        public void SetBuffer(byte[] buffer)
        {
            countPoligon = buffer.Length / poligonFloat;
            countVertices = buffer.Length / vertexSize;
            //ByteBuffer byteBuffer = new ByteBuffer();
            //byteBuffer.ArrayFloat(buffer);
            //bufferData.ConvertByte(byteBuffer.ToArray());
            bufferData.ConvertByte(buffer);
            Status = StatusMesh.Binding;
        }

        /// <summary>
        /// Занести буфер в OpenGL 
        /// </summary>
        public void BindBuffer()
        {
            if (bufferData.body && countPoligon > 0)
            {
                if (!empty) BindBufferReload();
                else BindBufferNew();
                bufferData.Free();
                Status = StatusMesh.Wait;
            }
            else
            {
                Delete();
            }
        }

        private void BindBufferNew()
        {
            gl.GenVertexArrays(1, vao);
            gl.BindVertexArray(vao[0]);
            gl.GenBuffers(1, vbo);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vbo[0]);

            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, bufferData.size, bufferData.data, OpenGL.GL_STATIC_DRAW);
            int stride = 28;//  vertexSize * sizeof(float);

            EnableVertex(0, 3, OpenGL.GL_FLOAT, stride, 0);
            EnableVertex(1, 2, OpenGL.GL_FLOAT, stride, 12);
            EnableVertexI(2, 1, OpenGL.GL_INT, stride, 20);
            EnableVertexI(3, 1, OpenGL.GL_INT, stride, 24);
            //EnableVertexI(2, 1, OpenGL.GL_BYTE, stride, 20);
            //EnableVertexI(3, 1, OpenGL.GL_BYTE, stride, 21);
            //EnableVertexI(4, 1, OpenGL.GL_BYTE, stride, 22);
            //EnableVertexI(5, 1, OpenGL.GL_BYTE, stride, 23);
            //    EnableVertexI(6, 1, OpenGL.GL_BYTE, stride, 24);

            gl.BindVertexArray(0);
            empty = false;
        }

        /// <summary>
        /// Внести атрибуту в сетку
        /// </summary>
        /// <param name="i">номер атрибуты начинается с 0</param>
        /// <param name="size">количество переменный OpenGL.GL_FLOAT</param>
        /// <param name="type">тип</param>
        /// <param name="stride">максимальное количества байт на все атрибуты вершины</param>
        /// <param name="offset">откуда начинается значение в массиве в байтах</param>
        private void EnableVertex(uint i, int size, uint type, int stride, int offset)
        {
            gl.VertexAttribPointer(i, size, type, false, stride, new IntPtr(offset));
            gl.EnableVertexAttribArray(i);
        }

        private void EnableVertexI(uint i, int size, uint type, int stride, int offset)
        {
            gl.VertexAttribIPointer(i, size, type, stride, new IntPtr(offset));
            gl.EnableVertexAttribArray(i);
        }


        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        private void BindBufferReload()
        {
            gl.BindVertexArray(vao[0]);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vbo[0]);
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, bufferData.size, bufferData.data, OpenGL.GL_STATIC_DRAW);
        }

        /// <summary>
        /// Прорисовать
        /// </summary>
        public void Draw()
        {
            if (!empty && countPoligon > 0)
            {
                Debug.CountPoligon += countPoligon;
                Debug.CountMesh++;
                gl.BindVertexArray(vao[0]);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, countVertices);
                gl.BindVertexArray(0);
            }
        }

        /// <summary>
        /// Удаление если объект удалиться сам
        /// </summary>
        public void Dispose() => Delete();

        /// <summary>
        /// Удалить
        /// </summary>
        public void Delete()
        {
            countPoligon = 0;
            if (!empty)
            {
                gl.DeleteVertexArrays(1, vao);
                gl.DeleteBuffers(1, vbo);
                empty = true;
            }
            Status = StatusMesh.Null;
            bufferData.Free();
        }

        /// <summary>
        /// Статус обработки сетки
        /// </summary>
        public enum StatusMesh
        {
            /// <summary>
            /// Пустой
            /// </summary>
            Null,
            /// <summary>
            /// Ждём
            /// </summary>
            Wait,
            /// <summary>
            /// Процесс рендеринга
            /// </summary>
            Rendering,
            /// <summary>
            /// Процесс связывания сетки с OpenGL
            /// </summary>
            Binding
        }
    }
}
